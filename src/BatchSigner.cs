using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace AssinadorP7s
{
    internal static class BatchSigner
    {
        public static int SignMany(IEnumerable<string> pdfPaths, string outputFolder, X509Certificate2 certificate, bool embedded, string hash)
        {
            if (pdfPaths == null)
                throw new ArgumentNullException("pdfPaths");

            if (String.IsNullOrWhiteSpace(outputFolder))
                throw new InvalidOperationException("Informe a pasta de saida.");

            int count = 0;
            foreach (string pdf in pdfPaths)
            {
                PdfValidator.ValidateBasic(pdf);
                string output = P7sSigner.BuildOutputPath(pdf, outputFolder, embedded);
                P7sSigner.SignPdf(pdf, output, certificate, embedded, hash);
                count++;
            }

            if (count == 0)
                throw new InvalidOperationException("Adicione pelo menos um PDF.");

            return count;
        }
    }
}
