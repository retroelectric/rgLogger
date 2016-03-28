using System;
using System.Fakes;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

/* Things to check:
 * 1. [DONE] Sends correctly configured emails for all messages the first time they are written
 * 2. Suppresses repeated notifications within the time out period
 * 3. Stops suppressing notifications after the time out period
 * 4. Correctly handles (DaysToWait = -1)
 * 5. Sends messages to the correct notification recipients
 * 6. What does it do when sending to a notification type that doesn't exist?
 * 7. Email subject is set correctly
 * 8. Correctly handles multiple notification types
 * 9. Saves a correctly formatted data file with suppressed notifications
 * 10. suppression is based on the type of notification. duplicate messages can be sent using multiple notifications configured identically but with different names.
 * 11. altering the configuration of a notification will not cause messages to be resent. matches are checked on Notification Name, Subject Suffix, and Message content.
 *      That means Recipients, Sender, Subject Prefix, ReplyTo can all be altered without causing notifications to be resent.
 */

namespace rgLogger.Tests {
    [TestClass]
    public class NotifierTest {
        [TestMethod]
        public void SendsAllNotificationsTheFirstTime() {
            string notificationName = "notification1";
            string notificationSubjectPrefix = "notification one:";
            string emailSender = "fakes@test.com";
            string emailRecipient = "notice@test.com";
            string dataFilename = "testing.dat";

            if (System.IO.File.Exists(dataFilename)) {
                System.IO.File.Delete(dataFilename);
            }

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
            };

            var expectedResults = new List<MailMessage>();
            foreach(var m in messagesToSend) {
                var r = new MailMessage() {
                    Sender = new MailAddress(emailSender),
                    From = new MailAddress(emailSender),
                    Subject = $"{ notificationSubjectPrefix } { m.Substring(0, m.IndexOf(" ")) }",
                    Body = m,
                    IsBodyHtml = false
                };

                r.ReplyToList.Add(new MailAddress(emailSender));
                r.To.Add(new MailAddress(emailRecipient));

                expectedResults.Add(r);
            }

            using (ShimsContext.Create()) {
                var notificationMails = new List<MailMessage>();

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = 7;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    foreach(var m in messagesToSend) {
                        n.SendNotification(notificationName, m, m.Substring(0, m.IndexOf(" ")));
                    }
                }

