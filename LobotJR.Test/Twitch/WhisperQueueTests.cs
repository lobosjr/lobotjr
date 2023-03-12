using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Test.Mocks;
using LobotJR.Twitch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LobotJR.Test.Twitch
{
    [TestClass]
    public class WhisperQueueTests
    {
        private IRepositoryManager RepositoryManager;

        [TestInitialize]
        public void Initialize()
        {
            RepositoryManager = new SqliteRepositoryManager(MockContext.Create());
        }

        [TestMethod]
        public void UpdateUserIdsLooksUpMessagesWithNullIds()
        {
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Foo", null, "test", DateTime.Now);
            queue.UpdateUserIds(new UserLookup(RepositoryManager));
            var canSend = queue.TryGetMessage(out var toSend);
            Assert.IsTrue(canSend);
            Assert.IsTrue(toSend.Message.Equals("test"));
            Assert.IsFalse(string.IsNullOrWhiteSpace(toSend.UserId));
        }

        [TestMethod]
        public void UpdateUserIdsRemovesMessagesWhereTwitchIdNotFound()
        {
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Invalid", null, "test", DateTime.Now);
            queue.UpdateUserIds(new UserLookup(RepositoryManager));
            var canSend = queue.TryGetMessage(out var toSend);
            Assert.IsFalse(canSend);
            Assert.IsNull(toSend);
        }

        [TestMethod]
        public void EnqueueAddsWhispersToQueue()
        {
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            var canSend = queue.TryGetMessage(out var toSend);
            Assert.IsTrue(canSend);
            Assert.IsTrue(toSend.Message.Equals("test"));
        }

        [TestMethod]
        public void GetMessagesRemovesWhispersFromQueue()
        {
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            queue.TryGetMessage(out var toSend);
            var canSend = queue.TryGetMessage(out toSend);
            Assert.IsFalse(canSend);
            Assert.IsNull(toSend);
        }

        [TestMethod]
        public void GetMessagesRespectsPerSecondLimit()
        {
            var queue = new WhisperQueue(RepositoryManager, 1, 10, 10);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            queue.Enqueue("Test", "0", "fail", DateTime.Now + TimeSpan.FromMilliseconds(1));
            var canSendFirst = queue.TryGetMessage(out var toSendFirst);
            queue.ReportSuccess(toSendFirst);
            var canSendSecond = queue.TryGetMessage(out var toSendSecond);
            Assert.IsTrue(canSendFirst);
            Assert.IsTrue(toSendFirst.Message.Equals("test"));
            Assert.IsFalse(canSendSecond);
            Assert.IsNull(toSendSecond);
        }

        [TestMethod]
        public void GetMessagesRespectsPerMinuteLimit()
        {
            var queue = new WhisperQueue(RepositoryManager, 10, 1, 10);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            queue.Enqueue("Test", "0", "fail", DateTime.Now + TimeSpan.FromMilliseconds(1));
            var canSendFirst = queue.TryGetMessage(out var toSendFirst);
            queue.ReportSuccess(toSendFirst);
            var canSendSecond = queue.TryGetMessage(out var toSendSecond);
            Assert.IsTrue(canSendFirst);
            Assert.IsTrue(toSendFirst.Message.Equals("test"));
            Assert.IsFalse(canSendSecond);
            Assert.IsNull(toSendSecond);
        }

        [TestMethod]
        public void GetMessagesRespectsMaxRecipientLimit()
        {
            var queue = new WhisperQueue(RepositoryManager, 10, 10, 1);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            queue.Enqueue("Second", "1", "fail", DateTime.Now + TimeSpan.FromMilliseconds(1));
            var canSendFirst = queue.TryGetMessage(out var toSendFirst);
            queue.ReportSuccess(toSendFirst);
            var canSendSecond = queue.TryGetMessage(out var toSendSecond);
            Assert.IsTrue(canSendFirst);
            Assert.IsTrue(toSendFirst.Message.Equals("test"));
            Assert.IsFalse(canSendSecond);
            Assert.IsNull(toSendSecond);
        }

        [TestMethod]
        public void GetMessagesExcludesMessagesWithNoUserId()
        {
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Test", null, "test", DateTime.Now);
            var canSend = queue.TryGetMessage(out var toSend);
            Assert.IsFalse(canSend);
            Assert.IsNull(toSend);
        }

        [TestMethod]
        public void ReportSuccessAddsWhisperTimer()
        {
            var timer = RepositoryManager.DataTimers.Read(x => x.Name.Equals("WhisperQueue")).First();
            RepositoryManager.DataTimers.Delete(timer);
            RepositoryManager.DataTimers.Commit();
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            var canSend = queue.TryGetMessage(out var toSend);
            queue.ReportSuccess(toSend);
            timer = RepositoryManager.DataTimers.Read(x => x.Name.Equals("WhisperQueue")).First();
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.Timestamp <= DateTime.Now);
        }

        [TestMethod]
        public void ReportSuccessUpdatesWhisperTimer()
        {
            var timer = RepositoryManager.DataTimers.Read(x => x.Name.Equals("WhisperQueue")).First();
            timer.Timestamp = DateTime.Now - TimeSpan.FromDays(2);
            RepositoryManager.DataTimers.Update(timer);
            RepositoryManager.DataTimers.Commit();
            var queue = new WhisperQueue(RepositoryManager, 1, 1, 1);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            var canSend = queue.TryGetMessage(out var toSend);
            queue.ReportSuccess(toSend);
            timer = RepositoryManager.DataTimers.Read(x => x.Name.Equals("WhisperQueue")).First();
            Assert.IsNotNull(timer);
            Assert.IsTrue(DateTime.Now - timer.Timestamp < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void ReportSuccessClearsWhisperRecipients()
        {
            var queue = new WhisperQueue(RepositoryManager, 10, 10, 2);
            queue.Enqueue("Test", "0", "test", DateTime.Now);
            var canSend = queue.TryGetMessage(out var toSend);
            queue.ReportSuccess(toSend);
            queue.Enqueue("Second", "1", "test", DateTime.Now);
            canSend = queue.TryGetMessage(out toSend);
            var timer = RepositoryManager.DataTimers.Read(x => x.Name.Equals("WhisperQueue")).First();
            timer.Timestamp = DateTime.Now - TimeSpan.FromDays(2);
            RepositoryManager.DataTimers.Update(timer);
            RepositoryManager.DataTimers.Commit();
            queue.ReportSuccess(toSend);
            queue.Enqueue("Third", "2", "test", DateTime.Now);
            canSend = queue.TryGetMessage(out toSend);
            Assert.IsTrue(canSend);
            Assert.IsTrue(toSend.Username.Equals("Third"));
        }

        [TestMethod]
        public void FreezeQueuePreventsNewMessages()
        {
            var queue = new WhisperQueue(RepositoryManager, 100, 100, 100);
            queue.FreezeQueue();
            queue.Enqueue("Test", null, "test", DateTime.Now);
            var canSend = queue.TryGetMessage(out var toSend);
            Assert.IsFalse(canSend);
            Assert.IsNull(toSend);
        }
    }
}
