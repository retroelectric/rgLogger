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
    class EmailLoggerTest {
        [TestMethod]
        public void SendsEmailMessagesForAllLogEntries() {
            var logMessages = new List<string>();

            using (ShimsContext.Create()) {
                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        logMessages.Add(e.Body);
                    };
                };

                var logWriter = new EmailLogger("test.server", LogLevel.All);
                logWriter.Write("the quick brown fox jumped over the lazy dog.", LogLevel.All);
                logWriter.Write("Who are you people? Torchwood.");

                Assert.AreEqual(logMessages, new List<string>() { "the quick brown fox jumped over the lazy dog.", "Who are you people? Torchwood." });
            }
        }
    }
}
