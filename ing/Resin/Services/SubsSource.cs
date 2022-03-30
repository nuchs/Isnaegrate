using EventStore.Client;
using Resin.Grpc;

namespace Resin.Services;

internal sealed class SubsSource : IDisposable
{
    private readonly EventStoreClient esdb;
    private readonly ILogger<SubsSource> log;
    private readonly SemaphoreSlim startLock = new(1, 1);
    private readonly CancellationTokenSource subsTokenSource = new();
    private bool started;
    private StreamSubscription? subscription;

    internal SubsSource(EventStoreClient esdb, string stream, ILogger<SubsSource> log)
    {
        this.esdb = esdb;

        Stream = stream;
        this.log = log;

        log.LogInformation("Created subscription source for stream {0}", Stream);
    }

    public string Stream { get; }

    public void Dispose()
    {
        log.LogInformation("Disposing of source for subscription to {0}", Stream);

        if (subsTokenSource.IsCancellationRequested)
        {
            return;
        }

        subsTokenSource.Cancel();
        startLock.Dispose();
        subsTokenSource.Dispose();
        subscription?.Dispose();
    }

    public async Task Start(
        Func<IsgEvent, Task> onNew,
        Action onDrop,
        ulong? position)
    {
        if (started)
            return;

        try
        {
            log.LogInformation("Starting source for subscription to {0}", Stream);

            await startLock.WaitAsync(subsTokenSource.Token).ConfigureAwait(false);

            if (!started)
            {
                await BeginSubscription(onNew, onDrop, position).ConfigureAwait(false);
                started = true;
            }

            log.LogInformation("Source for subscription to {0} started", Stream);
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        finally
        {
            if (startLock.CurrentCount == 0)
            {
                startLock.Release();
            }
        }
    }

    public async Task Stop()
    {
        if (!started)
            return;

        try
        {
            log.LogInformation("Stopping source for subscription to {0}", Stream);

            await startLock.WaitAsync(subsTokenSource.Token).ConfigureAwait(false);

            if (started)
            {
                started = false;
                subscription?.Dispose();
            }

            log.LogInformation("Source for subscription to {0} stopped", Stream);
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        finally
        {
            if (startLock.CurrentCount == 0)
            {
                startLock.Release();
            }
        }
    }

    private async Task BeginSubscription(
        Func<IsgEvent, Task> onNew,
        Action onDrop,
        ulong? position)
    {
        subscription = await esdb.SubscribeToStreamAsync(
              Stream,
              position.HasValue ? FromStream.After(position.Value) : FromStream.Start,
              eventAppeared: MakeAppearedHandler(onNew),
              subscriptionDropped: MakeDroppedHandler(onDrop),
              cancellationToken: subsTokenSource.Token)
              .ConfigureAwait(false);
    }

    private Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> MakeAppearedHandler(Func<IsgEvent, Task> onNew)
    {
        return async (_, rev, cancel) =>
        {
            await onNew(rev.Event.ToIsgEvent()).ConfigureAwait(false);
        };
    }

    private Action<StreamSubscription, SubscriptionDroppedReason, Exception?> MakeDroppedHandler(Action onDrop)
    {
        return (sub, reason, exp) =>
        {
            // Disposing of the subscription will trigger the drop handler. We
            // always set one of the following flags before doing the disposal
            // to prevent infinite recursion.
            if (subsTokenSource.IsCancellationRequested || !started)
            {
                return;
            }

            log.LogWarning(exp, "Subscription {0} to {1} was dropped because: {2}", sub.SubscriptionId, Stream, reason);

            Stop().Wait();

            onDrop();
        };
    }
}
