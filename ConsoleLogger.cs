using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rgLogger {
    /// <summary>
    /// Sends log messages to the console window.
    /// </summary>
    public class ConsoleLogger : BaseLogger {
        public ConsoleLoggerOutput OutputTo { get; set; } = ConsoleLoggerOutput.STDOUT;

        /// <summary>
        /// Create a new console logger using the default logging level.
        /// </summary>
        public ConsoleLogger() {
            LineEnding = Console.Out.NewLine;
        }

        /// <summary>
        /// Create a new console logger using the specified logging level.
        /// </summary>
        /// <param name="messageLevel"></param>
        public ConsoleLogger(LogLevel messageLevel) : this() {
            Level = messageLevel;
        }

        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="message">The log message.</param>
        internal override void WriteToLog(string message) {
            if((OutputTo & ConsoleLoggerOutput.STDOUT) != 0) { Console.WriteLine(message); }
            if((OutputTo & ConsoleLoggerOutput.STDERR) != 0) { Console.Error.WriteLine(message); }
            if((OutputTo & ConsoleLoggerOutput.VS_Debug) != 0) { Debug.WriteLine(message); }
            if((OutputTo & ConsoleLoggerOutput.VS_Trace) != 0) { Trace.WriteLine(message); }
        }
    }
}
