using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ComponentModel;

namespace rgLogger {
    public class EmailLogger : BaseLogger {
        /// <summary>
        /// SMTP server IP/hostname.
        /// </summary>
        public string smtpServer { get; set; }
        
        /// <summary>
        /// SMTP server port. (Default=25)
        /// </summary>
        public int smtpPort {
            get { return _smtpPort; }
            set { _smtpPort = value; }
        }

        /// <summary>
        /// Username to login to the SMTP server.
        /// </summary>
        public string smtpUser { get; set; }

        /// <summary>
        /// Password to login to the SMTP server.
        /// </summary>
        public string smtpPassword { get; set; }

        /// <summary>
        /// Send email asynchronously.  (Default=true)
        /// </summary>
        public bool async {
            get { return _async; }
            set { _async = value; }
        }
        
        /// <summary>
        /// Proxy server IP/hostname.
        /// </summary>
        public string proxyServer { get; set; }

        /// <summary>
        /// Proxy server port.
        /// </summary>
        public string proxyPort { get; set; }
        
        /// <summary>
        /// Address to send the emails from.
        /// </summary>
        public MailAddress sender { get; set; }

        /// <summary>
        /// Collection of email addresses to send the emails to.
        /// </summary>
        public MailAddressCollection recipient { get; set; }

        /// <summary>
        /// Reply-to address for the emails.
        /// </summary>
        public MailAddress reply_to { get; set; }

        /// <summary>
        /// Subject line for the emails.
        /// (Default="Log message from " + ProcessName)
        /// </summary>
        public string subject {
            get { return _subject; }
            set { _subject = value; }
        }

        private bool _sendingMessage = false;
        /// <summary>
        /// Shows if a message is currently being sent.
        /// </summary>
        public bool sendingMessage {
            get { return _sendingMessage; }
        }

        private bool _async = true;        
        private int _smtpPort = 25;
        private string _subject = "Log message from " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        private SmtpClient mailClient;
        private List<MailMessage> messageQueue = new List<MailMessage>();

        /// <summary>
        /// Create new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        public EmailLogger(string server) {
            smtpServer = server;
            Connect();
        }
        
        /// <summary>
        /// Create new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        /// <param name="port">SMTP server's port.</param>
        public EmailLogger(string server, int port) {
            smtpServer = server;
            smtpPort = port;
            Connect();
        }

        /// <summary>
        /// Create a new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname</param>
        /// <param name="messageLevel">Level of logging to allow through.</param>
        public EmailLogger(string server, logLevel messageLevel) : this(server) {
            level = messageLevel;
        }

        /// <summary>
        /// Create a new email logger.
        /// </summary>
        /// <param name="server">SMTP server's IP/hostname.</param>
        /// <param name="port">SMTP server's port.</param>
        /// <param name="messageLevel">Level of logging to allow through.</param>
        public EmailLogger(string server, int port, logLevel messageLevel) : this(server, port) {
            level = messageLevel;
        }

        private void Connect() {
            mailClient = new SmtpClient(smtpServer, smtpPort);
            mailClient.SendCompleted += new SendCompletedEventHandler(SendCompleted);
        }

        /// <summary>
        /// Sends an email message with the message.
        /// </summary>
        /// <param name="message">Message for the email body.</param>
        internal override void WriteToLog(string message) {
            var m = new MailMessage() {
                From = sender,
                Subject = subject,
                Body = message,
                ReplyTo = reply_to
            };
            foreach (MailAddress r in recipient) {
                m.To.Add(r);
            }

            if (async) {
                if (sendingMessage) {
                    messageQueue.Add(m);
                }
                else {
                    _sendingMessage = true;
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
                _sendingMessage = false;
            }
        }

        public void AddRecipient(MailAddress newRecipient) {
            if (recipient == null) { recipient = new MailAddressCollection(); }
            recipient.Add(newRecipient);
        }

        public void AddRecipient(string newRecipient) {
            recipient.Add(new MailAddress(newRecipient));
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