using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rgLogger {
    public class ConsoleLogger : BaseLogger {
        [Flags]
        public enum outputStreams {
            STDOUT = 1,
            STDERR = 2,
            VS_Debug = 4,
            VS_Trace = 8
        };

        private outputStreams _outputTo = outputStreams.STDOUT;
        public outputStreams outputTo {
            get { return _outputTo; }
            set { _outputTo = value; }
        }

        public ConsoleLogger() {
            LineEnding = Console.Out.NewLine;
        }
        public ConsoleLogger(logLevel messageLevel) : this() {
            level = messageLevel;
        }

        internal override void WriteToLog(string message) {
            if((outputTo & outputStreams.STDOUT) != 0) { Console.WriteLine(message); }
            if((outputTo & outputStreams.STDERR) != 0) { Console.Error.WriteLine(message); }
            if((outputTo & outputStreams.VS_Debug) != 0) { Debug.WriteLine(message); }
            if((outputTo & outputStreams.VS_Trace) != 0) { Trace.WriteLine(message); }
        }
    }
}
