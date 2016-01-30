namespace rgLogger {
    /// <summary>
    /// Logging detail levels
    /// </summary>
    public enum LogLevel {
        /// <summary>
        /// No log messages will match this level.
        /// </summary>
        None = 0,

        /// <summary>
        /// All log messages will match this level.
        /// </summary>
        All = 1,

        /// <summary>
        /// Fatal level.
        /// </summary>
        Fatal = 2,

        /// <summary>
        /// Warning level.
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Information level.
        /// </summary>
        Info = 4,

        /// <summary>
        /// Debug level.
        /// </summary>
        Debug = 5
    }
}
