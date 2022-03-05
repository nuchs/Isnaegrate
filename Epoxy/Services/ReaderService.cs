using Epoxy.Grpc;
using static Epoxy.Grpc.EpoxyHelpers;
using EventStore.Client;
using Grpc.Core;
using static Epoxy.Grpc.Reader;

namespace Epoxy.Services;

public class ReaderService :ReaderBase
{
    private readonly ILogger<ReaderService> log;
    private readonly EventStoreClient esdb;

    public ReaderService(EventStoreClient esdb, ILogger<ReaderService> log)
    {
        this.log= log;
        this.esdb= esdb;
    }

    public override Task<IsgEvent> Read(ReadRequest request, ServerCallContext context)
    {
        log.LogInformation("Some bugger wants to read {} from {}", request.Stream, request.Position);

        return Task.FromResult(NewIsgEvent("Epoxy", EventType.Test));
    }
}
