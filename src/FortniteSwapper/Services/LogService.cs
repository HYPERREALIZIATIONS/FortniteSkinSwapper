using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FortniteSwapper.Services;

public interface ILogService
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
    IReadOnlyList<string> Recent { get; }
    event Action<string>? EntryAdded;
}

public class LogService : ILogService
{
    private readonly object _lock = new();
    private readonly List<string> _recent = new();
    private readonly string _logDir;

    public LogService(string? logDir = null)
    {
        _logDir = logDir ?? Paths.LogsDir;
        Directory.CreateDirectory(_logDir);
    }

    public IReadOnlyList<string> Recent
    {
        get { lock (_lock) { return _recent.ToList(); } }
    }

    public event Action<string>? EntryAdded;

    public void Info(string message) => Write("INFO", message, null);
    public void Warn(string message) => Write("WARN", message, null);
    public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private void Write(string level, string message, Exception? ex)
    {
        var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var line = ex is null
            ? $"[{ts}] [{level}] {message}"
            : $"[{ts}] [{level}] {message} | {ex}";

        lock (_lock)
        {
            _recent.Add(line);
            if (_recent.Count > 500) _recent.RemoveAt(0);
            var file = Path.Combine(_logDir, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
            File.AppendAllText(file, line + Environment.NewLine);
        }

        EntryAdded?.Invoke(line);
    }
}
