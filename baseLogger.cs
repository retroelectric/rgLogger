using System;
using System.Collections.Generic;
using System.Linq;

namespace rgLogger {
    /// <summary>
    /// Logging detail levels
    /// </summary>
    public enum logLevel {
        None = 0,
        All = 1,
        Fatal = 2,
        Warn = 3,
        Info = 4,
        Debug = 5
    };

    /// <summary>
    /// A base class for all logging objects to implement that provides consistent
    /// message formating and filtering.
    /// </summary>
    public abstract class BaseLogger : IDisposable {

        internal bool isDisposed;

        private logLevel _level = logLevel.Debug;
        /// <summary>
        /// the default log level for messages
        /// </summary>
        public logLevel level {
            get { return _level; }
            set { _level = value; }
        }

        private string _timestamp = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// Format string for the timestamp
        /// </summary>
        public string timestamp {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        /// <summary>
        /// Show the timestamp in UTC?
        /// </summary>
        public bool timestampInUTC { get; set; }

        internal bool sendMessage(logLevel messageLevel) {
            if (level != logLevel.None && messageLevel != logLevel.None) {
                if (level == logLevel.All || messageLevel <= level) { return true; }
            }

            return false;
        }

        private bool _replaceLineEndings = true;
        public bool ReplaceLineEndings {
            get { return _replaceLineEndings; }
            set { _replaceLineEndings = value; }
        }

        private string _lineEnding = Environment.NewLine;
        public string LineEnding {
            get { return _lineEnding; }
            set { _lineEnding = value; }
        }

        internal string FixLineEndings(string message) {
            // line endings ordered by length except the one matching the proper line ending
            var otherLineEndings = from s in new string[] { "\n\r", "\r\n", "\n", "\r" }
                            where s!=LineEnding
                            orderby s.Length descending
                            select s;

            foreach (string nl in otherLineEndings) {
                message = message.Replace(nl, LineEnding);
            }

            return message;
        }

        /// <summary>
        /// Write a message to the logger
        /// </summary>
        /// <param name="message">Message text to log</param>
        /// <param name="messageLevel">Level of detail for this message</param>
        public virtual void Write(string message, logLevel messageLevel) {
            if (sendMessage(messageLevel)) {
                if (ReplaceLineEndings) { message = FixLineEndings(message); }
                WriteToLog(String.Format("{0} {1}", MessagePrefix(messageLevel), message));
            }
        }

        /// <summary>
        /// Generates the prefix for the log message text
        /// </summary>
        /// <param name="messageLevel">Level of detail for this message</param>
        /// <returns>prefix for the log message text</returns>
        public string MessagePrefix(logLevel messageLevel) {
            string messagePrefix = "";

            if (messageLevel != logLevel.All) {
                messagePrefix += String.Format("[{0}] ", messageLevel.ToString().ToUpper());
            }

            return String.Format("{0} {1}", GetCurrentTimestamp(), messagePrefix);
        }

        /// <summary>
        /// Overload of Write that calls Write(string,logLevel) with the default message level
        /// </summary>
        /// <param name="message">Message text to log</param>
        public void Write(string message) {
            Write(message, level);
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
            if (timestamp.Length == 0) { return ""; }

            return (timestampInUTC) ? DateTime.UtcNow.ToString(timestamp) : DateTime.Now.ToString(timestamp);
        }

        public virtual void Dispose() {
            if (isDisposed) { return; }

            isDisposed = true;
        }
    }
}