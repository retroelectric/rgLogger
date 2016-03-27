using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace rgLogger.Tests.Comparers {
    class MailMessageComparer : Comparer<MailMessage> {
        public override int Compare(MailMessage x, MailMessage y) {
            if (x == null && y == null) { return 0; }
            if (x == null || y == null) { return 1; }

            if (
                !CompareMailAddresses(x.Sender, y.Sender) ||
                !CompareMailAddressCollections(x.To, y.To) ||
                !CompareMailAddressCollections(x.CC, y.CC) ||
                !CompareMailAddressCollections(x.Bcc, y.Bcc) ||
                !CompareMailAddresses(x.From, y.From) ||
                !CompareMailAddressCollections(x.ReplyToList, y.ReplyToList) ||
                !string.Equals(x.Subject, y.Subject) ||
                x.SubjectEncoding != y.SubjectEncoding ||
                !string.Equals(x.Body, y.Body) ||
                x.BodyEncoding != y.BodyEncoding ||
                x.IsBodyHtml != y.IsBodyHtml ||
                x.Priority != y.Priority
            ) {
                // different
                return 1;
            }
            else {
                // the same
                return 0;
            }
        }

        public static bool CompareMailAddressCollections(MailAddressCollection x, MailAddressCollection y) {
            if (x == null && y == null) { return true; }
            if (x == null || y == null) { return false; }

            if (x.Count() != y.Count()) { return false; }

            return x.OrderBy(m => m.Address).Select(m => m.Address)
                .SequenceEqual(y.OrderBy(m => m.Address).Select(m => m.Address));
        }

        public static bool CompareMailAddresses(MailAddress x, MailAddress y) {
            if (x == null && y == null) { return true; }
            if (x == null || y == null) { return false; }
            return string.Equals(x.Address, y.Address);
        }
    }
}
