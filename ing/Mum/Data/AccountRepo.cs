using Grpc.Core;
using Resin.Grpc;
using System.Collections.Concurrent;

using static Epoxy.Grpc.PropositionExtensions;
using static Epoxy.Grpc.Writer;
using static Resin.Grpc.Reader;

namespace Mum.Data;

internal sealed class AccountRepo : IAsyncDisposable
{
    private const string EventStream = "Account";
    private readonly ConcurrentDictionary<Guid, Account> accounts = new();
    private readonly ILogger log;
    private readonly ReaderClient reader;
    private readonly WriterClient writer;
    private readonly Task subsTask;
    private readonly CancellationTokenSource subsCts;

    public AccountRepo(WriterClient writer, ReaderClient reader, ILogger<AccountRepo> log)
    {
        this.log = log;
        this.writer = writer;
        this.reader = reader;

        subsCts = new CancellationTokenSource();
        subsTask = Task.Run(Subscription, subsCts.Token);
        log.LogInformation("AccountRepo ready");
    }

    internal int NumberAccounts => accounts.Count;

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
        catch(RpcException ex) when (ex.Status.StatusCode == StatusCode.Cancelled)
        {
            log.LogInformation("Closed subscription");
        }
        catch(Exception ex)
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

    internal IEnumerable<Account> GetAllAccounts()
        => accounts.Values;

    internal async Task AddOrUpdateAccount(Account account)
    {
        var eventType = AccountEventTypes.Added;

        if (accounts.TryGetValue(account.Id, out var existing))
        {
            if (account == existing)
            {
                return;
            }

            eventType = AccountEventTypes.Updated;
        }

        await RecordAccountEvent(account, eventType);

        log.LogDebug("{} account {} for {}", eventType, account.Id, account.Name);
    }

    internal async Task RemoveAccount(Guid accountId)
    {
        if (accounts.TryGetValue(accountId, out var account))
        {
            await RecordAccountEvent(account.Id, AccountEventTypes.Deleted);

            log.LogDebug("Removed account {} for {}", accountId, account?.Name);
        }
    }

    private async Task RecordAccountEvent<T>(T payload, AccountEventTypes eventType)
    {
        var propSet = NewPropositionSet(
            EventStream,
            NewProposition(Guid.NewGuid(), eventType.ToString(), "Mum", payload));

        await writer.AppendAsync(propSet);
    }

    private void ProcessEvent(IsgEvent rawEvent)
    {
        switch (rawEvent.GetEventType<AccountEventTypes>())
        {
            case AccountEventTypes.Added:
            case AccountEventTypes.Updated:
                var accountData = rawEvent.DeserialiseEvent<Account>();
                accounts.AddOrUpdate(accountData.Id, _ => accountData, (_, _) => accountData);
                log.LogDebug("{} account {} for {}", rawEvent.Type, accountData.Id, accountData.Name);
                break;

            case AccountEventTypes.Deleted:
                var accountId = rawEvent.DeserialiseEvent<Guid>();
                accounts.Remove(accountId, out var account);
                log.LogDebug("Removed account {} for {}", accountId, account?.Name);
                break;

            default:
                log.LogWarning("Unable to process event - Unknown event type: {}", rawEvent.Type);
                break;
        }
    }
}
