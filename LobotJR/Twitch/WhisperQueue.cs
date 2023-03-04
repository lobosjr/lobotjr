using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Twitch
{
    /// <summary>
    /// A record of a whisper to be sent.
    /// </summary>
    public class WhisperRecord
    {
        /// <summary>
        /// The name of the user to send the whisper to.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The Twitch id of the user.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The content of the whisper.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The time the message was queued.
        /// </summary>
        public DateTime QueueTime { get; set; }

        public WhisperRecord(string userName, string userId, string message, DateTime queueTime)
        {
            Username = userName;
            UserId = userId;
            Message = message;
            QueueTime = queueTime;
        }
    }

    /// <summary>
    /// This class handles the whisper queue, ensuring messages can be sent as
    /// quickly as possible while still conforming to the twitch rate limits.
    /// </summary>
    public class WhisperQueue
    {
        private IRepository<AppSettings> AppSettings;
        private TimeSpan UniqueWhisperTimer = TimeSpan.FromDays(1);
        private int MaxRecipients = 40;

        /// <summary>
        /// Whether or not the queue is accepting new recipients.
        /// </summary>
        public bool NewRecipientsAllowed { get; set; } = true;

        /// <summary>
        /// A collection of whisper records waiting to be sent.
        /// </summary>
        public List<WhisperRecord> Queue { get; set; } = new List<WhisperRecord>();
        /// <summary>
        /// The timer that ensures the queue doesn't send messages that exceed
        /// the twitch limit of 3 whispers per second.
        /// </summary>
        public RollingTimer SecondTimer { get; set; } = new RollingTimer(TimeSpan.FromSeconds(1), 3);
        /// <summary>
        /// The timer that ensure the queue doesn't send messages that exceed
        /// the twitch limit of 100 whispers per minute.
        /// </summary>
        public RollingTimer MinuteTimer { get; set; } = new RollingTimer(TimeSpan.FromMinutes(1), 100);
        /// <summary>
        /// The names of every recipient of a whisper sent. This is used to ensure we do not exceed the limit on unique recipents of 40 per day.
        /// </summary>
        public List<string> WhisperRecipients { get; set; }

        public WhisperQueue(IRepositoryManager repositoryManager)
        {
            AppSettings = repositoryManager.AppSettings;
        }

        /// <summary>
        /// Adds a message to the whisper queue.
        /// </summary>
        /// <param name="user">The name of the user to send to.</param>
        /// <param name="userId">The Twitch id of the user to send to.</param>
        /// <param name="message">The content of the message to send.</param>
        /// <param name="dateTime">The time the message was queued.</param>
        public void Enqueue(string user, string userId, string message, DateTime dateTime)
        {
            var allowed = WhisperRecipients.Contains(user);
            if (!allowed && WhisperRecipients.Count < MaxRecipients && NewRecipientsAllowed)
            {
                WhisperRecipients.Add(user);
                allowed = true;
            }
            if (allowed)
            {
                Queue.Add(new WhisperRecord(user, userId, message, dateTime));
            }
        }

        /// <summary>
        /// Gets the messages from the queue that need to be sent, and removes
        /// them from the queue.
        /// </summary>
        /// <param name="cacheUpdated">Whether or not the userid cache has been
        /// updated.</param>
        /// <param name="userLookup">The UserLookup object to use to fetch new
        /// user ids.</param>
        /// <returns>A collection of records that should be sent.</returns>
        public IEnumerable<WhisperRecord> GetMessagesToSend(bool cacheUpdated, UserLookup userLookup)
        {

            var maxToSend = Math.Max(SecondTimer.AvailableOccurrences(), MinuteTimer.AvailableOccurrences());

            if (cacheUpdated)
            {
                Queue.ForEach(x => x.UserId = userLookup.GetId(x.UserId));
            }
            var toSend = Queue.Where(x => !string.IsNullOrWhiteSpace(x.UserId)).OrderByDescending(x => x.QueueTime).Take(maxToSend).ToList();
            Queue = Queue.Except(toSend).ToList();

            return toSend;
        }

        /// <summary>
        /// Reports that a whisper was successfully sent and updates the
        /// various rate limiters.
        /// </summary>
        /// <param name="record">The record that was sent.</param>
        public void ReportSuccess(WhisperRecord record)
        {
            var appSettings = AppSettings.Read().First();
            if (appSettings.WhisperTimerStart == null || DateTime.Now > appSettings.WhisperTimerStart + UniqueWhisperTimer)
            {
                appSettings.WhisperTimerStart = DateTime.Now;
                AppSettings.Update(appSettings);
                AppSettings.Commit();
                WhisperRecipients.Clear();
                NewRecipientsAllowed = true;
            }

            MinuteTimer.AddOccurrence(DateTime.Now);
            SecondTimer.AddOccurrence(DateTime.Now);
            WhisperRecipients.Add(record.Username);
        }

        /// <summary>
        /// Floods the minute timer to freeze the queue for one minute.
        /// </summary>
        public void FreezeQueue()
        {
            var now = DateTime.Now;
            for (var i = 0; i < MinuteTimer.MaxHits; i++)
            {
                MinuteTimer.AddOccurrence(now);
            }
        }

        /// <summary>
        /// Returns debug information about the state of the queue.
        /// </summary>
        /// <returns>An output string containing debug information.</returns>
        public string Debug()
        {
            return $"The current user list contains {WhisperRecipients.Count} entries. The minute timer contains {MinuteTimer.CurrentHitCount()} hits, and the second timer contains {SecondTimer.CurrentHitCount()} hits. There are {Queue.Count} messages in the queue.";
        }
    }
}
