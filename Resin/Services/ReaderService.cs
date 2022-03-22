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
            log.LogInformation("Hello {}, you'd like to read {} from {}", context.AuthContext.PeerIdentity.FirstOrDefault()?.Name ?? "No-one", request.Stream, request.Position);
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
            log.LogInformation("You're all up to date");
        }
        catch (Exception e)
        {
            log.LogError(e, "Failed to read from {} at position {}", request.Stream, request.Position);
            throw;
        }
    }

    public override async Task Subscribe(ReadRequest request, IServerStreamWriter<IsgEvent> responseStream, ServerCallContext context)
    {
        var done = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        var subId = "not set yet";

        try
        {
            log.LogInformation("Subscribing to {} from position {}", request.Stream, request.Position);
            using var sub = await esdb.SubscribeToStreamAsync(
                request.Stream.ToString(),
                FromStream.After(new StreamPosition(request.Position)), 
                eventAppeared: (sub, ev, cancel) =>
                {
                    log.LogInformation("Received new events for {}", sub.SubscriptionId);

                    return responseStream.WriteAsync(Helpers.NewIsgEvent(
                        ev.Event.EventId.ToString(),
                        ev.Event.EventType,
                        ev.Event.Metadata,
                        ev.Event.Position.CommitPosition,
                        ev.Event.Created,
                        ev.Event.Data));
                },
                subscriptionDropped: (sub, reason, exp) =>
                {
                    log.LogWarning(exp, "Dropping subscription {} because {}", sub.SubscriptionId, reason);
                    done.Cancel();
                },
                cancellationToken: done.Token)
                .ConfigureAwait(false);

            subId = sub.SubscriptionId; 
            await Task.Delay(-1, done.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            log.LogWarning("Subscription {} cancelled", subId);
        }
        finally
        {
            done.Cancel();
        }
    }
}
