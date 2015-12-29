using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace rgLogger {
    public class FileLogger : BaseLogger {
        private bool isClosed;
        public string filePath { get; set; }

        private bool _appendToLog = true;
        public bool AppendToLog {
            get { return _appendToLog; }
            set { _appendToLog = value; }
        }

        private StreamWriter logStream;

        public FileLogger(string filePath) {
            this.filePath = filePath;
            Open();
        }

        public FileLogger(string filePath, logLevel messageLevel) {
            level = messageLevel;
            this.filePath = filePath;
            Open();
        }

        public void Open() {
            logStream = new StreamWriter(filePath, AppendToLog);
            //logStream = System.IO.File.AppendText(filePath);
            isClosed = false;
        }

        internal override void WriteToLog(string message) {
            if(!isClosed) {
                logStream.WriteLine(message);
                logStream.Flush();
            }
        }

        public void Close() {
            isClosed = true;
            if(!isClosed) {
                logStream.Flush();
                logStream.Close();
            }
        }

        public override void Dispose(){
            if(!isDisposed) {
                Close();
                isDisposed = true;
            }
        }
    }
}
