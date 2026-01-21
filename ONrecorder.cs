using System.Diagnostics;

namespace controller
{
    internal static class ONrecorder
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            //Some logging code to catch crashes (hopefully). ChatGPT helped with this, so use with caution
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log("---Unhandled Exception---");
                Log(e.ExceptionObject.ToString());
            };

            Application.ThreadException += (sender, e) =>
            {
                Log("---Thread Exception---");
                Log(e.Exception.ToString());
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            //Run the form
            Application.Run(new Form1());
        }

        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ONrecorderDebugLog.txt");

        private static void Log(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.ffff} - {message}";
            Debug.WriteLine(line);

            try
            {
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            catch
            {
                Debug.WriteLine("Failed to write to exception log file..");
            }
        }
    }
}