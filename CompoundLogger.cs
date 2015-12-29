using System;
using System.Collections.Generic;
using System.Linq;

namespace rgLogger {
    public class CompoundLogger : BaseLogger, IEnumerable<BaseLogger> {
        private List<BaseLogger> loggerList;

        public CompoundLogger() {
            loggerList = new List<BaseLogger>();
            level = logLevel.All;
        }

        public CompoundLogger(List<BaseLogger> newList) {
            loggerList = newList;
            level = logLevel.All;
        }

        public void Add(BaseLogger newLogger) {
            loggerList.Add(newLogger);
        }

        public override void Write(string message, logLevel messageLevel) {
            foreach (BaseLogger l in loggerList) {
                l.Write(message, messageLevel);
            }
        }

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
