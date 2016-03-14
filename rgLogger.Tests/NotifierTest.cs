using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/* Things to check:
 * 1. Sends all notifications written to it
 * 2. Suppresses repeated notifications within the time out period
 * 3. Stops suppressing notifications after the time out period
 * 4. Correctly handles (DaysToWait = -1)
 */

namespace rgLogger.Tests {
    [TestClass]
    public class NotifierTest {
        [TestMethod]
        public void SendsAllNotifications() {
        }
    }
}
