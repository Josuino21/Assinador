using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace AssinadorP7s
{
    internal static class P7sSigner
    {
        public static void SignPdf(string inputPdfPath, string outputP7sPath, X509Certificate2 certificate, bool embedded, string hashName)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            AccessCheckResult readCheck = AccessPolicy.CanReadFile(inputPdfPath);
            if (!readCheck.Allowed)
                throw new InvalidOperationException(readCheck.Message);

            AccessCheckResult writeCheck = AccessPolicy.CanWriteFile(outputP7sPath);
            if (!writeCheck.Allowed)
                throw new InvalidOperationException(writeCheck.Message);

            byte[] pdfBytes = File.ReadAllBytes(inputPdfPath);
            ContentInfo contentInfo = new ContentInfo(pdfBytes);
            SignedCms signedCms = new SignedCms(contentInfo, !embedded);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            signer.IncludeOption = X509IncludeOption.EndCertOnly;
            signer.DigestAlgorithm = new Oid(HashOid(hashName));

            signedCms.ComputeSignature(signer, false);

            string directory = Path.GetDirectoryName(Path.GetFullPath(outputP7sPath));
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(outputP7sPath, signedCms.Encode());
        }

        public static string BuildOutputPath(string inputPdfPath, string outputDirectory, bool embedded)
        {
            string fileName = embedded
                ? Path.GetFileName(inputPdfPath) + ".p7s"
                : Path.GetFileNameWithoutExtension(inputPdfPath) + ".p7s";

            return Path.Combine(outputDirectory, fileName);
        }

        public static string HashOid(string hashName)
        {
            string normalized = (hashName ?? "sha256").Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "sha1": return "1.3.14.3.2.26";
                case "sha256": return "2.16.840.1.101.3.4.2.1";
                case "sha384": return "2.16.840.1.101.3.4.2.2";
                case "sha512": return "2.16.840.1.101.3.4.2.3";
                default:
                    throw new ArgumentException("Hash nao suportado: " + hashName);
            }
        }
    }
}
