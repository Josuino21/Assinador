using System;
using System.IO;

namespace AssinadorP7s
{
    internal sealed class AccessCheckResult
    {
        public bool Allowed { get; private set; }
        public string Message { get; private set; }

        private AccessCheckResult(bool allowed, string message)
        {
            Allowed = allowed;
            Message = message;
        }

        public static AccessCheckResult Ok(string message)
        {
            return new AccessCheckResult(true, message);
        }

        public static AccessCheckResult Denied(string message)
        {
            return new AccessCheckResult(false, message);
        }
    }

    internal static class AccessPolicy
    {
        public static AccessCheckResult CanReadFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return AccessCheckResult.Denied("Informe o arquivo de entrada.");

            if (!File.Exists(filePath))
                return AccessCheckResult.Denied("Arquivo de entrada nao encontrado.");

            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length == 0)
                        return AccessCheckResult.Denied("O arquivo de entrada esta vazio.");
                }

                return AccessCheckResult.Ok("Leitura permitida.");
            }
            catch (UnauthorizedAccessException)
            {
                return AccessCheckResult.Denied("O usuario atual nao tem permissao de leitura no arquivo selecionado.");
            }
            catch (IOException ex)
            {
                return AccessCheckResult.Denied("Nao foi possivel abrir o arquivo para leitura: " + ex.Message);
            }
        }

        public static AccessCheckResult CanWriteFile(string outputPath)
        {
            if (String.IsNullOrWhiteSpace(outputPath))
                return AccessCheckResult.Denied("Informe o arquivo de saida.");

            try
            {
                string directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
                if (String.IsNullOrWhiteSpace(directory))
                    directory = Directory.GetCurrentDirectory();

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string probe = Path.Combine(directory, ".assinador_write_test_" + Guid.NewGuid().ToString("N") + ".tmp");
                File.WriteAllText(probe, "ok");
                File.Delete(probe);
                return AccessCheckResult.Ok("Gravacao permitida.");
            }
            catch (UnauthorizedAccessException)
            {
                return AccessCheckResult.Denied("O usuario atual nao tem permissao de gravacao na pasta de destino.");
            }
            catch (IOException ex)
            {
                return AccessCheckResult.Denied("Nao foi possivel gravar na pasta de destino: " + ex.Message);
            }
        }

        public static string SuggestOutputPath(string inputPath)
        {
            string fileName = Path.GetFileName(inputPath) + ".p7s";
            string inputDirectory = Path.GetDirectoryName(Path.GetFullPath(inputPath));
            string candidate = Path.Combine(inputDirectory, fileName);

            AccessCheckResult result = CanWriteFile(candidate);
            if (result.Allowed)
                return candidate;

            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, fileName);
        }
    }
}
