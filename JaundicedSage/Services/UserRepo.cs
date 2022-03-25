using Grpc.Core;
using Resin.Grpc;
using System.Collections.Concurrent;

using static Resin.Grpc.Reader;

namespace JaundicedSage.Services;

public sealed class UserRepo : BackgroundService, IAsyncDisposable
{
    private const string EventStream = "Account";
    private readonly ConcurrentDictionary<Guid, User> users = new();
    private readonly ILogger log;
    private readonly ReaderClient reader;
    private readonly Task subsTask;
    private readonly CancellationTokenSource subsCts;

    public UserRepo(ReaderClient reader, ILogger<UserRepo> log)
    {
        this.log = log;
        this.reader = reader;

        subsCts = new CancellationTokenSource();
        subsTask = Task.Run(Subscription, subsCts.Token);
        log.LogInformation("UserRepo ready");
    }

    public async ValueTask DisposeAsync()
    {
        subsCts.Cancel();

        try
        {
            await subsTask;
        }
        catch (OperationCanceledException)
        {
            log.LogInformation("Closed subscription");
        }
        catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Cancelled)
        {
            log.LogInformation("Closed subscription");
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Error while closing subscription");
        }
    }

    internal async Task Subscription()
    {
        log.LogInformation("Subscribing to {} stream", EventStream);

        var reply = reader.Subscribe(new ReadRequest { Stream = EventStream, Position = 0 }, cancellationToken: subsCts.Token);

        await foreach (var isgEvent in reply.ResponseStream.ReadAllAsync(subsCts.Token))
        {
            ProcessEvent(isgEvent);
        }
    }

    internal User? GetUser(string idString)
    {
        return Guid.TryParse(idString, out var id)
            && users.TryGetValue(id, out var user) ? user : null;
    }

    internal IEnumerable<User> GetAllUsers()
        => users.Values;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    private void ProcessEvent(IsgEvent rawEvent)
    {
        switch (rawEvent.GetEventType<AccountEventTypes>())
        {
            case AccountEventTypes.Added:
            case AccountEventTypes.Updated:
                var accountData = rawEvent.DeserialiseEvent<Account>();
                users.AddOrUpdate(
                    accountData.Id,
                    id => new User(id).MergeAccountData(accountData),
                    (_, existing) => existing.MergeAccountData(accountData));
                log.LogDebug("{} account {} for {}", rawEvent.Type, accountData.Id, accountData.Name);
                break;

            case AccountEventTypes.Deleted:
                var accountId = rawEvent.DeserialiseEvent<Guid>();
                users.Remove(accountId, out var user);
                log.LogDebug("Account {} deleted, removing user {}", accountId, user?.Name);
                break;

            default:
                log.LogWarning("Unable to process event - Unknown event type: {}", rawEvent.Type);
                break;
        }
    }
}
