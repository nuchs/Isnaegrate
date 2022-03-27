namespace JaundicedSage.Services;

public sealed class User
{
    public User(Guid id) => Id = id;

    public Guid Id { get; }

    public string Title { get; private set; } = "No Title";

    public string Name { get; private set; } = "The user with no name";

    public string Org { get; private set; } = "No org";

    public DateTime CertExpiry { get; private set; } = DateTime.MinValue.ToUniversalTime();

    public DateTime LastOnline { get; private set; } = DateTime.MinValue.ToUniversalTime();

    public User MergeAccountData(Account account)
    {
        if (account.Id != Id)
        {
            throw new InvalidOperationException($"Id mismatch cannot merge data for account {account.Id} into user with id {Id}");
        }

        Name = account.Name;
        Org = account.Org;
        Title = account.Title;

        return this;
    }

    public User MergeCertData(DateTime certExpiry)
    {
        CertExpiry = certExpiry.ToUniversalTime();

        return this;
    }

    public User MergeLogonData(Session session)
    {
        if (session.Id != Id)
        {
            throw new InvalidOperationException($"Id mismatch cannot merge data for session {session.Id} into user with id {Id}");
        }

        LastOnline = session.When.ToUniversalTime();

        return this;
    }
}
