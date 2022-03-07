using EventStore.Client;
using Grpc.Core;
using Epoxy.Grpc.Reader;
using static Epoxy.Grpc.Reader.Reader;

namespace Resin.Services;

public class ReaderService : ReaderBase
{
    private readonly ILogger<ReaderService> log;
    private readonly EventStoreClient esdb;

    public ReaderService(EventStoreClient esdb, ILogger<ReaderService> log)
    {
        this.log = log;
        this.esdb = esdb;
    }

    public override async Task<IsgEventSet> Read(ReadRequest request, ServerCallContext context)
    {
        try
        {
            var results = esdb.ReadStreamAsync(Direction.Forwards, request.Stream.ToString(), request.Position);

            throw new Exception();
            //var events = await from result
            //             in results
            //             select EventHelpers.NewIsgEvent(
            //                 result.Event.EventId.ToString(),
            //                 result.Event.EventType,
            //                 result.Event.Metadata,
            //                 result.Event.Position.CommitPosition,
            //                 result.Event.Created,
            //                 result.Event.Data
            //             );

            //return EventHelpers.NewIsgEventSet(result.Event.EventStreamId, events);

        }
        catch (Exception e)
        {
            log.LogError(e, "Failed to read from {} at position {}", request.Stream, request.Position);
            throw;
        }
    }

    //public override Task<IsgEvent> Read(ReadRequest request, ServerCallContext context)
    //{
    //    log.LogInformation("Some bugger wants to read {} from {}", request.Stream, request.Position);

    //    return Task.FromResult(NewIsgEvent("Resin", EventType.Test));
    //}
}
