using System.Diagnostics;

namespace controller
{
    internal static class Logger
    {
        public static bool EnableLogging = true;   // set to false to disable log file output

        private static StreamWriter? _writer;

        public static void Initialize()
        {
            if (!EnableLogging) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"ONrecorderLog_{timestamp}.txt";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            _writer = new StreamWriter(path, append: false) { AutoFlush = true };
            Trace.Listeners.Add(new TimestampedFileListener(_writer));
        }

        public static void Close() => _writer?.Close();
    }

    internal class TimestampedFileListener : TraceListener
    {
        private readonly StreamWriter _writer;

        public TimestampedFileListener(StreamWriter writer) => _writer = writer;

        public override void Write(string? message) { }   // required override, unused

        public override void WriteLine(string? message)
        {
            if (!Logger.EnableLogging) return;
            _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} - {message}");
        }
    }
}
