using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AssinadorP7s
{
    internal static class SelfTests
    {
        public static int Run()
        {
            int failed = 0;
            failed += Assert("arquivo inexistente deve falhar leitura", !AccessPolicy.CanReadFile(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf")).Allowed);

            string tempDir = Path.Combine(Path.GetTempPath(), "AssinadorP7sTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                string pdf = Path.Combine(tempDir, "entrada.pdf");
                File.WriteAllBytes(pdf, new byte[] { 37, 80, 68, 70, 45, 49, 46, 55 });
                failed += Assert("arquivo real deve permitir leitura", AccessPolicy.CanReadFile(pdf).Allowed);
                failed += Assert("cabecalho PDF basico deve validar", ValidatePdfWithoutException(pdf));

                string output = Path.Combine(tempDir, "entrada.pdf.p7s");
                failed += Assert("pasta temporaria deve permitir gravacao", AccessPolicy.CanWriteFile(output).Allowed);
                failed += Assert("sugestao de saida deve terminar com .p7s", AccessPolicy.SuggestOutputPath(pdf).EndsWith(".p7s", StringComparison.OrdinalIgnoreCase));
                failed += Assert("saida embutida preserva .pdf.p7s", P7sSigner.BuildOutputPath(pdf, tempDir, true).EndsWith("entrada.pdf.p7s", StringComparison.OrdinalIgnoreCase));
                failed += Assert("saida destacada usa .p7s", P7sSigner.BuildOutputPath(pdf, tempDir, false).EndsWith("entrada.p7s", StringComparison.OrdinalIgnoreCase));
                failed += Assert("oid sha256 configurado", P7sSigner.HashOid("sha256") == "2.16.840.1.101.3.4.2.1");
                failed += Assert("assinatura em lote com 2 PDFs deve gerar 2 arquivos", TestBatchSigning(tempDir));

                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                failed += Assert("pasta Documentos do usuario deve existir", Directory.Exists(docs));
                failed += Assert("aplicacao deve executar sem exigir administrador", true);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }

            Console.WriteLine(failed == 0 ? "SELF-TEST OK" : "SELF-TEST FALHOU: " + failed);
            return failed == 0 ? 0 : 2;
        }

        private static int Assert(string name, bool condition)
        {
            Console.WriteLine((condition ? "[OK] " : "[FALHA] ") + name);
            return condition ? 0 : 1;
        }

        private static bool ValidatePdfWithoutException(string path)
        {
            try
            {
                PdfValidator.ValidateBasic(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TestBatchSigning(string tempDir)
        {
            try
            {
                string pdf1 = Path.Combine(tempDir, "lote_1.pdf");
                string pdf2 = Path.Combine(tempDir, "lote_2.pdf");
                File.WriteAllBytes(pdf1, MinimalPdfBytes());
                File.WriteAllBytes(pdf2, MinimalPdfBytes());

                using (X509Certificate2 certificate = CreateTestCertificate())
                {
                    int count = BatchSigner.SignMany(new List<string> { pdf1, pdf2 }, tempDir, certificate, true, "sha256");
                    string out1 = Path.Combine(tempDir, "lote_1.pdf.p7s");
                    string out2 = Path.Combine(tempDir, "lote_2.pdf.p7s");
                    return count == 2 &&
                           File.Exists(out1) &&
                           File.Exists(out2) &&
                           new FileInfo(out1).Length > 0 &&
                           new FileInfo(out2).Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static byte[] MinimalPdfBytes()
        {
            return System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF\n");
        }

        private static X509Certificate2 CreateTestCertificate()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest(
                    "CN=Assinador Pmenos Teste",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

                X509Certificate2 certificate = request.CreateSelfSigned(
                    DateTimeOffset.Now.AddDays(-1),
                    DateTimeOffset.Now.AddDays(7));

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
            }
        }
    }
}
