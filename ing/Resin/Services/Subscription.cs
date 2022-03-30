using Resin.Grpc;
using System.Collections.Concurrent;

using static Resin.Services.Swallower;

namespace Resin.Services;

internal sealed class Subscription : IDisposable
{
    private readonly ConcurrentDictionary<Guid, Func<IsgEvent, bool>> clients = new();
    private readonly SemaphoreSlim positionLock = new(1, 1);
    private readonly SubsSource source;
    private readonly ILogger<Subscription> log;
    private ulong? subPosition;
    private CancellationTokenSource subTokenSource = new();

    internal Subscription(SubsSource source, ILogger<Subscription> log)
    {
        this.source = source;
        this.log = log;

        log.LogInformation("Created subscription for stream {0}", source.Stream);
    }

    public void Dispose()
    {
        if (subTokenSource.IsCancellationRequested)
        {
            return;
        }

        log.LogInformation("Disposing of subscription {0}", source.Stream);

        Swallow(subTokenSource.Cancel, log);
        source.Dispose();
        positionLock.Dispose();
        subTokenSource.Dispose();
    }

    public async Task<(bool, CancellationToken)> TryAddClient(
        Guid clientId,
        Func<IsgEvent, bool> handler,
        ulong? clientPosition)
    {
        try
        {
            log.LogInformation("Trying to add client {0} to {1} at {2}", clientId, source.Stream, clientPosition.Describe());

            await positionLock.WaitAsync(subTokenSource.Token).ConfigureAwait(false);

            if (OutOfDate(clientPosition))
            {
                log.LogInformation(
                    "Client {0} is out of date, client {1}, stream {2}",
                    clientId,
                    clientPosition.Describe(),
                    subPosition.Describe());
                return (false, subTokenSource.Token);
            }

            if (!clients.TryAdd(clientId, handler))
            {
                log.LogInformation("Failed to add client {0} to client list", clientId);
                return (false, subTokenSource.Token);
            }

            if (clients.Count == 1)
            {
                subPosition = clientPosition;
                log.LogInformation("Setting subscription position for steam {0} to {1}", source.Stream, subPosition.Describe());
            }

            log.LogInformation("Client {0} added to {1} subscription", clientId, source.Stream);

            await source.Start(OnArrived, OnDropped, subPosition).ConfigureAwait(false);

            return (true, subTokenSource.Token);
        }
        catch (TaskCanceledException) { return (false, subTokenSource.Token); }
        catch (OperationCanceledException) { return (false, subTokenSource.Token); }
        catch (ObjectDisposedException) { return (false, subTokenSource.Token); }
        finally
        {
            if (positionLock.CurrentCount == 0)
            {
                positionLock.Release();
            }
        }
    }

    private async Task OnArrived(IsgEvent rev)
    {
        List<Guid> deadClients = new();

        try
        {
            await positionLock.WaitAsync(subTokenSource.Token).ConfigureAwait(false);
            subPosition = rev.Position;

            foreach (var (id, handler) in clients)
            {
                try
                {
                    if (!handler(rev))
                    {
                        deadClients.Add(id);
                    }
                }
                catch (Exception)
                {
                    deadClients.Add(id);
                }
            }

            foreach (var corpse in deadClients)
            {
                log.LogWarning(null, "Client {0} has died, removing", corpse);
                clients.TryRemove(corpse, out _);
            }

            if (clients.IsEmpty)
            {
                log.LogWarning(null, "No more clients left in subscription to {0}, stopping source", source.Stream);
                await source.Stop().ConfigureAwait(false);
                subPosition = null;
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        finally
        {
            if (positionLock.CurrentCount == 0)
            {
                positionLock.Release();
            }
        }
    }

    private void OnDropped()
    {
        try
        {
            positionLock.Wait(subTokenSource.Token);

            log.LogWarning(null, "Resetting subscription to {0} after it was dropped", source.Stream);

            subTokenSource.Cancel();
            clients.Clear();
            subPosition = null;
            subTokenSource.Dispose();
            subTokenSource = new CancellationTokenSource();
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        finally
        {
            if (positionLock.CurrentCount == 0)
            {
                positionLock.Release();
            }
        }
    }

    private bool OutOfDate(ulong? clientPosition)
    {
        // The client is only out of date if the subscription is not at the
        // start of the stream.
        if (subPosition is not null)
        {
            // If the subscription has started but the client is still at the
            // start of the stream then the client is behind
            if (clientPosition is null)
            {
                return true;
            }

            // Otherwise compare positions to see if the subscription is ahead
            // of the client
            if (clientPosition < subPosition)
            {
                return true;
            }
        }

        return false;
    }
}
