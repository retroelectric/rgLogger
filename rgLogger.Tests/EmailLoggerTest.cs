﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

/* Things to check:
 * 1. [DONE] An email is sent for each call to Write().
 * 2. Messages are not sent at the wrong logging level (test log level filtering)
 * 3. Proxy is configured correctly by the EmailLogger class.
 * 4. Async is used when the property is set.
 * 5. Adding recipients works correctly.
 * 6. ReplyTo uses the Sender address when not set.
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

                using (var logWriter = new EmailLogger("test.server", LogLevel.All) {
                    Sender = new MailAddress("fakes@test.server"),
                    Asynchronous = false
                }) {
                    logWriter.AddRecipient("recipient@test.server");

                    logWriter.Write("the quick brown fox jumped over the lazy dog.", LogLevel.All);
                    logWriter.Write("Who are you people? Torchwood.", LogLevel.Warn);

                    var expectedResult = new List<string>() {
                        // log line for logLevel.All output
                        $"{ new DateTime(1999, 12, 31, 23, 0, 13).ToString(logWriter.TimestampFormat) }  the quick brown fox jumped over the lazy dog.",
                        $"{ new DateTime(1999, 12, 31, 23, 1, 13).ToString(logWriter.TimestampFormat) } [WARN] Who are you people? Torchwood.",
                    };

                    CollectionAssert.AreEqual(logMessages, expectedResult);
                }
            }
        }

        [TestMethod]
        public void ReplyToUsesSenderAddressWhenNotSet() {
            using (var logWriter = new EmailLogger("test.server", LogLevel.Warn)) {
                logWriter.Sender = new MailAddress("sender@fakes.test");

                Assert.AreEqual("sender@fakes.test", logWriter.ReplyTo.Address);
            }
        }
    }
}
