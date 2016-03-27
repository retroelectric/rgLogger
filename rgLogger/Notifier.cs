using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace rgLogger {
    class Notifier : IDisposable {
        public string NotificationHistoryFile { get; set; } = "rgnotify.dat";

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
        private List<Notification> notifications;
        private List<NotificationMessage> notificationHistory;

        public Notifier(SmtpClient client) {
            mailClient = client;
        }

        protected Notifier() {
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
                    SubjectSuffix = subjectSuffix
                };

                if (NotificationMessageIsNew(message)) {
                    SendNotification(message);
                }
            }
        }

        private bool NotificationMessageIsNew(NotificationMessage message) {
            if (notificationHistory == null) {
                return true;
            }

            var previousMessage = from n in notificationHistory
                                  where n.NotificationDetails.Name == message.NotificationDetails.Name &&
                                        n.SubjectSuffix == message.SubjectSuffix &&
                                        n.Content == message.Content
                                  select n;

            if (previousMessage.Count() == 0) {
                return true;
            }

            var timeBetweenMessages = message.DateSent - previousMessage.Min(n => n.DateSent);
            if (timeBetweenMessages.TotalDays > DaysToWait) {
                return true;
            }
            else {
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

            mailClient.Send(notificationEmail);
        }

        public void Dispose() {
            mailClient.Dispose();
        }
    }
}
