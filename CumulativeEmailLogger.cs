using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rgLogger {
    public class CumulativeEmailLogger : EmailLogger {
        private bool _sendEmailOnDispose = true;
        /// <summary>
        /// This will ensure that the email always gets sent by calling SendEmail() from
        /// the Dispose() method if the emailBody is not empty.
        /// </summary>
        public bool SendEmailOnDispose {
            get { return _sendEmailOnDispose; }
            set { _sendEmailOnDispose = value; }
        }

        private StringBuilder emailBody = new StringBuilder();

        public CumulativeEmailLogger(string server) : base(server) { }
        public CumulativeEmailLogger(string server, int port) : base(server, port) { }
        public CumulativeEmailLogger(string server, logLevel defaultLevel) : base(server, defaultLevel) { }
        public CumulativeEmailLogger(string server, int port, logLevel defaultLevel) : base(server, port, defaultLevel) { }

        internal override void WriteToLog(string message) {
            emailBody.AppendLine(message);
        }

        /// <summary>
        /// Sends the email and clears the message body.
        /// </summary>
        public void SendEmail() {
            base.WriteToLog(emailBody.ToString());
            emailBody = new StringBuilder();
        }

        public override void Dispose() {
            base.Dispose();
            if (emailBody.Length > 0) { SendEmail(); }
        }
    }
}