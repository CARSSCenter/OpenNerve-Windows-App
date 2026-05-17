using System.Diagnostics;

namespace controller;

internal static class Logger
{
    public static bool EnableLogging = true;

    private static StreamWriter? _writer;
    private static TimestampedFileListener? _fileListener;

    public static string LogDirectory { get; private set; } =
        AppDomain.CurrentDomain.BaseDirectory;

    public static string? LogFileName { get; private set; }

    public static string? CurrentLogPath { get; private set; }

    public static void Configure(string? directory, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(directory))
        {
            LogDirectory = directory.Trim();
        }

        LogFileName = string.IsNullOrWhiteSpace(fileName) ? null : fileName.Trim();
    }

    public static void Initialize()
    {
        if (!EnableLogging || _writer != null)
        {
            return;
        }

        var path = ResolveLogPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        _writer = new StreamWriter(path, append: false) { AutoFlush = true };
        _fileListener = new TimestampedFileListener(_writer);
        Trace.Listeners.Add(_fileListener);
        CurrentLogPath = path;
    }

    public static void Reinitialize()
    {
        Close();
        if (EnableLogging)
        {
            Initialize();
        }
    }

    public static void Close()
    {
        if (_fileListener != null)
        {
            Trace.Listeners.Remove(_fileListener);
            _fileListener = null;
        }

        _writer?.Close();
        _writer = null;
        CurrentLogPath = null;
    }

    private static string ResolveLogPath()
    {
        var fileName = LogFileName;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"OpenNerve_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        }
        else if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".txt";
        }

        return Path.Combine(LogDirectory, fileName);
    }
}

internal class TimestampedFileListener : TraceListener
{
    private readonly StreamWriter _writer;

    public TimestampedFileListener(StreamWriter writer) => _writer = writer;

    public override void Write(string? message)
    {
    }

    public override void WriteLine(string? message)
    {
        if (!Logger.EnableLogging)
        {
            return;
        }

        _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} - {message}");
    }
}
