using System.Collections.Generic;
using System.Net.Mail;

namespace rgLogger {
    public interface INotifier {
        int DaysToWait { get; set; }
        string NotificationHistoryFile { get; set; }
        string ReplyTo { get; set; }
        string Sender { get; set; }
        SmtpClient EmailSmtpClient { get; set; }

        void AddNotification(Notification notification);
        void AddNotification(string name, string subjectPrefix, string recipientEmail);
        void AddNotification(string name, string subjectPrefix, IEnumerable<string> recipientEmails);
        void Dispose();
        void SendNotification(string notificationName, string content);
        void SendNotification(string notificationName, string content, string subjectSuffix);
    }
}