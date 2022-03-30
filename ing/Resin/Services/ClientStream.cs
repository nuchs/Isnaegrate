using Resin.Grpc;
using System.Collections.Concurrent;

using static Resin.Services.Swallower;

namespace Resin.Services;

internal sealed class ClientStream : IAsyncEnumerable<IsgEvent>, IAsyncDisposable
{
    private readonly ConcurrentQueue<IsgEvent> buffer = new();
    private readonly ReadSource catchup;
    private readonly Task catchupTask;
    private readonly Guid clientId = Guid.NewGuid();
    private readonly ILogger<ClientStream> log;
    private readonly CancellationTokenSource streamTokenSource = new();
    private readonly Subscription subs;
    private readonly SemaphoreSlim workFlag = new(0);

    internal ClientStream(ReadSource catchup, Subscription subs, string name, ILogger<ClientStream> log)
    {
        this.catchup = catchup;
        this.subs = subs;
        this.log = log;

        log.LogInformation("Assigning client {0} id {1}", name, clientId);

        catchupTask = Task.Run(CatchUp);
    }

    public async ValueTask DisposeAsync()
    {
        log.LogInformation("Disposing of client stream {0}", clientId);

        Swallow(streamTokenSource.Cancel, log);
        await Swallow(StopCatchUp, log).ConfigureAwait(false);
        workFlag.Dispose();
        streamTokenSource.Dispose();
    }

    public async IAsyncEnumerator<IsgEvent> GetAsyncEnumerator(CancellationToken clientToken)
    {
        var token = CombineTokens(clientToken);

        while (!token.IsCancellationRequested)
        {
            await WaitForWork(token).ConfigureAwait(false);

            if (buffer.TryDequeue(out var ev))
            {
                yield return ev;
            }
        }

        Swallow(streamTokenSource.Cancel, log);
        log.LogInformation("Client stream for {0} was cancelled {1}", clientId, GetReason(clientToken));
    }

    private string GetReason(CancellationToken clientToken)
        => clientToken.IsCancellationRequested ? "by client" : "by subscription";

    public void Stop()
    {
        log.LogInformation("Stop requested for client {0}", clientId);
        Swallow(streamTokenSource.Cancel, log);
    }

    private async Task CatchUp()
    {
        try
        {
            var (success, cancel) = await subs.TryAddClient(clientId, Enqueue, catchup.Position).ConfigureAwait(false);

            while (!success)
            {
                await foreach (var isg in catchup.WithCancellation(streamTokenSource.Token).ConfigureAwait(false))
                {
                    if (!Enqueue(isg))
                    {
                        return;
                    }
                }

                (success, cancel) = await subs.TryAddClient(clientId, Enqueue, catchup.Position).ConfigureAwait(false);
            }

            cancel.Register(() => streamTokenSource.Cancel());
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Client {0} failed catchup, cancelling connection.", clientId);
            streamTokenSource.Cancel();
        }
    }

    private CancellationToken CombineTokens(CancellationToken? clientToken)
    {
        if (clientToken is null)
        {
            return streamTokenSource.Token;
        }

        return CancellationTokenSource.CreateLinkedTokenSource(streamTokenSource.Token, clientToken.Value).Token;
    }

    private bool Enqueue(IsgEvent ev)
    {
        if (streamTokenSource.Token.IsCancellationRequested)
        {
            return false;
        }

        if (catchup.Position is null || ev.Position > catchup.Position)
        {
            buffer.Enqueue(ev);
            workFlag.Release();
        }

        return true;
    }

    private Task StopCatchUp() => catchupTask;

    private async Task WaitForWork(CancellationToken token)
    {
        try
        {
            await workFlag.WaitAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.LogInformation("Workflag stopped: {0}, stopping client stream {1}", ex.GetType(), clientId);
            Swallow(streamTokenSource.Cancel, log);
        }
    }
}
