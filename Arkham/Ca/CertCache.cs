using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace Arkham.Ca;

public class CertCache : ICertCache
{
    private readonly ConcurrentDictionary<string, X509Certificate2> cache = new();

    public void Add(X509Certificate2 cert)
        => cache.AddOrUpdate(cert.SerialNumber, _ => cert, (_, _) => cert);

    public X509Certificate2 Get(string serial)
        => cache[serial];

    public IEnumerable<X509Certificate2> GetAll()
        => cache.Values;
}
