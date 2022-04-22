using EventStore.Client;
using Resin.Grpc;
using System.Collections.Concurrent;

namespace Resin.Services;

public sealed class ConnectionManager : IDisposable
{
    private readonly EventStoreClient esdb;
    private readonly ILogger<ConnectionManager> log;
    private readonly ILogger<Subscription> subLog;
    private readonly ILogger<SubsSource> subSrcLog;
    private readonly ILogger<ClientStream> clientLog;
    private readonly ILogger<ReadSource> readSrcLog;
    private readonly CancellationTokenSource serviceToken = new();
    private readonly ConcurrentDictionary<string, Subscription> subscriptions = new();

    public ConnectionManager(EventStoreClient esdb, ILoggerFactory loggerFactory)
    {
        this.esdb = esdb;

        log = loggerFactory.CreateLogger<ConnectionManager>(); ;
        clientLog = loggerFactory.CreateLogger<ClientStream>();
        readSrcLog = loggerFactory.CreateLogger<ReadSource>();
        subLog = loggerFactory.CreateLogger<Subscription>();
        subSrcLog = loggerFactory.CreateLogger<SubsSource>();
    }

    public void Dispose()
    {
        if (serviceToken.IsCancellationRequested)
        {
            return;
        }

        log.LogInformation("Disposing of subscription manager");

        serviceToken.Cancel();

        foreach (var sub in subscriptions.Values)
        {
            sub.Dispose();
        }

        serviceToken.Dispose();
    }

    internal IAsyncEnumerable<IsgEvent> Read(string name, string stream, ulong position)
    {
        serviceToken.Token.ThrowIfCancellationRequested();

        log.LogInformation("{0} requested reader for stream {1} from position {2}", name, stream, position);

        return new ReadSource(esdb, stream, position, serviceToken.Token, readSrcLog);
    }

    internal ClientStream Subscribe(string name, string stream, ulong? position = null)
    {
        serviceToken.Token.ThrowIfCancellationRequested();

        log.LogInformation("{0} is subscribing to stream {1} at {2}", name, stream, position.Describe());

        var catchup = new ReadSource(esdb, stream, position, serviceToken.Token, readSrcLog);
        var subSource = new SubsSource(esdb, stream, subSrcLog);
        var sub = subscriptions.GetOrAdd(stream, _ => new Subscription(subSource, subLog));

        return new ClientStream(catchup, sub, name, clientLog);
    }
}
