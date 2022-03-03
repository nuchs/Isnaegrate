using EventStore.Client;
using Grpc.Core;

namespace Epoxy.Services;

public class ReaderService
{
    private readonly ILogger<ReaderService> log;
    private readonly EventStoreClient esdb;

    public ReaderService(EventStoreClient esdb, ILogger<ReaderService> log)
    {
        this.log= log;
        this.esdb= esdb;
    }
}
