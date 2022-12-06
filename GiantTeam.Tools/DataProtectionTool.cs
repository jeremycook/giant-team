using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

public class DataProtectionTool
{
    public static void DataProtection(DataProtectionFormat format = DataProtectionFormat.Pem)
    {
        X500DistinguishedName subject = new X500DistinguishedName("CN=Data Protection Certificate");
        using var algorithm = RSA.Create(keySizeInBits: 4096);
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment, critical: true));
        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));
        var pfxBytes = certificate.Export(X509ContentType.Pkcs12, string.Empty);

        var textCert =
        "-----BEGIN PRIVATE KEY-----\n" +
        string.Concat(Convert.ToBase64String(pfxBytes).Select((ch, i) => ch + (i % 64 == 0 && i > 0 ? "\n" : ""))) + "\n" +
        "-----END PRIVATE KEY-----";

        switch (format)
        {
            case DataProtectionFormat.Json:
                Console.WriteLine(JsonSerializer.Serialize(textCert));
                break;

            case DataProtectionFormat.Pem:
            default:
                Console.WriteLine(textCert);
                break;
        }
    }
}