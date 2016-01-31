using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    /// <summary>
    /// A notification message.
    /// </summary>
    [Serializable]
    public class NotificationMessage : IEquatable<NotificationMessage> {
        /// <summary>
        /// Initializes a new instance of the NotificationMessage class.
        /// </summary>
        /// <param name="name">The name of the notification type</param>
        /// <param name="subject">The subject of the notification message</param>
        /// <param name="message">The message sent in the notification</param>
        public NotificationMessage(string name, string subject, string message) {
            this.Name = name;
            this.Subject = subject;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the notification name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date this notification was sent.
        /// </summary>
        public DateTime DateSent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this notification has been used.
        /// </summary>
        internal bool NotificationUsed { get; set; }

        /// <summary>
        /// Compare two NotificationMessages to determine if they are the same.
        /// </summary>
        /// <param name="other">The other NotificationMessage to compare.</param>
        /// <returns>True if the notification messages are the same.</returns>
        public bool Equals(NotificationMessage other) {
            if (string.Equals(this.Name, other.Name) && string.Equals(this.Subject, other.Subject) && string.Equals(this.Message, other.Message)) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
