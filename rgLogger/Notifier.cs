using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    /// <summary>
    /// Uses an EmailLogger or CumulativeEmailLogger to send notifications which can be configured to suppress duplicate alerts for a given period of time.
    /// </summary>
    public class Notifier: IDisposable {
        private EmailLogger logWriter;
        private Dictionary<string, List<string>> notifications = new Dictionary<string, List<string>>();
        private List<NotificationMessage> sentNotifications;

        /// <summary>
        /// Initializes a new instance of the Notifier class.
        /// </summary>
        /// <param name="logger"></param>
        public Notifier(EmailLogger logger) {
            logWriter = logger;
        }

        /// <summary>
        /// Gets or sets the filename used for storing the notification data.
        /// </summary>
        public string NotificationStorageFile { get; set; } = "rgnotify.dat";

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="NotificationName">Name of the notification type.</param>
        /// <param name="Recipient">Recipients' email addresses.</param>
        public void AddNotification(string NotificationName, List<string> Recipients) {
            notifications.Add(NotificationName, Recipients);
        }

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="NotificationName">Name of the notification type.</param>
        /// <param name="Recipient">Recipient's email address.</param>
        public void AddNotification(string NotificationName, string Recipient) {
            AddNotification(NotificationName, new List<string>() { Recipient });
        }

        /// <summary>
        /// Send a notification message
        /// </summary>
        /// <param name="Notification">A NotificationMessage object that defines the message to be sent.</param>
        public void SendNotification(NotificationMessage Notification) {
            if (!notifications.ContainsKey(Notification.Name)) { return; }

            var notificationHistory = sentNotifications.Where(n => n.Equals(Notification)).SingleOrDefault();

            if (notificationHistory == null) {
                Notification.NotificationUsed = true;
                Notification.DateSent = DateTime.Now;
                sentNotifications.Add(Notification);
                SendEmail(Notification);
            }
            else {
                notificationHistory.NotificationUsed = true;
            }
        }

        /// <summary>
        /// Stores the notifications that were sent during this run.
        /// </summary>
        private void StoreSentNotifications() {
            using (var s = new System.IO.FileStream(NotificationStorageFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None)) {
                var f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                f.Serialize(s, sentNotifications.Where(n => n.NotificationUsed).ToList());
            }
        }

        /// <summary>
        /// Retrieves the notifications that were sent during the last run.
        /// </summary>
        private void GetSentNotifications(int DaysToWait) {
            try {
                using (var notificationFileStream = new System.IO.FileStream(NotificationStorageFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)) {
                    var notificationBinaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    var storedNotifications = (List<NotificationMessage>)notificationBinaryFormatter.Deserialize(notificationFileStream);

                    if (DaysToWait >= 0) {
                        sentNotifications = storedNotifications.Where(x => x.DateSent.AddDays(DaysToWait) >= DateTime.Now).ToList();
                    }
                    else {
                        sentNotifications = storedNotifications;
                    }
                }
            }
            catch (System.IO.FileNotFoundException) {
                // no notification file found so the list must be empty
                sentNotifications = new List<NotificationMessage>();
            }
            catch (System.Runtime.Serialization.SerializationException) {
                // deserialization failed. just use an empty list.
                sentNotifications = new List<NotificationMessage>();
            }
        }

        /// <summary>
        /// Send the notification email.
        /// </summary>
        /// <param name="Notification">The notification message to send.</param>
        private void SendEmail(NotificationMessage Notification) {
            logWriter.AddRecipient(notifications[Notification.Name]);
            logWriter.Subject = Notification.Subject;
            logWriter.Write(Notification.Message, LogLevel.All);
        }

        /// <summary>
        /// Close the email notification object and serialize the sent notifications to disk.
        /// </summary>
        public void Dispose() {
            StoreSentNotifications();
            logWriter.Dispose();
        }
    }
}
