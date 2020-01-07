using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace NWS_Alerts
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static readonly string LogDirectory = Path.GetTempPath() + "\\" + AppDomain.CurrentDomain.FriendlyName;
        static string LogFile = LogDirectory + @"\Application.log";

        protected override void OnStartup(StartupEventArgs e)
        {
            if (File.Exists(LogFile))
            {
                while (IsFileLocked(LogFile))
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }
    }
}
