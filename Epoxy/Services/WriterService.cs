using EventStore.Client;
using Grpc.Core;
using Ing.Grpc.Epoxy;
using static Ing.Grpc.Epoxy.Writer;

namespace Epoxy.Services;

public class WriterService : WriterBase
{
    private readonly ILogger<WriterService> log;
    private readonly EventStoreClient esdb;

    public WriterService(EventStoreClient esdb, ILogger<WriterService> log)
    {
        this.log = log;
        this.esdb = esdb;
    }

    public override async Task<OpResult> Append(PropositionSet request, ServerCallContext context)
    {
        try
        {
            log.LogInformation("Writing {} events to {}", request.Propositions.Count, request.Stream);

            var result = await esdb.AppendToStreamAsync(
                request.Stream.ToString(),
                StreamState.Any,
                from prop in request.Propositions select prop.ToEventData());

            return new OpResult() { Success = true };
        }
        catch (Exception e)
        {
            log.LogError(e, "Failed to append events to {}", request.Stream);
            return new OpResult() { Success = false };
        }
    }
}
