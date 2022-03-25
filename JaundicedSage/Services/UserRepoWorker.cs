using Grpc.Core;
using Resin.Grpc;


using static Resin.Grpc.Reader;

namespace JaundicedSage.Services;

public sealed class UserRepoWorker : BackgroundService
{
    private const string EventStream = "Account";
    private readonly ILogger<UserRepoWorker> log;
    private readonly ReaderClient reader;
    private readonly UserRepo repo;

    public UserRepoWorker(UserRepo repo, ReaderClient reader, ILogger<UserRepoWorker> log)
    {
        this.log = log;
        this.reader = reader;
        this.repo = repo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Subscription(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            log.LogInformation("Closed subscription");
        }
        catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Cancelled)
        {
            log.LogInformation("GRPC closed subscription");
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Subscription error - closing");
        }
    }

    private async Task Subscription(CancellationToken stoppingToken)
    {
        log.LogInformation("Subscribing to {} stream", EventStream);

        var reply = reader.Subscribe(new ReadRequest { Stream = EventStream, Position = 0 }, cancellationToken: stoppingToken);

        await foreach (var isgEvent in reply.ResponseStream.ReadAllAsync(stoppingToken))
        {
            ProcessEvent(isgEvent);
        }
    }

    private void ProcessEvent(IsgEvent rawEvent)
    {
        switch (rawEvent.GetEventType<AccountEventTypes>())
        {
            case AccountEventTypes.Added:
            case AccountEventTypes.Updated:
                var accountData = rawEvent.DeserialiseEvent<Account>();
                if(accountData is not null)
                    repo.AddOrUpdateAccount(accountData, rawEvent.Type);
                break;

            case AccountEventTypes.Deleted:
                var accountId = rawEvent.DeserialiseEvent<Guid>();
                repo.RemoveAccount(accountId);
                break;

            default:
                log.LogWarning("Unable to process event - Unknown event type: {}", rawEvent.Type);
                break;
        }
    }
}
