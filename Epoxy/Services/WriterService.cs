using EventStore.Client;
using Grpc.Core;

namespace Epoxy.Services;

public class WriterService
{
    private readonly ILogger<WriterService> log;
    private readonly EventStoreClient esdb;

    public WriterService(EventStoreClient esdb, ILogger<WriterService> log)
    {
        this.log = log;
        this.esdb = esdb;
    }
}
