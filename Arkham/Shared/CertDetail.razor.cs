using Microsoft.AspNetCore.Components;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Arkham.Shared
{
    public partial class CertDetail
    {
        [Parameter, EditorRequired]
        public X509Certificate2 Cert { get; set; }

        private string GetKeyUsage()
        {
            var keyUsage = Cert?.Extensions.Where(e => e.Oid?.FriendlyName == "Key Usage").Cast<X509KeyUsageExtension>().FirstOrDefault();
            if (keyUsage is null)
                return string.Empty;

            return Stringify(keyUsage.KeyUsages);
        }

        private string Stringify(X509KeyUsageFlags keyUsages)
        {
            if (keyUsages.HasFlag(X509KeyUsageFlags.EncipherOnly))
                return "Encipherment only";

            if (keyUsages.HasFlag(X509KeyUsageFlags.DecipherOnly))
                return "Decipherment only";

            var desc = new StringBuilder();

            if (keyUsages.HasFlag(X509KeyUsageFlags.CrlSign))
                desc.Append("CRL Signing ");

            if (keyUsages.HasFlag(X509KeyUsageFlags.DataEncipherment))
                desc.Append("Data Encipherment ");

            if (keyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature))
                desc.Append("Digital Signing ");

            if (keyUsages.HasFlag(X509KeyUsageFlags.KeyAgreement))
                desc.Append("Key Agreement ");

            if (keyUsages.HasFlag(X509KeyUsageFlags.KeyCertSign))
                desc.Append("Key Cert Signing ");

            if (keyUsages.HasFlag(X509KeyUsageFlags.KeyEncipherment))
                desc.Append("Key Encipherment ");

            if (keyUsages.HasFlag(X509KeyUsageFlags.NonRepudiation))
                desc.Append("Non-repudiation ");

            return desc.ToString();
        }
    }
}