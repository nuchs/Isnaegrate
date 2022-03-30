using EventStore.Client;
using Resin.Grpc;

namespace Resin.Services;

internal sealed class ReadSource : IAsyncEnumerable<IsgEvent>
{
    private readonly EventStoreClient esdb;
    private readonly ILogger<ReadSource> log;
    private readonly CancellationToken serviceToken;
    private readonly string stream;
    private StreamPosition? position;

    internal ReadSource(EventStoreClient esdb, string stream, ulong? position, CancellationToken serviceToken, ILogger<ReadSource> log)
    {
        this.esdb = esdb;
        this.stream = stream;
        this.serviceToken = serviceToken;
        this.log = log;
        this.position = position.HasValue ? new StreamPosition(position.Value) : null;
    }

    public ulong? Position => position;

    public async IAsyncEnumerator<IsgEvent> GetAsyncEnumerator(CancellationToken clientToken = default)
    {
        log.LogInformation("Read started on {0} from {1}", stream, position.Describe());

        var cancel = CombineTokens(clientToken);
        var results = esdb.ReadStreamAsync(Direction.Forwards, stream, position ?? 0, cancellationToken: cancel);

        await foreach (var result in results.ConfigureAwait(false))
        {
            if (cancel.IsCancellationRequested)
            {
                log.LogDebug("Read cancelled");
                yield break;
            }

            yield return result.Event.ToIsgEvent();
            position = result.Event.EventNumber;
        }

        log.LogInformation("Completed read for stream {0} @ {1}", stream, position.Describe());
    }

    private CancellationToken CombineTokens(CancellationToken? clientToken)
    {
        if (clientToken is null)
        {
            return serviceToken;
        }

        return CancellationTokenSource.CreateLinkedTokenSource(serviceToken, clientToken.Value).Token;
    }
}
