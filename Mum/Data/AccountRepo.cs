using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using Ing.Grpc.Common.Events;
using Ing.Grpc.Epoxy;
using static Epoxy.Grpc.EpoxyHelpers;
using static Ing.Grpc.Epoxy.Writer;
using static Ing.Grpc.Resin.Reader;
using Grpc.Core;
using Grpc.Net.Client;
using Ing.Grpc.Common.Events;
using Ing.Grpc.Resin;
using System.Text;

namespace Mum.Data;

internal sealed class AccountRepo: IDisposable
{
    private readonly ConcurrentDictionary<Guid, Account> accounts = new();
    private readonly ILogger log;
    private readonly ReaderClient reader;
    private readonly WriterClient writer;
    private readonly Task subsTask;
    private readonly CancellationTokenSource subsToken;

    public AccountRepo(WriterClient writer, ReaderClient reader, ILogger<AccountRepo> log)
    {
        this.log = log;
        this.writer = writer;
        this.reader = reader;

        subsToken = new CancellationTokenSource();
        subsTask = Task.Run(Subscription, subsToken.Token);
    }

    public void Dispose()
    {
        subsToken.Cancel();
        subsTask.Wait();
    }

    internal int NumberAccounts => accounts.Count;

    internal async Task Subscription()
    {
        log.LogInformation("Subscribing to account stream");

        var reply = reader.Subscribe(new ReadRequest { Stream = EventStream.Account, Position = 0 });

        await foreach (var isgEvent in reply.ResponseStream.ReadAllAsync())
        {
            ProcessEvent(isgEvent);
        }

        log.LogInformation("Closing subscription");
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

        accounts.AddOrUpdate(account.Id, _ => account, (_, _) => account);

        await RecordAccountEvent(account, eventType);

        log.LogDebug("{} account {} for {}", eventType, account.Id, account.Name);
    }

    internal async Task RemoveAccount(Guid accountId)
    {
        if (accounts.Remove(accountId, out var account))
        {
            await RecordAccountEvent(account.Id, AccountEventTypes.Deleted);

            log.LogDebug("Removed account {} for {}", accountId, account?.Name);
        }
    }


    //private AccountEventTypes GetEventType(ResolvedEvent rawEvent)
    //{
    //    if (Enum.TryParse<AccountEventTypes>(rawEvent.Event.EventType, out var type))
    //    {
    //        return type;
    //    }
    //    else
    //    {
    //        return AccountEventTypes.Unknown;
    //    }
    //}

    //        throw new NullReferenceException("Event contained no data");
    //    }
    //    catch
    //    {
    //        log.LogError("Failed to deserialise {} event {}", raw.Event.EventType, raw.Event.EventId);
    //        throw;
    //    }
    //}

    //        if (value != null)
    //        {
    //            return value;
    //        }

    private async Task RecordAccountEvent<T>(T payload, AccountEventTypes eventType)
    {
       // var json = JsonSerializer.Serialize(payload);
       // var eventData = new EventData(
       //    Uuid.NewUuid(),
       //    eventType.ToString(),
       //    Encoding.UTF8.GetBytes(json)
       //);

       // await es.AppendToStreamAsync(
       //     AccountStream,
       //     StreamState.Any,
       //     new[] { eventData }
       // );
    }

    private void ProcessEvent(IsgEvent rawEvent)
    {
        //switch (GetEventType(rawEvent))
        //{
        //    case AccountEventTypes.Added:
        //    case AccountEventTypes.Updated:
        //        var accountData = DeserialiseEvent<Account>(rawEvent);
        //        accounts.AddOrUpdate(accountData.Id, _ => accountData, (_, _) => accountData);
        //        log.LogDebug("{} account {} for {}", rawEvent.Event.EventType, accountData.Id, accountData.Name);
        //        break;

        //    case AccountEventTypes.Deleted:
        //        var accountId = DeserialiseEvent<Guid>(rawEvent);
        //        accounts.Remove(accountId, out var account);
        //        log.LogDebug("Removed account {} for {}", accountId, account?.Name);
        //        break;

        //    default:
        //        log.LogWarning("Unable to process event - Unknown event type: {}", rawEvent.Event.EventType);
        //        break;
        //}
    }

    //private T DeserialiseEvent<T>(ResolvedEvent raw)
    //{
    //    try
    //    {
    //        var json = Encoding.UTF8.GetString(raw.Event.Data.Span);
    //        var value = JsonSerializer.Deserialize<T>(json);
}
