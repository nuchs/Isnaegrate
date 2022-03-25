namespace JaundicedSage.Services;

using System.Collections.Concurrent;

public class UserRepo
{
    private readonly ConcurrentDictionary<Guid, User> users = new();
    private readonly ILogger<UserRepo> log;

    public UserRepo(ILogger<UserRepo> log) => this.log = log;

    internal User? GetUser(string idString)
        => Guid.TryParse(idString, out var id) &&
            users.TryGetValue(id, out var user) ? user : null;

    internal IEnumerable<User> GetAllUsers()
        => users.Values;

    internal void AddOrUpdateAccount(Account account, string action)
    {
        users.AddOrUpdate(
            account.Id,
            id => new User(id).MergeAccountData(account),
            (_, existing) => existing.MergeAccountData(account));
        log.LogDebug("{} account {} for {}", action, account.Id, account.Name);
    }

    internal void RemoveAccount(Guid id)
    {
        users.Remove(id, out var user);
        log.LogDebug("Account {} deleted, removing user {}", id, user?.Name);
    }
}
