using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    /// <summary>
    /// Directs where to send the console logger output. Multiple targets can be specified by combining these flags.
    /// </summary>
    [Flags]
    public enum ConsoleLoggerOutput {
        /// <summary>
        /// Writes to the console's standard output.
        /// </summary>
        ConsoleStandardOutput = 1,

        /// <summary>
        /// Writes to the console's error output.
        /// </summary>
        ConsoleErrorOutput = 2,

        /// <summary>
        /// Writes to Visual Studio's debug window.
        /// </summary>
        VisualStudioDebugWindow = 4,

        /// <summary>
        /// Writes to Visual Studio's trace window.
        /// </summary>
        VisualStudioTraceWindow = 8
    }
}
