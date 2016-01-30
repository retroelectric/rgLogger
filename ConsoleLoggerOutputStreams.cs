using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    [Flags]
    public enum ConsoleLoggerOutput {
        STDOUT = 1,
        STDERR = 2,
        VS_Debug = 4,
        VS_Trace = 8
    };
}
