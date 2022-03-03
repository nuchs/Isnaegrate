using System.Security.Cryptography.X509Certificates;

namespace Arkham.Ca;

public interface IAuthority
{
    X509Certificate2 IntermediateCert { get; }
    X509Certificate2 RootCert { get; }

    void SignCsr();
}