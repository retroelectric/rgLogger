using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    [Serializable]
    class NotificationMessage {
        public Notification NotificationDetails { get; set; }
        public string SubjectSuffix { get; set; }
        public string Content { get; set; }
        public DateTime DateSent { get; set; } = DateTime.Now;
        internal bool NotificationUsed;
    }
}
