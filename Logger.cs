using System.Diagnostics;

namespace controller
{
    internal static class Logger
    {
        public static bool EnableLogging { get; private set; }

        private static StreamWriter? _writer;
        private static TimestampedFileListener? _listener;

        public static void Initialize()
        {
            if (!EnableLogging || _writer != null)
                return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"ONrecorderLog_{timestamp}.txt";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            _writer = new StreamWriter(path, append: false) { AutoFlush = true };
            _listener = new TimestampedFileListener(_writer);
            Trace.Listeners.Add(_listener);
        }

        public static void SetFileLoggingEnabled(bool enabled)
        {
            if (EnableLogging == enabled)
            {
                if (enabled && _writer == null)
                    Initialize();
                return;
            }

            EnableLogging = enabled;
            if (enabled)
                Initialize();
            else
                Close();
        }

        public static void Close()
        {
            if (_listener != null)
            {
                Trace.Listeners.Remove(_listener);
                _listener = null;
            }

            _writer?.Close();
            _writer = null;
        }
    }

    internal class TimestampedFileListener : TraceListener
    {
        private readonly StreamWriter _writer;

        public TimestampedFileListener(StreamWriter writer) => _writer = writer;

        public override void Write(string? message) { }   // required override, unused

        public override void WriteLine(string? message)
        {
            if (!Logger.EnableLogging)
                return;
            _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} - {message}");
        }
    }
}
