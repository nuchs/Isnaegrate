using Grpc.Core;
using Resin.Grpc;


using static Resin.Grpc.Reader;

namespace JaundicedSage.Services;

public sealed class UserRepoWorker : BackgroundService
{
    private const string AccountStream = "Account";
    private const string SessionStream = "Session";
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
            var accountSubscription = Subscribe(AccountStream, ProcessAccountEvent, stoppingToken);
            var sessionSubscription = Subscribe(SessionStream, ProcessSessionEvent, stoppingToken);

            await Task.WhenAll(accountSubscription, sessionSubscription);
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

    private async Task Subscribe(string stream, Action<IsgEvent> handler, CancellationToken stoppingToken)
    {
        log.LogInformation("Subscribing to {} stream", stream);

        var reply = reader.Subscribe(new ReadRequest { Stream = stream, Position = 0 }, cancellationToken: stoppingToken);

        await foreach (var isgEvent in reply.ResponseStream.ReadAllAsync(stoppingToken))
        {
            handler(isgEvent);
        }
    }

    private void ProcessAccountEvent(IsgEvent raw)
    {
        switch (raw.GetEventType<AccountEventTypes>())
        {
            case AccountEventTypes.Added:
            case AccountEventTypes.Updated:
                var accountData = raw.DeserialiseEvent<Account>();
                if(accountData is not null)
                    repo.AddOrUpdateUser(accountData, raw.Type);
                break;

            case AccountEventTypes.Deleted:
                var accountId = raw.DeserialiseEvent<Guid>();
                repo.RemoveAccount(accountId);
                break;

            default:
                log.LogWarning("Unable to process event - Unknown event type: {}", raw.Type);
                break;
        }
    }

    private void ProcessSessionEvent(IsgEvent raw)
    {
        switch (raw.GetEventType<SessionEventTypes>())
        {
            case SessionEventTypes.SessionStart:
            case SessionEventTypes.SessionEnd:
                var session = raw.DeserialiseEvent<Session>();
                session.When = raw.When.ToDateTime();
                repo.AddOrUpdateUser(session);
                break;

            default:
                log.LogWarning("Unable to process event - Unknown event type: {}", raw.Type);
                break;
        }
    }
}
