using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rgLogger {
    /// <summary>
    /// Sends a single email containing all of the log messages written to it.
    /// </summary>
    public class CumulativeEmailLogger : EmailLogger, IDisposable {
        /// <summary>
        /// Used to build the message body from the different log messages.
        /// </summary>
        private StringBuilder emailBody = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the CumulativeEmailLogger class.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        public CumulativeEmailLogger(string server) : base(server) { }

        /// <summary>
        /// Initializes a new instance of the CumulativeEmailLogger class.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        /// <param name="port">The port to connect to on the server.</param>
        public CumulativeEmailLogger(string server, int port) : base(server, port) { }

        /// <summary>
        /// Initializes a new instance of the CumulativeEmailLogger class.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        /// <param name="defaultLevel">Default logging level to use.</param>
        public CumulativeEmailLogger(string server, LogLevel defaultLevel) : base(server, defaultLevel) { }

        /// <summary>
        /// Initializes a new instance of the CumulativeEmailLogger class.
        /// </summary>
        /// <param name="server">The hostname or IP address of the SMTP server.</param>
        /// <param name="port">The port to connect to on the server.</param>
        /// <param name="defaultLevel">Default logging level to use.</param>
        public CumulativeEmailLogger(string server, int port, LogLevel defaultLevel) : base(server, port, defaultLevel) { }

        /// <summary>
        /// Gets or sets a value indicating whether the email should be sent when the object is disposed if it contains any content. (Default = true)
        /// </summary>
        public bool SendEmailOnDispose { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether emails should only be sent if they contain content.
        /// </summary>
        public bool SendEmptyEmails { get; set; }

        /// <summary>
        /// Sends the email and clears the message body ready to send the next email.
        /// </summary>
        public void SendEmail() {
            if (emailBody.Length > 0 || SendEmptyEmails) {
                base.WriteToLog(emailBody.ToString());
                emailBody.Clear();
            }
        }

        /// <summary>
        /// Disposes of the object and, depending on value of SendEmailOnDispose, sends the email as long as it contains some content.
        /// </summary>
        public override void Dispose() {
            if (SendEmailOnDispose) {
                SendEmail();
            }

            base.Dispose();
        }

        /// <summary>
        /// Write a log message to the email body.
        /// </summary>
        /// <param name="message">The log message.</param>
        internal override void WriteToLog(string message) {
            emailBody.AppendLine(message);
        }
    }
}