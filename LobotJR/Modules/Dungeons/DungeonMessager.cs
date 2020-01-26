using LobotJR.Client;
using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LobotJR.Modules.Dungeons
{
    public class DungeonMessager
    {
        public static string DUNGEON_COMPLETE = "doneguid74293847562934";
        public static int MSG_INSTANT = 0;
        public static int MSG_QUEUED = 1;
        public static int MSG_DUNGEON_COMPLETE = 2;

        private DateTime lastMessage;
        private HashSet<string> receivers;
        private IrcClient ircMessenger;
        private static readonly int cooldown = 9000;
        private readonly string myChannel = "";

        public Queue<List<string>> messageQueue;

        public void SendIrcMessage(string message)
        {
            ircMessenger.SendIrcMessage(message);
        }

        public DungeonMessager(ref IrcClient whisperClient, string channel, Party myParty)
        {
            HashSet<string> temp = new HashSet<string>();
            foreach (var member in myParty.members)
            {
                temp.Add(member.name);
            }
            receivers = temp;
            ircMessenger = whisperClient;
            lastMessage = DateTime.Now;
            messageQueue = new Queue<List<string>>();
            myChannel = channel;
        }

        public void RemoveMember(string player)
        {
            foreach (var member in receivers)
            {
                if (member == player)
                {
                    receivers.Remove(player);
                    continue;
                }
            }
        }

        public void SendChatMessage(int msgType, string message, string user)
        {
            List<string> temp = new List<string>
            {
                msgType.ToString(),
                message,
                user
            };
            messageQueue.Enqueue(temp);
        }

        public void SendChatMessage(string message, string user)
        {
            List<string> temp = new List<string>
            {
                MSG_INSTANT.ToString(),
                message,
                user
            };
            messageQueue.Enqueue(temp);
        }

        public void SendChatMessage(string message, Party myParty)
        {
            List<string> temp = new List<string>
            {
                MSG_QUEUED.ToString(),
                message
            };
            foreach (var member in myParty.members)
            {
                temp.Add(member.name);
            }
            messageQueue.Enqueue(temp);
        }

        // if there's no chat cooldown, lobot tries to send a message from the queue and then remove it. otherwise, do nothing
        // to determine dungeon complete, if msg = DUNGEON_COMPLETE, return 1, otherwise return 0
        public int ProcessQueue()
        {
            List<string> temp = messageQueue.Peek();
            int msgType = -1;
            int.TryParse(temp.ElementAt(0), out msgType);
            if (msgType == MSG_DUNGEON_COMPLETE)
                return 1;

            if (msgType == MSG_INSTANT || ((DateTime.Now - lastMessage).TotalMilliseconds > cooldown))
            {
                //fix this
                string toSend = "";
                if (temp.Count > 1)
                    toSend = temp.ElementAt(1);
                else
                    toSend = temp.ElementAt(0);

                if (toSend == DUNGEON_COMPLETE)
                    return 1;
                if (!(temp.Count > 1))
                    return 0;

                temp.RemoveRange(0, 2);
                int i = 0;
                int numReceivers = temp.Count();
                foreach (var receiver in temp)
                {
                    string tempMsg = ".w " + receiver + " " + toSend;
                    string msg = ":" + "lobotjr" + "!" + "lobotjr" + "@" + "lobotjr" + "tmi.twitch.tv PRIVMSG #" + myChannel + " :" + tempMsg;

                    try
                    {
                        SendIrcMessage(msg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + ": Error occured: " + e);
                        return 0;
                    }
                    if (numReceivers > i)
                    {
                        Thread.Sleep(600);
                        i++;
                    }
                }
                messageQueue.Dequeue();
                lastMessage = DateTime.Now;
                return 0;
            }
            else
            {
                return 0;
            }
        }
    }
}
