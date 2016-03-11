using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    /// <summary>
    /// Uses an EmailLogger or CumulativeEmailLogger to send notifications which can be configured to suppress duplicate alerts for a given period of time.
    /// </summary>
    public class Notifier : IDisposable {
        /// <summary>
        /// Email logger used to send the notifications.
        /// </summary>
        private EmailLogger logWriter;

        /// <summary>
        /// Stores the types of notifications that can be sent and their recipients.
        /// </summary>
        private Dictionary<string, List<string>> notifications = new Dictionary<string, List<string>>();

        /// <summary>
        /// Stores the notification that have been sent.
        /// </summary>
        private List<NotificationMessage> sentNotifications;

        /// <summary>
        /// Initializes a new instance of the Notifier class.
        /// </summary>
        /// <param name="logger">The EmailLogger object used to send the notifications.</param>
        /// <param name="daysToWait">Number of days to suppress a notification. Default is to not suppress them at all.</param>
        public Notifier(EmailLogger logger, int daysToWait = -1) {
            logWriter = logger;
            GetSentNotifications(daysToWait);
        }

        /// <summary>
        /// Gets or sets the filename used for storing the notification data.
        /// </summary>
        public string NotificationStorageFile { get; set; } = "rgnotify.dat";

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="notificationName">Name of the notification type.</param>
        /// <param name="recipients">Recipients' email addresses.</param>
        public void AddNotification(string notificationName, List<string> recipients) {
            notifications.Add(notificationName, recipients);
        }

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="notificationName">Name of the notification type.</param>
        /// <param name="recipient">Recipient's email address.</param>
        public void AddNotification(string notificationName, string recipient) {
            AddNotification(notificationName, new List<string>() { recipient });
        }

        /// <summary>
        /// Send a notification message
        /// </summary>
        /// <param name="notification">A NotificationMessage object that defines the message to be sent.</param>
        public void SendNotification(NotificationMessage notification) {
            if (!notifications.ContainsKey(notification.Name)) {
                return;
            }

            var notificationHistory = sentNotifications.Where(n => n.Equals(notification)).SingleOrDefault();

            if (notificationHistory == null) {
                notification.NotificationUsed = true;
                notification.DateSent = DateTime.Now;
                sentNotifications.Add(notification);
                SendEmail(notification);
            }
            else {
                notificationHistory.NotificationUsed = true;
            }
        }

        /// <summary>
        /// Close the email notification object and serialize the sent notifications to disk.
        /// </summary>
        public void Dispose() {
            StoreSentNotifications();
            logWriter.Dispose();
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
        /// <param name="daysToWait">Number of days to suppress a notification.</param>
        private void GetSentNotifications(int daysToWait) {
            try {
                using (var notificationFileStream = new System.IO.FileStream(NotificationStorageFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)) {
                    var notificationBinaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    var storedNotifications = (List<NotificationMessage>)notificationBinaryFormatter.Deserialize(notificationFileStream);

                    if (daysToWait >= 0) {
                        sentNotifications = storedNotifications.Where(x => x.DateSent.AddDays(daysToWait) >= DateTime.Now).ToList();
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
        /// <param name="notification">The notification message to send.</param>
        private void SendEmail(NotificationMessage notification) {
            logWriter.AddRecipient(notifications[notification.Name]);
            logWriter.Subject = notification.Subject;
            logWriter.Write(notification.Message, LogLevel.All);
        }
    }
}
