using System;
using System.Fakes;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

/* Things to check:
 * 01. [DONE] Sends correctly configured emails for all messages the first time they are written
 * 02. [DONE] Suppresses repeated notifications within the time out period
 * 03. [DONE] Stops suppressing notifications after the time out period
 * 04. [DONE] DaysToWait == 0 means messages will never be suppressed.
 * 05. [DONE - 01] Sends messages to the correct notification recipients
 * 06. [DONE - 01] What does it do when sending to a notification type that doesn't exist?
 * 07. [DONE - 01] Email subject is set correctly
 * 08. [DONE] Correctly handles multiple notification types
 * 09. Saves a correctly formatted data file with suppressed notifications
 * 10. Suppression is based on the type of notification. duplicate messages can be sent using multiple notifications configured identically but with different names.
 * 11. Altering the configuration of a notification will not cause messages to be resent. matches are checked on Notification Name, Subject Suffix, and Message content.
 *      That means Recipients, Sender, Subject Prefix, ReplyTo can all be altered without causing notifications to be resent.
 * 12. [DONE] Sends messages again once they aren't repeated for a run.
 */

namespace rgLogger.Tests {
    [TestClass]
    public class NotifierTest {
        string notificationName = "notification1";
        string notificationSubjectPrefix = "notification one:";
        string emailSender = "fakes@test.com";
        string emailRecipient = "notice@test.com";
        string dataFilename = "testing.dat";

        List<string> messagesToSend = new List<string>() {
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

        [TestMethod]
        public void SendsAllNotificationsTheFirstTime() {
            DeleteDataFile();

            var expectedResults = new List<MailMessage>();
            foreach (var m in messagesToSend) {
                expectedResults.Add(StandardMailMessage(m, m.Substring(0, m.IndexOf(" "))));
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

                    foreach (var m in messagesToSend) {
                        n.SendNotification(notificationName, m, m.Substring(0, m.IndexOf(" ")));
                    }
                }

                CollectionAssert.AreEqual(expectedResults, notificationMails, new Comparers.MailMessageComparer(), "Notification emails sent do not match the expected result.");
            }
        }

        [TestMethod]
        public void CorrectlySuppressesMessages() {
            int notificationTimeout = 7;
            DeleteDataFile();

            using (ShimsContext.Create()) {
                int CurrentDay = 0;
                List<MailMessage> notificationMails = new List<MailMessage>();
                List<MailMessage> expectedResult;

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                ShimDateTime.NowGet = () => {
                    return new DateTime(2000, 1, CurrentDay, 11, 12, 13);
                };

                /* DAY ONE */
                CurrentDay = 1;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "Day 1.")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "Day 1.");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 1.");

                /* DAY TWO */
                CurrentDay = 2;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[1], "Day 2.")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    // resend the previous messages. they should not be in the actual result.
                    n.SendNotification(notificationName, messagesToSend[0], "Day 1.");
                    // send a new notification that should be in the actual result.
                    n.SendNotification(notificationName, messagesToSend[1], "Day 2.");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 2.");

