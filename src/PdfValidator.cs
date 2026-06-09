using System;
using System.IO;

namespace AssinadorP7s
{
    internal static class PdfValidator
    {
        public static void ValidateBasic(string path)
        {
            AccessCheckResult readCheck = AccessPolicy.CanReadFile(path);
            if (!readCheck.Allowed)
                throw new InvalidOperationException(readCheck.Message);

            byte[] header = new byte[5];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Read(header, 0, header.Length) != header.Length)
                    throw new InvalidOperationException("PDF invalido ou incompleto.");
            }

            string marker = System.Text.Encoding.ASCII.GetString(header);
            if (marker != "%PDF-")
                throw new InvalidOperationException("O arquivo selecionado nao parece ser um PDF valido.");
        }
    }
}
