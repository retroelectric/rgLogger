using System;
using System.Collections.Generic;
using System.Linq;

namespace rgLogger {
    /// <summary>
    /// A base class for all logging objects to implement that provides consistent
    /// message formatting and filtering.
    /// </summary>
    public abstract class BaseLogger : IDisposable {
        /// <summary>
        /// Gets or sets the log level to send messages at.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Gets or sets the timestamp format to use for log messages.
        /// </summary>
        public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Gets or sets a value indicating whether the timestamp should be in UTC.
        /// </summary>
        public bool TimestampInUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep the existing line endings or replace them with the value of the LineEnding parameter.
        /// </summary>
        public bool KeepLineEndings { get; set; }

        /// <summary>
        /// Gets or sets the line ending to use when replacing them if (KeepLineEndings == false). Default value is Environment.NewLine
        /// </summary>
        public string LineEnding { get; set; } = Environment.NewLine;

        /// <summary>
        /// Write a message to the logger
        /// </summary>
        /// <param name="message">Message text to log</param>
        /// <param name="messageLevel">Level of detail for this message</param>
        public virtual void Write(string message, LogLevel messageLevel) {
            if (CheckMessageLevel(messageLevel)) {
                if (!KeepLineEndings) {
                    message = FixLineEndings(message);
                }

                WriteToLog(string.Format("{0} {1}", MessagePrefix(messageLevel), message));
            }
        }

        /// <summary>
        /// Generates the prefix for the log message text
        /// </summary>
        /// <param name="messageLevel">Level of detail for this message</param>
        /// <returns>prefix for the log message text</returns>
        public string MessagePrefix(LogLevel messageLevel) {
            var logLevelText = (messageLevel == LogLevel.All) ? string.Empty : $"[{ messageLevel.ToString().ToUpper() }]";

            return $"{ GetCurrentTimestamp() } { logLevelText }";
        }

        /// <summary>
        /// Overload of Write that calls Write(string,logLevel) with the default message level
        /// </summary>
        /// <param name="message">Message text to log</param>
        public void Write(string message) {
            Write(message, Level);
        }

        /// <summary>
        /// gets the current timestamp
        /// </summary>
        /// <returns>a string containing the current time</returns>
        public string GetCurrentTimestamp() {
            if (string.IsNullOrEmpty(TimestampFormat)) {
                return string.Empty;
            }

            return (TimestampInUtc) ? DateTime.UtcNow.ToString(TimestampFormat) : DateTime.Now.ToString(TimestampFormat);
        }

        /// <summary>
        /// Abstract method used to actually write the log message to the destination.
        /// </summary>
        /// <param name="message">Log message to write.</param>
        internal abstract void WriteToLog(string message);

        /// <summary>
        /// Returns a string with the line endings replaced.
        /// </summary>
        /// <param name="originalString">The string to replace the line endings in.</param>
        /// <returns>The original string with the line endings converted.</returns>
        internal string FixLineEndings(string originalString) {
            // line endings ordered by length except the one matching the proper line ending
            var otherLineEndings = from s in new string[] { "\n\r", "\r\n", "\n", "\r" }
                                   where s != LineEnding
                                   orderby s.Length descending
                                   select s;

            string[] allLineEndings = { "\n\r", "\r\n", "\n", "\r" };

            foreach (string nl in otherLineEndings) {
                originalString = originalString.Replace(nl, LineEnding);
            }

            return originalString;
        }

        /// <summary>
        /// Checks if the message level meets the level set in the Level property.
        /// </summary>
        /// <param name="messageLevel">The log level to check.</param>
        /// <returns>True or false indicating if the log message meets the level.</returns>
        protected bool CheckMessageLevel(LogLevel messageLevel) {
            if (Level != LogLevel.None && messageLevel != LogLevel.None) {
                if (Level == LogLevel.All || messageLevel <= Level) {
                    return true;
                }
            }

            return false;
        }

        public virtual void Dispose() { }
    }
}