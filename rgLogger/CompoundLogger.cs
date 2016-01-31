using System;
using System.Collections.Generic;
using System.Linq;

namespace rgLogger {
    /// <summary>
    /// Combines multiple loggers so more than action can be taken for every log entry.
    /// </summary>
    public class CompoundLogger : BaseLogger, IDisposable, IEnumerable<BaseLogger> {
        /// <summary>
        /// Contains the collection of loggers.
        /// </summary>
        private List<BaseLogger> loggerList;

        /// <summary>
        /// Initializes a new instance of the CompoundLogger class.
        /// </summary>
        public CompoundLogger() {
            loggerList = new List<BaseLogger>();
            Level = LogLevel.All;
        }

        /// <summary>
        /// Initializes a new instance of the CompoundLogger class.
        /// </summary>
        /// <param name="loggerCollection">A collection of configured logging objects.</param>
        public CompoundLogger(List<BaseLogger> loggerCollection) {
            loggerList = loggerCollection;
            Level = LogLevel.All;
        }

        /// <summary>
        /// Adds a new logger to the collection.
        /// </summary>
        /// <param name="newLogger">The configured logging objects to add.</param>
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
        /// Enumerates over the loggers in the collection.
        /// </summary>
        /// <returns>The enumerator for the logging collection.</returns>
        public IEnumerator<BaseLogger> GetEnumerator() {
            return loggerList.GetEnumerator();
        }

        /// <summary>
        /// Enumerates over the loggers in the collection.
        /// </summary>
        /// <returns>The enumerator for the logging collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Disposes of all loggers in the collection that implement IDisposable.
        /// </summary>
        public void Dispose() {
            foreach (BaseLogger logger in loggerList) {
                if (logger is IDisposable) {
                    ((IDisposable)logger).Dispose();
                }
            }
        }

        /// <summary>
        /// Writes a message to all of the loggers in the collection with their default logging level.
        /// </summary>
        /// <param name="message">The log message.</param>
        internal override void WriteToLog(string message) {
            foreach (BaseLogger l in loggerList) {
                l.WriteToLog(message);
            }
        }
    }
}
