using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;

namespace rgLogger {
    /// <summary>
    /// Sends an email for each log message written.
    /// </summary>
    public class EmailLogger : BaseLogger, IDisposable {
        /// <summary>
        /// The SmtpClient object to use to send emails.
        /// </summary>
        private SmtpClient mailClient;

        /// <summary>
        /// Stores the queue of email messages to send.
        /// </summary>
        private List<MailMessage> messageQueue = new List<MailMessage>();

        /// <summary>
        /// Backing field for the ReplyTo property.
        /// </summary>
        private MailAddress _replyTo;

        /// <summary>
        /// Initializes a new instance of the EmailLogger class using the provided SmtpClient.
        /// </summary>
        /// <param name="client">A configured SmtpClient object.</param>
        public EmailLogger(SmtpClient client) {
            mailClient = client;
            mailClient.SendCompleted += new SendCompletedEventHandler(SendCompleted);
        }

        /// <summary>
        /// Initializes a new instance of the EmailLogger class.
        /// </summary>
        /// <param name="server">Hostname or IP address of the SMTP server.</param>
        /// <param name="port">The port to use when connecting to the SMTP server. (Default = 25)</param>
        public EmailLogger(string server, int port = 25)
            : this(new SmtpClient() { Host = server, Port = port }) { }

        /// <summary>
        /// Initializes a new instance of the EmailLogger class.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname</param>
        /// <param name="messageLevel">Level of logging to allow through.</param>
        public EmailLogger(string server, LogLevel messageLevel) : this(server) {
            Level = messageLevel;
        }

        /// <summary>
        /// Initializes a new instance of the EmailLogger class.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        /// <param name="port">SMTP server's port.</param>
        /// <param name="messageLevel">Level of logging to allow through.</param>
        public EmailLogger(string server, int port, LogLevel messageLevel) : this(server, port) {
            Level = messageLevel;
        }

        /// <summary>
        /// Gets or sets the SMTP server IP address or hostname.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the port used to connect to the SMTP server.
        /// </summary>
        public int SmtpPort { get; set; } = 25;

        /// <summary>
        /// Gets or sets the username to use when logging into the SMTP server.
        /// </summary>
        public string SmtpUser { get; set; }

        /// <summary>
        /// Gets or sets the password used to login to the SMTP server.
        /// </summary>
        public string SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email should be sent asynchronously. (Default = true)
        /// </summary>
        public bool Asynchronous { get; set; } = true;

        /// <summary>
        /// Gets or sets the proxy server IP address or hostname.
        /// </summary>
        public string ProxyServer { get; set; }

        /// <summary>
        /// Gets or sets the port used to connect to the proxy server.
        /// </summary>
        public string ProxyPort { get; set; }

        /// <summary>
        /// Gets or sets the email address to send emails from.
        /// </summary>
        public MailAddress Sender { get; set; }

        /// <summary>
        /// Gets or sets a collection of email addresses to be the recipients of the email.
        /// </summary>
        public MailAddressCollection Recipient { get; set; } = new MailAddressCollection();

        /// <summary>
        /// Gets or sets the email address to set as the reply to address.
        /// </summary>
        public MailAddress ReplyTo {
            get {
                return _replyTo ?? Sender;
            }

            set {
                _replyTo = value;
            }
        }

        /// <summary>
        /// Gets or sets the subject line for the emails. (Default = "Log message from ProcessName")
        /// </summary>
        public string Subject { get; set; } = "Log message from " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        /// <summary>
        /// Gets a value indicating whether a message is currently being sent.
        /// </summary>
        public bool SendingMessage { get; private set; }

        /// <summary>
        /// Adds another recipient to the log messages.
        /// </summary>
        /// <param name="newRecipient">A MailAddress object for the new recipient.</param>
        public void AddRecipient(MailAddress newRecipient) {
            Recipient.Add(newRecipient);
        }

        /// <summary>
        /// Adds another recipient to the log messages.
        /// </summary>
        /// <param name="newRecipient">Email address of the recipient.</param>
        public void AddRecipient(string newRecipient) {
            AddRecipient(new MailAddress(newRecipient));
        }

        /// <summary>
        /// Adds multiple recipients to the log messages.
        /// </summary>
        /// <param name="newRecipients">A collection of MailAddress objects for the new recipients.</param>
        public void AddRecipient(IEnumerable<MailAddress> newRecipients) {
            foreach (var m in newRecipients) {
                AddRecipient(m);
            }
        }

        /// <summary>
        /// Adds multiple recipients to the log messages.
        /// </summary>
        /// <param name="newRecipients">A collection of email addresses for the new recipients.</param>
        public void AddRecipient(IEnumerable<string> newRecipients) {
            foreach (var m in newRecipients) {
                AddRecipient(new MailAddress(m));
            }
        }

        /// <summary>
        /// Disposes of the EmailLogger class.
        /// </summary>
        public virtual void Dispose() {
            mailClient.Dispose();
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
        /// Event handler for when a mail is finished sending.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">AsyncCompletedEventArgs object</param>
        private void SendCompleted(object sender, AsyncCompletedEventArgs e) {
            if (messageQueue.Any()) {
                mailClient.SendAsync(messageQueue[0], null);
                messageQueue.RemoveAt(0);
            }
            else {
                SendingMessage = false;
            }
        }
    }
}