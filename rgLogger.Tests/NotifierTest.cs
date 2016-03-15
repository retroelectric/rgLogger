using System;
using System.Fakes;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

/* Things to check:
 * 1. Sends correctly configured emails for all messages written
 * 2. Suppresses repeated notifications within the time out period
 * 3. Stops suppressing notifications after the time out period
 * 4. Correctly handles (DaysToWait = -1)
 * 5. Sends messages to the correct notification recipients
 * 6. What does it do when sending to a notification type that doesn't exist?
 * 7. Email subject is set correctly
 * 8. Correctly handles multiple notification types
 * 9. Saves a correctly formatted data file with suppressed notifications
 */

namespace rgLogger.Tests {
    [TestClass]
    public class NotifierTest {
        [TestMethod]
        public void SendsAllEmailMessages() {

        }


        [TestMethod]
        public void SendsAllNotifications() {
            var notificationMails = new List<MailMessage>();
            using (ShimsContext.Create()) {
                ShimDateTime.NowGet = () => {
                    // make DateTime.Now deterministic based on the number of log messages that have been written.
                    return DeterministicDateTime(1);
                };

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                var messagesToSend = new List<string>() {
                    "native son in my city talk that slang",
                    "virtual insight tranquilo hold on",
                    "some breaks afternoon soul late night jazz",
                    "just jammin' swucca chust muy tranquilo",
                    "chilaxin' by the sea guitar madness",
                    "muy tranquilo no way out afternoon soul",
                    "late night jazz victory some breaks",
                    "sitar chop pizzi chop the anthem",
                    "talk that slang hold on obviously",
                    "indigo child just jammin' faraway"
                }.OrderBy(m => m);

                using (var emlog = new EmailLogger("test.com")) {
                    emlog.Sender = new MailAddress("fakes@test.com");
                    emlog.Asynchronous = false;
                    using (var notlog = new Notifier(emlog)) {
                        // configure notifications
                        notlog.AddNotification("notice1", "notice1@test.com");

                        foreach(var message in messagesToSend) {
                            notlog.SendNotification(new NotificationMessage("notice1", "notification 1", message));
                        }

                        var expectedResult = from m in messagesToSend
                                             select new MailMessage(emlog.Sender.Address, "notice@test.com", "notification 1", m);

                        CollectionAssert.AreEqual(notificationMails, expectedResult.ToList());
                    }
                }
            }
        }

        private DateTime DeterministicDateTime(int days) {
            return new DateTime(1999, 12, days, 0, 0, 0);
        }
    }
}
