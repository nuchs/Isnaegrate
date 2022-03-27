using System.Security.Cryptography.X509Certificates;

namespace Arkham.Ca;

public class Authority : IAuthority
{
    private readonly ICertCache cache;
    private readonly ILogger<Authority> log;
    private const string CertDir = "Certs";
    private const string RootPath = $@"{CertDir}\ca_root.cer";
    private const string IntermediatePath = $@"{CertDir}\ca_intermediate.cer";

    public X509Certificate2 RootCert { get; }

    public X509Certificate2 IntermediateCert { get; }

    public Authority(ICertCache cache, ILogger<Authority> log)
    {
        this.cache = cache;
        this.log = log;

        RootCert = new X509Certificate2(RootPath);
        IntermediateCert = new X509Certificate2(IntermediatePath);
    }

    public void SignCsr()
    {
        log.LogInformation("Signing CSR");
    }
}
