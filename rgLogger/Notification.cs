using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rgLogger {
    public class Notification {
        public string Name { get; set; }

        public HashSet<string> Recipients { get; } = new HashSet<string>();

        public string EmailSubjectPrefix { get; set; }

        public bool BodyIsHtml { get; set; }
    }
}
