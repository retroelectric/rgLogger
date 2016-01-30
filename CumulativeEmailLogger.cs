using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rgLogger {
    public class CumulativeEmailLogger : EmailLogger {
        /// <summary>
        /// This will ensure that the email always gets sent by calling SendEmail() from
        /// the Dispose() method if the emailBody is not empty.
        /// </summary>
        public bool SendEmailOnDispose { get; set; } = true;

        private StringBuilder emailBody = new StringBuilder();

        /// <summary>
        /// Creates a cumulative email logger.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        public CumulativeEmailLogger(string server) : base(server) { }

        /// <summary>
        /// Creates a cumulative email logger.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        /// <param name="port">The port to connect to on the server.</param>
        public CumulativeEmailLogger(string server, int port) : base(server, port) { }

        /// <summary>
        /// Creates a cumulative email logger.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        /// <param name="defaultLevel">Default logging level to use.</param>
        public CumulativeEmailLogger(string server, LogLevel defaultLevel) : base(server, defaultLevel) { }

        /// <summary>
        /// Creates a cumulative email logger.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        /// <param name="port">The port to connect to on the server.</param>
        /// <param name="defaultLevel">Default logging level to use.</param>
        public CumulativeEmailLogger(string server, int port, LogLevel defaultLevel) : base(server, port, defaultLevel) { }

        /// <summary>
        /// Write a log message to the email body.
        /// </summary>
        /// <param name="message">The log message.</param>
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