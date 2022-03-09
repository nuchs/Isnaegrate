using EventStore.Client;
using Grpc.Core;
using Ing.Grpc.Resin;
using static Ing.Grpc.Resin.Reader;

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

    public override async Task Read(ReadRequest request, IServerStreamWriter<IsgEvent> stream, ServerCallContext context)
    {
        try
        {
            var results = esdb.ReadStreamAsync(Direction.Forwards, request.Stream.ToString(), request.Position);

            await foreach (var result in results)
            {
                await stream.WriteAsync(Helpers.NewIsgEvent(
                                   result.Event.EventId.ToString(),
                                   result.Event.EventType,
                                   result.Event.Metadata,
                                   result.Event.Position.CommitPosition,
                                   result.Event.Created,
                                   result.Event.Data
                               ));
            }
        }
        catch (Exception e)
        {
            log.LogError(e, "Failed to read from {} at position {}", request.Stream, request.Position);
            throw;
        }
    }
}