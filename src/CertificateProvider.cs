using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace AssinadorP7s
{
    internal sealed class CertificateOption
    {
        public X509Certificate2 Certificate { get; private set; }
        public string DisplayName { get; private set; }

        public CertificateOption(X509Certificate2 certificate)
        {
            Certificate = certificate;
            DisplayName = BuildDisplayName(certificate);
        }

        public override string ToString()
        {
            return DisplayName;
        }

        private static string BuildDisplayName(X509Certificate2 certificate)
        {
            string subject = certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (String.IsNullOrWhiteSpace(subject))
                subject = certificate.Subject;

            return subject + " | vence em " + certificate.NotAfter.ToString("dd/MM/yyyy") + " | " + certificate.Thumbprint;
        }
    }

    internal static class CertificateProvider
    {
        public static IList<CertificateOption> LoadUserSigningCertificates()
        {
            List<CertificateOption> certificates = new List<CertificateOption>();
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                foreach (X509Certificate2 certificate in store.Certificates)
                {
                    if (IsUsableForSigning(certificate))
                        certificates.Add(new CertificateOption(certificate));
                }
            }
            finally
            {
                store.Close();
            }

            certificates.Sort(delegate(CertificateOption a, CertificateOption b)
            {
                return DateTime.Compare(b.Certificate.NotAfter, a.Certificate.NotAfter);
            });

            return certificates;
        }

        public static X509Certificate2 FindByThumbprint(string thumbprint)
        {
            if (String.IsNullOrWhiteSpace(thumbprint))
                return null;

            string normalized = NormalizeThumbprint(thumbprint);
            foreach (CertificateOption option in LoadUserSigningCertificates())
            {
                if (NormalizeThumbprint(option.Certificate.Thumbprint) == normalized)
                    return option.Certificate;
            }

            return null;
        }

        private static bool IsUsableForSigning(X509Certificate2 certificate)
        {
            if (certificate == null)
                return false;

            DateTime now = DateTime.Now;
            if (now < certificate.NotBefore || now > certificate.NotAfter)
                return false;

            if (!certificate.HasPrivateKey)
                return false;

            foreach (X509Extension extension in certificate.Extensions)
            {
                X509KeyUsageExtension keyUsage = extension as X509KeyUsageExtension;
                if (keyUsage != null)
                {
                    X509KeyUsageFlags flags = keyUsage.KeyUsages;
                    return (flags & X509KeyUsageFlags.DigitalSignature) == X509KeyUsageFlags.DigitalSignature ||
                           (flags & X509KeyUsageFlags.NonRepudiation) == X509KeyUsageFlags.NonRepudiation;
                }
            }

            return true;
        }

        private static string NormalizeThumbprint(string thumbprint)
        {
            return thumbprint.Replace(" ", String.Empty).Replace("\u200e", String.Empty).ToUpperInvariant();
        }
    }
}
