namespace Bowser.Data;

public record User(string Id, string Title, string Name, string Org, DateTime CertExpiry, DateTime LastOnline);