                CollectionAssert.AreEqual(expectedResults, notificationMails, new Comparers.MailMessageComparer(), "Notification emails sent do not match the expected result.");
            }
        }
        /*
        [TestMethod]
        public void SendsAllEmailNotificationsOnTheInitialWrite() {
            string notificationName = "notice1";
            string notificationSubject = "the quick brown fox jumped over the lazy dog";
            string emailSender = "fakes@test.com";
            string notificationRecipient = "notice@test.com";
            string dataFilename = "testing.dat";

            if (System.IO.File.Exists(dataFilename)) {
                System.IO.File.Delete(dataFilename);
            }

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
                }.OrderBy(m => m).ToList();

                using (var emlog = new EmailLogger("test.com")) {
                    emlog.Sender = new MailAddress(emailSender);
                    emlog.Asynchronous = false;
                    using (var notlog = new Notifier(emlog)) {
                        notlog.NotificationStorageFile = dataFilename;
                        // configure notifications
                        notlog.AddNotification(notificationName, notificationRecipient);

                        foreach (var message in messagesToSend) {
                            notlog.SendNotification(new NotificationMessage(notificationName, notificationSubject, message));
                        }

                        var expectedResult = new List<MailMessage>();

                        foreach (var message in messagesToSend) { //.Select(m => )) {
                            var bodyText = $"{ DeterministicDateTime(1).ToString(emlog.TimestampFormat) }  { message }";
                            var mailmsg = new MailMessage(emailSender, notificationRecipient, notificationSubject, bodyText);
                            mailmsg.ReplyToList.Add(new MailAddress(emailSender));
                            expectedResult.Add(mailmsg);
                        }

                        //.Select(m => new MailMessage(emailSender, notificationRecipient, notificationSubject, m) {
                        //    Sender = new MailAddress(emailSender),
                        //    ReplyTo = new MailAddress(emailSender)
                        //})
                        //.ToList();

                        for (int i = 0; i < expectedResult.Count(); i++) {
                            System.Diagnostics.Debug.WriteLine($"mailobject{ i + 1 }");
                            System.Diagnostics.Debug.WriteLine($"nm from: { notificationMails[i].From.Address }");
                            System.Diagnostics.Debug.WriteLine($"er from: { expectedResult[i].From.Address }");

                            System.Diagnostics.Debug.WriteLine($"nm to: { notificationMails[i].To[0].Address }");
                            System.Diagnostics.Debug.WriteLine($"er to: { expectedResult[i].To[0].Address }");

                            System.Diagnostics.Debug.WriteLine($"nm subject: { notificationMails[i].Subject }");
                            System.Diagnostics.Debug.WriteLine($"er subject: { expectedResult[i].Subject}");

                            System.Diagnostics.Debug.WriteLine($"nm body: { notificationMails[i].Body }");
                            System.Diagnostics.Debug.WriteLine($"er body: { expectedResult[i].Body }");
                        }

                        CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer());

                        //CollectionAssert.AreEquivalent(notificationMails, expectedResult, "The notifications sent to not match what was expected.");
                    }
                }
            }
        }
        */

        /*
    [TestMethod]
    public void DoesNotSendNotificationsThatWereAlreadySent() {
        string notificationName = "notice1";
        string notificationSubject = "the quick brown fox jumped over the lazy dog";
        string emailSender = "fakes@test.com";
        string notificationRecipient = "notice@test.com";
        string dataFilename = "testing.dat";
        int dayOfNotification = 1;

        if (System.IO.File.Exists(dataFilename)) {
            System.IO.File.Delete(dataFilename);
        }

        var notificationMails = new List<MailMessage>();
        using (ShimsContext.Create()) {
            ShimDateTime.NowGet = () => {
                // make DateTime.Now deterministic based on the number of log messages that have been written.
                return DeterministicDateTime(dayOfNotification);
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
                emlog.Sender = new MailAddress(emailSender);
                emlog.Asynchronous = false;
                using (var notlog = new Notifier(emlog)) {
                    notlog.DaysToWait = 7;
                    notlog.NotificationStorageFile = dataFilename;
                    // configure notifications
                    notlog.AddNotification(notificationName, notificationRecipient);

                    // send the notifications the first time.
                    foreach (var message in messagesToSend) {
                        notlog.SendNotification(new NotificationMessage(notificationName, notificationSubject, message));
                    }

                    var expectedResult = from m in messagesToSend
                                         select new MailMessage(emlog.Sender.Address, notificationRecipient, notificationSubject, m) {
                                             Sender = new MailAddress(emailSender),
                                             ReplyTo = new MailAddress(emailSender)
                                         };
                                         */


        /* this is the test SendsAllEmailNotificationsOnTheInitialWrite()
         * if that fails this test should fail. either the notifications aren't being sent correctly
         * or something has changed and this test needs to be updated */
        /*CollectionAssert.AreEqual(notificationMails, expectedResult.ToList(), "The notifications sent to not match what was expected.");

        // switch the day and reset the notification and expected result collections
        // to prepare a  clean slate for the next round of notifications being sent
        dayOfNotification = 2;
        notificationMails = new List<MailMessage>();
        expectedResult = new List<MailMessage>();

        // send the notifications again
        foreach (var message in messagesToSend) {
            notlog.SendNotification(new NotificationMessage(notificationName, notificationSubject, message));
        }
    }
}
}
}
*/
        private DateTime DeterministicDateTime(int days) {
            return new DateTime(1999, 12, days, 0, 0, 0);
        }
    }
}
