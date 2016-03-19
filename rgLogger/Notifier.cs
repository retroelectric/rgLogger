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
        private List<NotificationMessage> _sentNotifications;

        /// <summary>
        /// Initializes a new instance of the Notifier class.
        /// </summary>
        /// <param name="logger">The EmailLogger object used to send the notifications.</param>
        /// <param name="daysToWait">Number of days to suppress a notification. Default is to not suppress them at all.</param>
        public Notifier(EmailLogger logger) {
            logWriter = logger;
        }

        /// <summary>
        /// Gets or sets the filename used for storing the notification data.
        /// </summary>
        public string NotificationStorageFile { get; set; } = "rgnotify.dat";
        public int DaysToWait = -1;

        private List<NotificationMessage> SentNotifications {
            get {
                if (_sentNotifications == null) {
                    LoadSentNotifications();
                }
                return _sentNotifications;
            }
        }

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

            var notificationHistory = SentNotifications.Where(n => n.Equals(notification)).SingleOrDefault();

            if (notificationHistory == null) {
                notification.NotificationUsed = true;
                notification.DateSent = DateTime.Now;
                SentNotifications.Add(notification);
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
                f.Serialize(s, SentNotifications.Where(n => n.NotificationUsed).ToList());
            }
        }

        /// <summary>
        /// Retrieves the notifications that were sent during the last run.
        /// </summary>
        /// <param name="DaysToWait">Number of days to suppress a notification.</param>
        public void LoadSentNotifications(bool failWhenCorrupted = false) {
            try {
                using (var notificationFileStream = new System.IO.FileStream(NotificationStorageFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)) {
                    var notificationBinaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    var storedNotifications = (List<NotificationMessage>)notificationBinaryFormatter.Deserialize(notificationFileStream);

                    if (DaysToWait >= 0) {
                        _sentNotifications = storedNotifications.Where(x => x.DateSent.AddDays(DaysToWait) >= DateTime.Now).ToList();
                    }
                    else {
                        _sentNotifications = storedNotifications;
                    }
                }
            }
            catch (System.IO.FileNotFoundException) {
                // no notification file found so the list must be empty
                _sentNotifications = new List<NotificationMessage>();
            }
            catch (System.Runtime.Serialization.SerializationException e) {
                // deserialization failed. 
                if (failWhenCorrupted) {
                    throw e;
                }
                else {
                    // just use an empty list.
                    _sentNotifications = new List<NotificationMessage>();
                }
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
