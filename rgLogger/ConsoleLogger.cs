using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rgLogger {
    /// <summary>
    /// Sends log messages to the console window.
    /// </summary>
    public class ConsoleLogger : BaseLogger {
        /// <summary>
        /// Initializes a new instance of the ConsoleLogger class with the default logging level.
        /// </summary>
        public ConsoleLogger() {
            LineEnding = Console.Out.NewLine;
        }

        /// <summary>
        /// Initializes a new instance of the ConsoleLogger class with the specified logging level.
        /// </summary>
        /// <param name="messageLevel">The logging level to use.</param>
        public ConsoleLogger(LogLevel messageLevel) : this() {
            Level = messageLevel;
        }

        /// <summary>
        /// Gets or sets which outputs to write log messages to.
        /// </summary>
        public ConsoleLoggerOutput OutputTo { get; set; } = ConsoleLoggerOutput.ConsoleStandardOutput;

        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="message">The log message.</param>
        internal override void WriteToLog(string message) {
            if (OutputTo.HasFlag(ConsoleLoggerOutput.ConsoleStandardOutput)) {
                Console.Write(message);
            }

            if (OutputTo.HasFlag(ConsoleLoggerOutput.ConsoleErrorOutput)) {
                Console.Error.WriteLine(message);
            }

            if (OutputTo.HasFlag(ConsoleLoggerOutput.VisualStudioDebugWindow)) {
                Debug.WriteLine(message);
            }

            if (OutputTo.HasFlag(ConsoleLoggerOutput.VisualStudioTraceWindow)) {
                Trace.WriteLine(message);
            }
        }
    }
}
