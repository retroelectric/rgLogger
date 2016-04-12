namespace rgLogger {
    public interface IBaseLogger {
        bool KeepLineEndings { get; set; }
        LogLevel Level { get; set; }
        string LineEnding { get; set; }
        string TimestampFormat { get; set; }
        bool TimestampInUtc { get; set; }

        void Dispose();
        string GetCurrentTimestamp();
        string MessagePrefix(LogLevel messageLevel);
        void Write(string message);
        void Write(string message, LogLevel messageLevel);
    }
}