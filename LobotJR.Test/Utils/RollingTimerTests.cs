using LobotJR.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace LobotJR.Test.Utils
{
    [TestClass]
    public class RollingTimerTests
    {
        [TestMethod]
        public void AddsOccurrences()
        {
            var timer = new RollingTimer(TimeSpan.FromMilliseconds(1000), 2);
            timer.AddOccurrence(DateTime.Now);
            Assert.AreEqual(1, timer.AvailableOccurrences());
        }

        [TestMethod]
        public void ReturnsNoAvailableOccurrencesWhenFull()
        {
            var timer = new RollingTimer(TimeSpan.FromMilliseconds(1000), 2);
            timer.AddOccurrence(DateTime.Now);
            timer.AddOccurrence(DateTime.Now);
            Assert.AreEqual(0, timer.AvailableOccurrences());
        }

        [TestMethod]
        public void OccurrencesAreRemovedAfterInterval()
        {
            var timer = new RollingTimer(TimeSpan.FromMilliseconds(100), 2);
            timer.AddOccurrence(DateTime.Now);
            Assert.AreEqual(1, timer.AvailableOccurrences());
            Thread.Sleep(50);
            timer.AddOccurrence(DateTime.Now);
            Assert.AreEqual(0, timer.AvailableOccurrences());
            Thread.Sleep(50);
            Assert.AreEqual(1, timer.AvailableOccurrences());
            Thread.Sleep(50);
            Assert.AreEqual(2, timer.AvailableOccurrences());
        }
    }
}
