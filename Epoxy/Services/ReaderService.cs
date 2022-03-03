using Epoxy.Grpc;
using EventStore.Client;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Isnaegrate.Grpc.Events;
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

    public override Task<IsgEvent> Read(Request request, ServerCallContext context)
    {
        log.LogInformation("Some bugger wants to read {} from {}", request.Stream, request.Position);

        var evt = new IsgEvent()
        {
            Id = new Uid() { Value = Guid.NewGuid().ToString() },
            Type = EventType.Test,
            Source = "EPOXY",
            When = Timestamp.FromDateTime(DateTime.UtcNow),
        };

        return Task.FromResult(evt);
    }
}
