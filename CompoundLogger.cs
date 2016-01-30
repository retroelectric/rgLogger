using System;
using System.Collections.Generic;
using System.Linq;

namespace rgLogger {
    /// <summary>
    /// Combines multiple loggers so more than action can be taken for every log entry.
    /// </summary>
    public class CompoundLogger : BaseLogger, IEnumerable<BaseLogger> {
        private List<BaseLogger> loggerList;
        public CompoundLogger() {
            loggerList = new List<BaseLogger>();
            Level = LogLevel.All;
        }
        /// <summary>
        /// Creates a new CompoundLogger
        /// </summary>
        /// <param name="newList"></param>
        public CompoundLogger(List<BaseLogger> newList) {
            loggerList = newList;
            Level = LogLevel.All;
        }

        /// <summary>
        /// Adds a new logger to the collection.
        /// </summary>
        /// <param name="newLogger">The configured logger to add.</param>
        public void Add(BaseLogger newLogger) {
            loggerList.Add(newLogger);
        }

        /// <summary>
        /// Writes a message to all of the loggers in the collection with the specified logging level.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="messageLevel">The logging level to report.</param>
        public override void Write(string message, LogLevel messageLevel) {
            foreach (BaseLogger l in loggerList) {
                l.Write(message, messageLevel);
            }
        }

        /// <summary>
        /// Writes a message to all of the loggers in the collection with their default logging level.
        /// </summary>
        /// <param name="message">The log message.</param>
        internal override void WriteToLog(string message) {
            foreach(BaseLogger l in loggerList) {
                l.WriteToLog(message);
            }
        }

        public IEnumerator<BaseLogger> GetEnumerator() {
            return loggerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public new void Dispose() {
            foreach(BaseLogger l in loggerList) {
                l.Dispose();
            }
        }
    }
}
