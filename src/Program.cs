using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace AssinadorP7s
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length > 0 && String.Equals(args[0], "--self-test", StringComparison.OrdinalIgnoreCase))
                return SelfTests.Run();

            if (args.Length > 0 && String.Equals(args[0], "--sign", StringComparison.OrdinalIgnoreCase))
                return SignFromCommandLine(args);

            if (args.Length > 0)
                return SignFromLegacyCommandLine(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }

        private static int SignFromCommandLine(string[] args)
        {
            try
            {
                if (args.Length < 4)
                {
                    Console.Error.WriteLine("Uso: assinador_pmenos.exe --sign entrada.pdf saida.p7s thumbprint");
                    return 1;
                }

                string input = args[1];
                string output = args[2];
                string thumbprint = args[3];
                X509Certificate2 certificate = CertificateProvider.FindByThumbprint(thumbprint);
                if (certificate == null)
                {
                    Console.Error.WriteLine("Certificado nao encontrado no repositorio do usuario atual.");
                    return 1;
                }

                PdfValidator.ValidateBasic(input);
                P7sSigner.SignPdf(input, output, certificate, false, "sha256");
                Console.WriteLine("Arquivo P7S gerado: " + output);
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static int SignFromLegacyCommandLine(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("Uso: assinador_pmenos.exe documento.pdf certificado.pfx -o saida.p7s --senha senha --embutido --hash sha256");
                    return 1;
                }

                string pdf = args[0];
                string pfx = args[1];
                string output = null;
                string password = null;
                string hash = "sha256";
                bool embedded = false;

                for (int i = 2; i < args.Length; i++)
                {
                    string arg = args[i];
                    if ((arg == "-o" || arg == "--output") && i + 1 < args.Length)
                        output = args[++i];
                    else if (arg == "--senha" && i + 1 < args.Length)
                        password = args[++i];
                    else if (arg == "--hash" && i + 1 < args.Length)
                        hash = args[++i];
                    else if (arg == "--embutido")
                        embedded = true;
                }

                PdfValidator.ValidateBasic(pdf);
                X509Certificate2 certificate = PfxCertificateProvider.Load(pfx, password);

                if (String.IsNullOrWhiteSpace(output))
                {
                    string directory = Path.GetDirectoryName(Path.GetFullPath(pdf));
                    output = P7sSigner.BuildOutputPath(pdf, directory, embedded);
                }

                P7sSigner.SignPdf(pdf, output, certificate, embedded, hash);
                Console.WriteLine("Arquivo assinado gerado: " + Path.GetFullPath(output));
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
