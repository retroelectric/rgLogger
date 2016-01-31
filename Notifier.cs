using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    /// <summary>
    /// Uses an EmailLogger or CumulativeEmailLogger to send notifications which can be configured to suppress duplicate alerts for a given period of time.
    /// </summary>
    public class Notifier {
        private EmailLogger logWriter;
        private Dictionary<string, List<string>> notifications;


        public Notifier(EmailLogger logger) {
            logWriter = logger;
        }

        public string NotificationStorageFile { get; set; } = "rgnotify.dat";


    }
}
