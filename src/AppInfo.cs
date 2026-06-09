using System;
using System.IO;
using System.Security.Principal;

namespace AssinadorP7s
{
    internal static class AppInfo
    {
        public const string Name = "Assinador Pmenos";
        public const string Version = "1.0.0";

        public static string LogDirectory
        {
            get
            {
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, "AssinadorPmenos");
            }
        }

        public static bool IsRunningAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