                /* DAY THREE */
                CurrentDay = 3;
                notificationMails = new List<MailMessage>();

                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[2], "Day 3.")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    // resend the previous messages. they should not be in the actual result.
                    n.SendNotification(notificationName, messagesToSend[0], "Day 1.");
                    n.SendNotification(notificationName, messagesToSend[1], "Day 2.");
                    // send a new notification that should be in the actual result.
                    n.SendNotification(notificationName, messagesToSend[2], "Day 3.");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 3.");

                /* DAY FOUR - no new messages */
                notificationMails = new List<MailMessage>();
                CurrentDay = 4;
                expectedResult = new List<MailMessage>() { };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    // resend the previous messages. they should not be in the actual result.
                    n.SendNotification(notificationName, messagesToSend[0], "Day 1.");
                    n.SendNotification(notificationName, messagesToSend[1], "Day 2.");
                    n.SendNotification(notificationName, messagesToSend[2], "Day 3.");
                    // send a new notification that should be in the actual result.
                    // *** there are no new messages sent in this test ***
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 4.");
            }
        }

        [TestMethod]
        public void ResendsNotificationsAfterTimeoutExpires() {
            int notificationTimeout = 3;
            DeleteDataFile();

            using (ShimsContext.Create()) {
                int CurrentDay = 0;
                List<MailMessage> notificationMails = new List<MailMessage>();
                List<MailMessage> expectedResult;

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                ShimDateTime.NowGet = () => {
                    return new DateTime(2000, 1, CurrentDay, 11, 12, 13);
                };

                /* DAY ONE */
                CurrentDay = 1;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "jane says")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "jane says");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 1.");

                /* DAY TWO */
                CurrentDay = 2;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>();

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "jane says");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 2.");

                /* DAY THREE */
                CurrentDay = 3;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>();

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    // resend the previous messages. they should not be in the actual result.
                    n.SendNotification(notificationName, messagesToSend[0], "jane says");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 3.");

                /* DAY FOUR - resend initial message */
                CurrentDay = 4;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "jane says")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "jane says");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 4.");
            }
        }

        [TestMethod]
        public void SendsNotificationAfterMessageIsNotRepeated() {
            int notificationTimeout = 7;
            DeleteDataFile();

            using (ShimsContext.Create()) {
                int CurrentDay = 0;
                List<MailMessage> notificationMails = new List<MailMessage>();
                List<MailMessage> expectedResult;

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                ShimDateTime.NowGet = () => {
                    return new DateTime(2000, 1, CurrentDay, 11, 12, 13);
                };

                /* DAY ONE */
                CurrentDay = 1;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "velvet revolver")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "velvet revolver");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 1.");

                /* DAY TWO */
                CurrentDay = 2;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>();

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 2.");

                /* DAY THREE */
                CurrentDay = 3;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "velvet revolver")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "velvet revolver");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 3.");
            }
        }

        [TestMethod]
        public void MessagesAreNeverSuppressedWhenDaysToWaitIsZero() {
            int notificationTimeout = 0;
            DeleteDataFile();

            using (ShimsContext.Create()) {
                int CurrentDay = 0;
                List<MailMessage> notificationMails = new List<MailMessage>();
                List<MailMessage> expectedResult;

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                ShimDateTime.NowGet = () => {
                    return new DateTime(2000, 1, CurrentDay, 11, 12, 13);
                };

                /* DAY ONE */
                CurrentDay = 1;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "fridge lazer"),
                    StandardMailMessage(messagesToSend[1], "pew pew pew"),
                    StandardMailMessage(messagesToSend[2], "rome antium cumae"),
                    StandardMailMessage(messagesToSend[3], "black mirror")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "fridge lazer");
                    n.SendNotification(notificationName, messagesToSend[1], "pew pew pew");
                    n.SendNotification(notificationName, messagesToSend[2], "rome antium cumae");
                    n.SendNotification(notificationName, messagesToSend[3], "black mirror");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 1.");

                /* DAY TWO */
                CurrentDay = 2;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "fridge lazer"),
                    StandardMailMessage(messagesToSend[1], "pew pew pew"),
                    StandardMailMessage(messagesToSend[2], "rome antium cumae"),
                    StandardMailMessage(messagesToSend[3], "black mirror"),
                    StandardMailMessage(messagesToSend[4], "cool ridge"),
                    StandardMailMessage(messagesToSend[5], "cables to go"),
                    StandardMailMessage(messagesToSend[6], "elsa anna olaf"),
                    StandardMailMessage(messagesToSend[7], "product guide")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "fridge lazer");
                    n.SendNotification(notificationName, messagesToSend[1], "pew pew pew");
                    n.SendNotification(notificationName, messagesToSend[2], "rome antium cumae");
                    n.SendNotification(notificationName, messagesToSend[3], "black mirror");
                    n.SendNotification(notificationName, messagesToSend[4], "cool ridge");
                    n.SendNotification(notificationName, messagesToSend[5], "cables to go");
                    n.SendNotification(notificationName, messagesToSend[6], "elsa anna olaf");
                    n.SendNotification(notificationName, messagesToSend[7], "product guide");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 2.");

                /* DAY THREE */
                CurrentDay = 3;
                notificationMails = new List<MailMessage>();
                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "fridge lazer"),
                    StandardMailMessage(messagesToSend[4], "cool ridge"),
                    StandardMailMessage(messagesToSend[5], "cables to go"),
                    StandardMailMessage(messagesToSend[6], "elsa anna olaf"),
                    StandardMailMessage(messagesToSend[7], "product guide")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = notificationTimeout;

                    n.AddNotification(notificationName, notificationSubjectPrefix, emailRecipient);

                    n.SendNotification(notificationName, messagesToSend[0], "fridge lazer");
                    n.SendNotification(notificationName, messagesToSend[4], "cool ridge");
                    n.SendNotification(notificationName, messagesToSend[5], "cables to go");
                    n.SendNotification(notificationName, messagesToSend[6], "elsa anna olaf");
                    n.SendNotification(notificationName, messagesToSend[7], "product guide");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result for Day 3.");
            }
        }

        [TestMethod]
        public void CorrectlyHandlesMultipleNotificationTypes() {
            DeleteDataFile();

            using (ShimsContext.Create()) {
                int currentMinute = 0;
                List<MailMessage> notificationMails = new List<MailMessage>();
                List<MailMessage> expectedResult;

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                ShimDateTime.NowGet = () => {
                    return new DateTime(2000, 1, 1, 12, currentMinute++, 13);
                };

                expectedResult = new List<MailMessage>() {
                    StandardMailMessage(messagesToSend[0], "n1m1", "notice1@test.com", "notification type 1"),
                    StandardMailMessage(messagesToSend[1], "n2m1", "notice2@test.com", "notification type 2"),
                    StandardMailMessage(messagesToSend[2], "n3m1", "notice3@test.com", "notification type 3"),
                    StandardMailMessage(messagesToSend[3], "n1m2", "notice1@test.com", "notification type 1"),
                    StandardMailMessage(messagesToSend[4], "n2m2", "notice2@test.com", "notification type 2")
                };

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;
                    n.DaysToWait = 7;

                    n.AddNotification("notification1", "notification type 1", "notice1@test.com");
                    n.AddNotification("notification2", "notification type 2", "notice2@test.com");
                    n.AddNotification("notification3", "notification type 3", "notice3@test.com");

                    n.SendNotification("notification1", messagesToSend[0], "n1m1");
                    n.SendNotification("notification2", messagesToSend[1], "n2m1");
                    n.SendNotification("notification3", messagesToSend[2], "n3m1");
                    n.SendNotification("notification1", messagesToSend[3], "n1m2");
                    n.SendNotification("notification2", messagesToSend[4], "n2m2");
                }

                CollectionAssert.AreEqual(expectedResult, notificationMails, new Comparers.MailMessageComparer(), "Notifications sent do not match the expected result.");
            }
        }

        [TestMethod]
        public void CorrectlyHandlesNotificationsWithMultipleRecipients() {
            DeleteDataFile();

            var expectedResults = new List<MailMessage>();
            foreach (var m in messagesToSend) {
                expectedResults.Add(StandardMailMessage(m, m.Substring(0, m.IndexOf(" "))));
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

                    n.AddNotification(notificationName, notificationSubjectPrefix, new string[] { emailRecipient });

                    foreach (var m in messagesToSend) {
                        n.SendNotification(notificationName, m, m.Substring(0, m.IndexOf(" ")));
                    }
                }

                CollectionAssert.AreEqual(expectedResults, notificationMails, new Comparers.MailMessageComparer(), "Notification emails sent do not match the expected result.");
            }
        }
        
        [TestMethod]
        public void NotificationsAreSentWithCorrectlyConfiguredSubjects() {
            DeleteDataFile();

            var notificationMails = new List<MailMessage>();

            using (ShimsContext.Create()) {
                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        notificationMails.Add(e);
                    };
                };

                var expectedResult = messagesToSend.OrderBy(m => m).Select(m => $"this is a prefix { m }").ToList();

                using (var n = new Notifier("mail.test.com")) {
                    n.Sender = emailSender;
                    n.NotificationHistoryFile = dataFilename;

                    n.AddNotification(notificationName, "this is a prefix", emailRecipient);

                    foreach (var m in messagesToSend.OrderBy(m => m)) {
                        n.SendNotification(notificationName, m, m);
                    }

                    CollectionAssert.AreEqual(expectedResult, notificationMails.Select(m => m.Subject).ToList());
                }
            }
        }

        private MailMessage StandardMailMessage(string content, string subjectSuffix, string recipient, string subjectPrefix) {
            var r = new MailMessage() {
                Sender = new MailAddress(emailSender),
                From = new MailAddress(emailSender),
                Subject = $"{ subjectPrefix } { subjectSuffix }",
                Body = content,
                IsBodyHtml = false
            };

            r.ReplyToList.Add(new MailAddress(emailSender));
            r.To.Add(new MailAddress(recipient));

            return r;
        }

        private MailMessage StandardMailMessage(string content, string subjectSuffix) {
            return StandardMailMessage(content, subjectSuffix, emailRecipient, notificationSubjectPrefix);
        }

        private void DeleteDataFile() {
            if (System.IO.File.Exists(dataFilename)) {
                System.IO.File.Delete(dataFilename);
            }
        }
        private DateTime DeterministicDateTime(int days) {
            return new DateTime(1999, 12, days, 0, 0, 0);
        }
    }
}
