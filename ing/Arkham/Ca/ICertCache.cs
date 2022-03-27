using System.Security.Cryptography.X509Certificates;

namespace Arkham.Ca;

public interface ICertCache
{
    void Add(X509Certificate2 cert);
    X509Certificate2 Get(string serial);
    IEnumerable<X509Certificate2> GetAll();
}