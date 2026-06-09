using System;
using System.Security.Cryptography.X509Certificates;

namespace AssinadorP7s
{
    internal static class PfxCertificateProvider
    {
        public static X509Certificate2 Load(string pfxPath, string password)
        {
            AccessCheckResult readCheck = AccessPolicy.CanReadFile(pfxPath);
            if (!readCheck.Allowed)
                throw new InvalidOperationException(readCheck.Message);

            X509Certificate2 certificate = new X509Certificate2(
                pfxPath,
                password,
                X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException("O certificado PFX/P12 nao possui chave privada.");

            DateTime now = DateTime.Now;
            if (now < certificate.NotBefore || now > certificate.NotAfter)
                throw new InvalidOperationException("O certificado esta fora do periodo de validade.");

            return certificate;
        }
    }
}
