using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;

namespace rgLogger {
    public class Notifier : IDisposable {
        private string _dataFile = "rgnotify.dat";
        public string NotificationHistoryFile {
            get {
                return _dataFile;
            }
            set {
                if (value != _dataFile) {
                    _dataFile = value;
                    _history = null;
                }
            }
        }

        public int DaysToWait { get; set; } = -1;

        public string Sender { get; set; }

        public string ReplyTo {
            get {
                return _replyTo ?? Sender;
            }
            set {
                _replyTo = value;
            }
        }

        private SmtpClient mailClient;
        private string _replyTo;
        private List<Notification> notifications = new List<Notification>();
        private List<NotificationMessage> _history;
        private List<NotificationMessage> notificationHistory {
            get {
                if (_history == null) {
                    LoadNotificationHistory();
                }

                return _history;
            }
        }

        public Notifier(string mailServer) : this(new SmtpClient() { Host = mailServer }) { }

        public Notifier(SmtpClient client) {
            mailClient = client;
        }

        public void AddNotification(string name, string subjectPrefix, string recipientEmail) {
            AddNotification(name, subjectPrefix, new List<string> { recipientEmail });
        }

        public void AddNotification(string name, string subjectPrefix, IEnumerable<string> recipientEmails) {
            var n = new Notification() {
                Name = name,
                EmailSubjectPrefix = subjectPrefix
            };

            foreach(var r in recipientEmails) {
                n.Recipients.Add(r);
            }
        }

        public void AddNotification(Notification notification) {
            if (notifications.Where(n => n.Name == notification.Name).FirstOrDefault() == null) {
                notifications.Add(notification);
            }
        }

        public void SendNotification(string notificationName, string message) {
            SendNotification(notificationName, message, string.Empty);
        }

        public void SendNotification(string notificationName, string content, string subjectSuffix) {
            var notification = notifications.Where(n => n.Name == notificationName).FirstOrDefault();
            if (notification != null) {
                var message = new NotificationMessage() {
                    NotificationDetails = notification,
                    Content = content,
                    SubjectSuffix = subjectSuffix,
                    NotificationUsed = true
                };

                if (NotificationMessageIsNew(message)) {
                    SendNotification(message);
                }
            }
        }

        private void LoadNotificationHistory() {
            try {
                using (var fstream = new FileStream(NotificationHistoryFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var xserial = new XmlSerializer(typeof(List<NotificationMessage>), new XmlRootAttribute("NotificationHistory"));

                    _history = (List<NotificationMessage>)xserial.Deserialize(fstream);
                }
            }
            catch (FileNotFoundException) {
                _history = new List<NotificationMessage>();
            }
        }

        private void SaveNotificationHistory() {
            if (_history != null) {
                using (var xwriter = new XmlTextWriter(NotificationHistoryFile, Encoding.UTF8)) {
                    xwriter.Formatting = Formatting.Indented;
                    var xserial = new XmlSerializer(typeof(List<NotificationMessage>), new XmlRootAttribute("NotificationHistory"));
                    xserial.Serialize(xwriter, _history.Where(n => n.NotificationUsed).ToList());
                }
            }
            else {
                if (File.Exists(NotificationHistoryFile)) {
                    File.Delete(NotificationHistoryFile);
                }
            }
        }

        private bool NotificationMessageIsNew(NotificationMessage message) {
            if (DaysToWait <= 0) {
                return true;
            }

            if (notificationHistory.Count() == 0) {
                return true;
            }

            var previousMessage = (from n in notificationHistory
                                   where n.NotificationDetails.Name == message.NotificationDetails.Name &&
                                         n.SubjectSuffix == message.SubjectSuffix &&
                                         n.Content == message.Content
                                   orderby n.DateSent
                                   select n).FirstOrDefault();

            if (previousMessage == null) {
                return true;
            }

            var timeBetweenMessages = message.DateSent - previousMessage.DateSent;
            if (timeBetweenMessages.TotalDays >= DaysToWait) {
                return true;
            }
            else {
                previousMessage.NotificationUsed = true;
                return false;
            }
        }

        private void SendNotification(NotificationMessage message) {
            // send the notification email
            var senderMailAddress = new MailAddress(Sender);

            var notificationEmail = new MailMessage() {
                Sender = senderMailAddress,
                From = senderMailAddress,
                Subject = $"{ message.NotificationDetails.EmailSubjectPrefix } { message.SubjectSuffix }".Trim(),
                Body = message.Content,
                IsBodyHtml = message.NotificationDetails.BodyIsHtml,
            };

            notificationEmail.ReplyToList.Add(senderMailAddress);

            foreach (var recipient in message.NotificationDetails.Recipients) {
                notificationEmail.To.Add(new MailAddress(recipient));
            }

            notificationHistory.Add(message);
            mailClient.Send(notificationEmail);
        }

        public void Dispose() {
            mailClient.Dispose();
            SaveNotificationHistory();
        }
    }
}
