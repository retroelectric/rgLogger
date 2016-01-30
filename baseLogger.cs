using System;
using System.Collections.Generic;
using System.Linq;

namespace rgLogger {


    /// <summary>
    /// A base class for all logging objects to implement that provides consistent
    /// message formating and filtering.
    /// </summary>
    public abstract class BaseLogger : IDisposable {
        protected bool isDisposed;

        /// <summary>
        /// the default log level for messages
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Format string for the timestamp
        /// </summary>
        public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Show the timestamp in UTC?
        /// </summary>
        public bool TimestampInUtc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageLevel"></param>
        /// <returns></returns>
        protected bool sendMessage(LogLevel messageLevel) {
            if (Level != LogLevel.None && messageLevel != LogLevel.None) {
                if (Level == LogLevel.All || messageLevel <= Level) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Sets whether to keep the line endings or replace them with the value of the LineEnding parameter.
        /// </summary>
        public bool KeepLineEndings { get; set; }

        /// <summary>
        /// If KeepLineEndings is true then the line endings of the log messages will be replaced by this. Default is Environment.NewLine
        /// </summary>
        public string LineEnding { get; set; } = Environment.NewLine;

        /// <summary>
        /// Returns a string with the line endings replaced.
        /// </summary>
        /// <param name="OriginalString">The string to replace the line endings in.</param>
        /// <returns>The original string with the line endings converted.</returns>
        internal string FixLineEndings(string OriginalString) {
            // line endings ordered by length except the one matching the proper line ending
            var otherLineEndings = from s in new string[] { "\n\r", "\r\n", "\n", "\r" }
                            where s!=LineEnding
                            orderby s.Length descending
                            select s;

            string[] allLineEndings = { "\n\r", "\r\n", "\n", "\r" };

            foreach (string nl in otherLineEndings) {
                OriginalString = OriginalString.Replace(nl, LineEnding);
            }

            return OriginalString;
        }

        /// <summary>
        /// Write a message to the logger
        /// </summary>
        /// <param name="message">Message text to log</param>
        /// <param name="messageLevel">Level of detail for this message</param>
        public virtual void Write(string message, LogLevel messageLevel) {
            if (sendMessage(messageLevel)) {
                if (!KeepLineEndings) { message = FixLineEndings(message); }
                WriteToLog(String.Format("{0} {1}", MessagePrefix(messageLevel), message));
            }
        }

        /// <summary>
        /// Generates the prefix for the log message text
        /// </summary>
        /// <param name="messageLevel">Level of detail for this message</param>
        /// <returns>prefix for the log message text</returns>
        public string MessagePrefix(LogLevel messageLevel) {
            string messagePrefix = "";

            if (messageLevel != LogLevel.All) {
                messagePrefix += String.Format("[{0}] ", messageLevel.ToString().ToUpper());
            }

            return String.Format("{0} {1}", GetCurrentTimestamp(), messagePrefix);
        }

        /// <summary>
        /// Overload of Write that calls Write(string,logLevel) with the default message level
        /// </summary>
        /// <param name="message">Message text to log</param>
        public void Write(string message) {
            Write(message, Level);
        }

        /// <summary>
        /// Abstract method used to actually write the log message to the destination.
        /// </summary>
        /// <param name="message">Log message to write.</param>
        internal abstract void WriteToLog(string message);

        /// <summary>
        /// gets the current timestamp
        /// </summary>
        /// <returns>a string containing the current time</returns>
        public string GetCurrentTimestamp() {
            if (string.IsNullOrEmpty(TimestampFormat)) { return ""; }

            return (TimestampInUtc) ? DateTime.UtcNow.ToString(TimestampFormat) : DateTime.Now.ToString(TimestampFormat);
        }

        public virtual void Dispose() {
            if (isDisposed) { return; }

            isDisposed = true;
        }
    }
}