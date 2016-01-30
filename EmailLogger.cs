using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ComponentModel;

namespace rgLogger {
    /// <summary>
    /// Sends an email for each log message written.
    /// </summary>
    public class EmailLogger : BaseLogger {
        /// <summary>
        /// SMTP server IP/hostname.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// SMTP server port. (Default=25)
        /// </summary>
        public int SmtpPort { get; set; } = 25;

        /// <summary>
        /// Username to login to the SMTP server.
        /// </summary>
        public string SmtpUser { get; set; }

        /// <summary>
        /// Password to login to the SMTP server.
        /// </summary>
        public string SmtpPassword { get; set; }

        /// <summary>
        /// Send email asynchronously.  (Default=true)
        /// </summary>
        public bool Asynchronous { get; set; } = true;
        
        /// <summary>
        /// Proxy server IP/hostname.
        /// </summary>
        public string ProxyServer { get; set; }

        /// <summary>
        /// Proxy server port.
        /// </summary>
        public string ProxyPort { get; set; }
        
        /// <summary>
        /// Address to send the emails from.
        /// </summary>
        public MailAddress Sender { get; set; }

        /// <summary>
        /// Collection of email addresses to send the emails to.
        /// </summary>
        public MailAddressCollection Recipient { get; set; }

        /// <summary>
        /// Reply-to address for the emails.
        /// </summary>
        public MailAddress ReplyTo { get; set; }

        /// <summary>
        /// Subject line for the emails.
        /// (Default="Log message from " + ProcessName)
        /// </summary>
        public string Subject { get; set; } = "Log message from " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        /// <summary>
        /// Shows if a message is currently being sent.
        /// </summary>
        public bool SendingMessage { get; private set; }

        private SmtpClient mailClient;
        private List<MailMessage> messageQueue = new List<MailMessage>();

        /// <summary>
        /// Create new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        public EmailLogger(string server) {
            SmtpServer = server;
            Connect();
        }
        
        /// <summary>
        /// Create new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        /// <param name="port">SMTP server's port.</param>
        public EmailLogger(string server, int port) {
            SmtpServer = server;
            SmtpPort = port;
            Connect();
        }

        /// <summary>
        /// Create a new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname</param>
        /// <param name="messageLevel">Level of logging to allow through.</param>
        public EmailLogger(string server, LogLevel messageLevel) : this(server) {
            Level = messageLevel;
        }

        /// <summary>
        /// Create a new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        /// <param name="port">SMTP server's port.</param>
        /// <param name="messageLevel">Level of logging to allow through.</param>
        public EmailLogger(string server, int port, LogLevel messageLevel) : this(server, port) {
            Level = messageLevel;
        }

        private void Connect() {
            mailClient = new SmtpClient(SmtpServer, SmtpPort);
            mailClient.SendCompleted += new SendCompletedEventHandler(SendCompleted);
        }

        /// <summary>
        /// Sends an email message with the message.
        /// </summary>
        /// <param name="message">Message for the email body.</param>
        internal override void WriteToLog(string message) {
            var m = new MailMessage() {
                From = Sender,
                Subject = Subject,
                Body = message,
            };

            m.ReplyToList.Add(ReplyTo);

            foreach (MailAddress r in Recipient) {
                m.To.Add(r);
            }

            if (Asynchronous) {
                if (SendingMessage) {
                    messageQueue.Add(m);
                }
                else {
                    SendingMessage = true;
                    mailClient.SendAsync(m, null);
                }
            }
            else {
                mailClient.Send(m);
            }
        }

        /// <summary>
        /// Event handler for when a mail is finished sending
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void SendCompleted(object Sender, AsyncCompletedEventArgs e) {
            if (messageQueue.Any()) {
                mailClient.SendAsync(messageQueue[0], null);
                messageQueue.RemoveAt(0);
            }
            else {
                SendingMessage = false;
            }
        }

        public void AddRecipient(MailAddress newRecipient) {
            if (Recipient == null) { Recipient = new MailAddressCollection(); }
            Recipient.Add(newRecipient);
        }

        public void AddRecipient(string newRecipient) {
            Recipient.Add(new MailAddress(newRecipient));
        }

        public void AddRecipient(List<MailAddress> newRecipients) {
            foreach (var m in newRecipients) {
                AddRecipient(m);
            }
        }

        public void AddRecipient(List<string> newRecipients) {
            foreach (var m in newRecipients) {
                AddRecipient(new MailAddress(m));
            }
        }

        public void AddRecipient(string[] newRecipients) {
            foreach (var m in newRecipients) {
                AddRecipient(new MailAddress(m));
            }
        }

        public override void Dispose() {
            base.Dispose();
        }
    }
}