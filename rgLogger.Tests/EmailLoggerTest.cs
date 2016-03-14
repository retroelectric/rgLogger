using System;
using System.Fakes;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

/* Things to check:
 * 1. [DONE] An email is sent for each call to Write().
 * 2. Messages are not sent at the wrong logging level (test log level filtering)
 * 3. Async is used when the property is set.
 * 4. Adding recipients works correctly.
 * 5. [DONE] ReplyTo uses the Sender address when not set.
 */

using rgLogger;

namespace rgLogger.Tests {
    [TestClass]
    public class EmailLoggerTest {
        [TestMethod]
        public void SendsEmailMessagesForAllLogEntries() {
            var logMessages = new List<string>();

            using (ShimsContext.Create()) {
                ShimDateTime.NowGet = () => {
                    // make DateTime.Now deterministic based on the number of log messages that have been written.
                    return new DateTime(1999, 12, 24, 23, logMessages.Count, 13);
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
                        $"{ new DateTime(1999, 12, 24, 23, 0, 13).ToString(logWriter.TimestampFormat) }  the quick brown fox jumped over the lazy dog.",
                        $"{ new DateTime(1999, 12, 24, 23, 1, 13).ToString(logWriter.TimestampFormat) } [WARN] Who are you people? Torchwood.",
                    };

                    CollectionAssert.AreEqual(logMessages, expectedResult, "Log messages do not match.");
                }
            }
        }

        [TestMethod]
        public void MessagesAreNotSentAtTheWrongLoggingLevel() {
            var logMessages = new List<string>();

            using (ShimsContext.Create()) {
                ShimDateTime.NowGet = () => {
                    // make DateTime.Now deterministic based on the number of log messages that have been written.
                    return new DateTime(1999, 12, 24, 23, logMessages.Count, 13);
                };

                ShimSmtpClient.Constructor = @this => {
                    var shim = new ShimSmtpClient(@this);
                    shim.SendMailMessage = e => {
                        logMessages.Add(e.Body);
                    };
                };

                var messagesToSend = new List<KeyValuePair<LogLevel, string>>() {
                    new KeyValuePair<LogLevel, string>(LogLevel.Debug, "this is a debug message"),
                    new KeyValuePair<LogLevel, string>(LogLevel.Fatal, "this is a fatal message"),
                    new KeyValuePair<LogLevel, string>(LogLevel.Info, "this is an info message"),
                    new KeyValuePair<LogLevel, string>(LogLevel.None, "this is a none message"),
                    new KeyValuePair<LogLevel, string>(LogLevel.Warn, "this is a warn message"),
                    new KeyValuePair<LogLevel, string>(LogLevel.All, "this is an all message")
                };

                foreach (LogLevel lvl in Enum.GetValues(typeof(LogLevel))) {
                    // reset log messages
                    logMessages = new List<string>();

                    using (var logWriter = new EmailLogger("test.server", LogLevel.All) {
                        Sender = new MailAddress("fakes@test.server"),
                        Asynchronous = false,
                        Level = lvl
                    }) {
                        // write log messages
                        foreach(var m in messagesToSend.OrderBy(x => x.Key)) {
                            logWriter.Write(m.Value, m.Key);
                        }

                        // check log messages
                        var expectedResults = new List<string>();
                        int i = 0;
                        foreach(var m in messagesToSend.OrderBy(x => x.Key)) {
                            if (lvl != LogLevel.None && m.Key != LogLevel.None) {
                                if (lvl == LogLevel.All || m.Key <= lvl) {
                                    expectedResults.Add($"{ new DateTime(1999, 12, 24, 23, i, 13).ToString(logWriter.TimestampFormat) } { ((m.Key == LogLevel.All) ? string.Empty : $"[{ m.Key.ToString().ToUpper() }]") } { m.Value }");
                                    i++;
                                }
                            }
                        }

                        CollectionAssert.AreEqual(logMessages, expectedResults, $"Failed for log level { lvl.ToString().ToUpper() }.");
                    }
                }
            }
        }

        [TestMethod]
        public void ReplyToUsesSenderAddressWhenNotSet() {
            using (var logWriter = new EmailLogger("test.server", LogLevel.Warn)) {
                logWriter.Sender = new MailAddress("sender@fakes.test");

                Assert.AreEqual(logWriter.Sender.Address, logWriter.ReplyTo.Address, "ReplyTo does not equal Sender.");
            }
        }
    }
}