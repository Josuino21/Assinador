using System;
using System.IO;

namespace AssinadorP7s
{
    internal static class Logger
    {
        public static void Error(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(AppInfo.LogDirectory);
                string path = Path.Combine(AppInfo.LogDirectory, "assinador.log");
                File.AppendAllText(path, DateTime.Now.ToString("s") + " " + ex + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
