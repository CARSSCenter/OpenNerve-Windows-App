using System.Diagnostics;

namespace controller;

internal sealed class UiDebugTraceListener : TraceListener
{
    private const int MaxChars = 32_000;
    private readonly TextBox _target;

    public UiDebugTraceListener(TextBox target) => _target = target;

    public override void Write(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Append(message);
        }
    }

    public override void WriteLine(string? message) => Append(message);

    private void Append(string? message)
    {
        if (string.IsNullOrEmpty(message) || _target.IsDisposed)
        {
            return;
        }

        var line = $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}";

        void Update()
        {
            _target.AppendText(line);
            if (_target.TextLength > MaxChars)
            {
                _target.Text = _target.Text[^MaxChars..];
            }
        }

        if (_target.InvokeRequired)
        {
            _target.BeginInvoke(Update);
        }
        else
        {
            Update();
        }
    }
}
