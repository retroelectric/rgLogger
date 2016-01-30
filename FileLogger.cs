using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace rgLogger {
    /// <summary>
    /// Write log messages to a text file.
    /// </summary>
    public class FileLogger : BaseLogger {
        private bool isClosed;

        /// <summary>
        /// Sets the path in the file system to create the log files in.
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// Sets if the log file should be appended to.
        /// </summary>
        public bool OverwriteLog { get; set; }

        /// <summary>
        /// Stream to write the log file.
        /// </summary>
        private StreamWriter logStream;

        /// <summary>
        /// Create a new file logger.
        /// </summary>
        /// <param name="filePath">The file path to create the log files in.</param>
        public FileLogger(string filePath) {
            this.filePath = filePath;
            Open();
        }

        /// <summary>
        /// Creates a new file logger.
        /// </summary>
        /// <param name="filePath">The file path to create the log files in.</param>
        /// <param name="messageLevel">The logging level to write messages at.</param>
        public FileLogger(string filePath, LogLevel messageLevel) {
            Level = messageLevel;
            this.filePath = filePath;
            Open();
        }

        /// <summary>
        /// Opens the stream to the log file.
        /// </summary>
        public void Open() {
            logStream = new StreamWriter(filePath, OverwriteLog);
            isClosed = false;
        }

        /// <summary>
        /// Writes a message to the log file.
        /// </summary>
        /// <param name="message"></param>
        internal override void WriteToLog(string message) {
            if(!isClosed) {
                logStream.WriteLine(message);
                logStream.Flush();
            }
        }

        /// <summary>
        /// Closes the stream to the log file.
        /// </summary>
        public void Close() {
            isClosed = true;
            if(!isClosed) {
                logStream.Flush();
                logStream.Close();
            }
        }

        /// <summary>
        /// Closes the stream to the log file if necessary.
        /// </summary>
        public override void Dispose(){
            if(!isDisposed) {
                Close();
                isDisposed = true;
            }
        }
    }
}
