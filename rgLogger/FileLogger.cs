using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace rgLogger {
    /// <summary>
    /// Write log messages to a text file.
    /// </summary>
    public class FileLogger : BaseLogger, IDisposable {
        /// <summary>
        /// Stream used to write to the log file.
        /// </summary>
        private StreamWriter logStream;

        /// <summary>
        /// Initializes a new instance of the FileLogger class.
        /// </summary>
        /// <param name="filePath">The file path to create the log files in.</param>
        /// <param name="messageLevel">The logging level to write messages at.</param>
        public FileLogger(string filePath, LogLevel messageLevel) {
            Level = messageLevel;
            this.FilePath = filePath;
            Open();
        }

        /// <summary>
        /// Initializes a new instance of the FileLogger class.
        /// </summary>
        /// <param name="filePath">The file path to create the log files in.</param>
        public FileLogger(string filePath) {
            this.FilePath = filePath;
            Open();
        }

        /// <summary>
        /// Gets a value indicating whether the file is currently open.
        /// </summary>
        public bool FileIsOpen { get; private set; }

        /// <summary>
        /// Gets or sets the file path to create the log files in.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the log file or append to it. (Default = false; Append to the log.)
        /// </summary>
        public bool OverwriteLog { get; set; }

        /// <summary>
        /// Opens the stream to the log file.
        /// </summary>
        public void Open() {
            if (!FileIsOpen) {
                logStream = new StreamWriter(FilePath, OverwriteLog);
                FileIsOpen = true;
            }
        }

        /// <summary>
        /// Closes the stream to the log file.
        /// </summary>
        public void Close() {
            if (FileIsOpen) {
                logStream.Flush();
                logStream.Close();
                FileIsOpen = false;
            }
        }

        /// <summary>
        /// Closes the stream to the log file if necessary.
        /// </summary>
        public void Dispose() {
            Close();
        }

        /// <summary>
        /// Writes a message to the log file.
        /// </summary>
        /// <param name="logMessage">Log message to write.</param>
        internal override void WriteToLog(string logMessage) {
            if (FileIsOpen) {
                logStream.WriteLine(logMessage);
                logStream.Flush();
            }
        }
    }
}
