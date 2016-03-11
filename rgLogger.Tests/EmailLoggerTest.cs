using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

/* Things to check:
 * 1. An email is sent for each call to Write()
 * 2. Messages are not sent at the wrong logging level (test log level filtering)
 * 3. Proxy is configured correctly by the EmailLogger class
 * 4. Async is used when the property is set
 * 5. Adding recipients works correctly
 */

using rgLogger;

namespace rgLogger.Tests {
    [TestClass]
    public class EmailLoggerTest {
        [TestMethod]
        public void SendsEmailMessagesForAllLogEntries() {
            var logMessages = new List<string>();

            using (ShimsContext.Create()) {
                System.Fakes.ShimDateTime.NowGet = () => {
                    // make DateTime.Now deterministic based on the number of log messages that have been written.
                    return new DateTime(1999, 12, 31, 23, logMessages.Count, 13);
                };

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        logMessages.Add(e.Body);
                    };
                };

                var smptcli = new SmtpClient("smtp.test.com");

                //var logWriter = new EmailLogger("test.server", LogLevel.All) {
                var logWriter = new EmailLogger(smptcli) {
                    Sender = new MailAddress("fake@microsoft.com"),
                    Asynchronous = false
                };
                logWriter.AddRecipient("test@test.com");

                logWriter.Write("the quick brown fox jumped over the lazy dog.", LogLevel.All);
                logWriter.Write("Who are you people? Torchwood.", LogLevel.Warn);

                var expectedResult = new List<string>() {
                    // log line for logLevel.All output
                    $"{ new DateTime(1999, 12, 31, 23, 1, 13).ToString(logWriter.TimestampFormat) } the quick brown fox jumped over the lazy dog.",
                    $"{ new DateTime(1999, 12, 31, 23, 2, 13).ToString(logWriter.TimestampFormat) } [WARN] Who are you people? Torchwood.",
                };
                Assert.AreEqual(logMessages, expectedResult);
            }
        }
    }
}
