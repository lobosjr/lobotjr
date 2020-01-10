using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using Wolfcoins;
using Adventures;

namespace TwitchBot
{
    class IrcClient
    {
        public DateTime timeLast;
        public DateTime dungeonTimeLast;
        public bool connected;
        public Queue<string> messageQueue = new Queue<string>();
        public Queue<Dictionary<Party,string>> dungeonQueue = new Queue<Dictionary<Party,string>>();
        //private int dungeonCooldown = 7500;
        private int cooldown = 65;
        private string username;
        private string channel;
        private string myIp;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string username, string password)
        {
            timeLast = DateTime.Now;
            dungeonTimeLast = DateTime.Now;
            this.username = username;
            this.myIp = ip;
            tcpClient = new TcpClient(ip, port);
            this.connected = tcpClient.Connected;

            if (connected)
            {
                Console.WriteLine("Successfully connected to IRC server: " + ip);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                outputStream.WriteLine("PASS oauth:" + password);
                outputStream.WriteLine("NICK " + username);
                outputStream.WriteLine("USER " + username + " 8 * :" + username);
                outputStream.Flush();
            }
            else
            {
                Console.WriteLine("Unable to connect to IRC server: " + ip);

            }

        }

        static void Whisper(string user, string message, IrcClient whisperClient)
        {
            string toSend = ".w " + user + " " + message;
            whisperClient.sendChatMessage(toSend);
        }

        static void Whisper(Party party, string message, IrcClient whisperClient)
        {
            for (int i = 0; i < party.NumMembers(); i++)
            {
                string toSend = ".w " + party.members.ElementAt(i).name + " " + message;
                whisperClient.sendChatMessage(toSend);
            }
        }

        public void joinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
            Console.WriteLine("Joined channel: " + channel);
        }

        public void sendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void sendChatMessage(string message)
        {
            messageQueue.Enqueue(message);
        }

        // if there's no chat cooldown, lobot tries to send a message from the queue and then remove it. otherwise, do nothing
        public void processQueue()
        {
            if ((DateTime.Now - timeLast).TotalMilliseconds > cooldown)
            {
                string temp = messageQueue.Dequeue();
                string msg = ":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + temp;
                
                try
                    {
                        sendIrcMessage(msg);
                    }
                    catch(Exception e)
                    {
                        messageQueue.Enqueue(temp);
                        Console.WriteLine("Error occured: " + e);
                    }
                timeLast = DateTime.Now;
            }
            
        }

        public string[] readMessage(Currency userList, string channel)
        {
            //if (tcpClient.Connected != true)
            //{
            //    string[] buf = { };
            //    return buf;
            //}
            try
            {
                if (tcpClient.GetStream().DataAvailable)
                {
                    string message = inputStream.ReadLine();
                    string[] temp = parseMessage(message);
                    if (temp[0] != null)
                    {
                        Match match = Regex.Match(temp[1], @"([A-Za-z0-9])\.([A-Za-z])([A-Za-z0-9])", RegexOptions.IgnoreCase);
                        string check = temp[1].ToLower();
                        if (match.Success)
                        {
                            if (check.Contains("d.va") || userList.subSet.Contains(temp[0]))
                            {
                                int i = 0;
                            }
                            else if( check.Contains("OCEAN MAN 🌊  😍"))
                            {
                                string timeout = "/timeout " + temp[0] + " 1";
                                sendChatMessage(timeout);
                                sendChatMessage("NOCEAN MAN");
                                string[] noMsg = { "" };
                                return noMsg;
                            }
                            else
                            { 
                                userList.UpdateViewers(channel);
                                if (userList.xpList != null)
                                {
                                    if (userList.xpList.ContainsKey(temp[0]) && (userList.determineLevel(temp[0]) < 2))
                                    {
                                        string timeout = "/timeout " + temp[0] + " 1";
                                        sendChatMessage(timeout);
                                        sendChatMessage("Links may only be posted by viewers of Level 2 or above. (Message me '?' for more details)");
                                        string[] noMsg = { "" };
                                        return noMsg;
                                    }
                                    else if (!userList.xpList.ContainsKey(temp[0]))
                                    {
                                        string timeout = "/timeout " + temp[0] + " 1";
                                        sendChatMessage(timeout);
                                        sendChatMessage("Links may only be posted by viewers of Level 2 or above. (Message me '?' for more details)");
                                        string[] noMsg = { "" };
                                        return noMsg;
                                    }
                                }
                            }
                        }
                        message = temp[0] + " says: " + temp[1];
                    }

                    return temp;
                }
                else
                {
                    string[] temp = { "" };
                    return temp;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bot disconnected. Reason: " + ex);
                string[] buf = { "Bot disconnected. Reason: " + ex };
                return buf;
                // Todo: Open a new connection, or otherwise gracefully close bot.
            }


        }

        public string[] readMessage()
        {
            if (tcpClient.GetStream().DataAvailable)
            {
                string message = inputStream.ReadLine();
                string[] temp = parseMessage(message);
                if (temp[0] != null)
                {
                    message = temp[0] + " says: " + temp[1];
                }

                //Console.WriteLine(message);

                return temp;
            }
            else
            {
                string[] temp = { "" };
                return temp;

            }

        }

        public string[] parseMessage(string message)
        {
            if (message.StartsWith("PING"))
            {
                //PONG
                sendIrcMessage("PONG tmi.twitch.tv\r\n");
            }
            if(message.Contains("•"))
            {
                string[] temp = { "Someone", "I sent a beep noise :(" };
                return temp;
            }
            int count = 0;
            string buf = "";
            int charPos = 0;
            bool serverParsed = false;
            string server = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (count == 2)
                {
                    buf += message[i];
                    charPos++;
                }
                else if(message[i] == ':')
                {
                    count++;
                }
                else if (!serverParsed)
                {
                    if(message[i] == ' ')
                    {
                        serverParsed = true;
                    }
                    else
                    {
                        server += message[i];
                    }
                }
            }
            if (server != "tmi.twitch.tv")
            {
                if (message.Contains("PRIVMSG"))
                {
                    string name = message.Substring(1, message.IndexOf("!") - 1);
                    string[] output = {name, buf};
                    return output;
                }
                else if (message.Contains("WHISPER"))
                {
                    string[] temp = message.Split(new char[]{' '},5);
                    int size = temp.Length;
                    int myColon = temp[size - 1].IndexOf(':');
                    int length = temp[1].IndexOf("!") - 1;
                    string name = temp[1].Substring(1, length);
                    string msg = temp[size - 1].Substring(1);
                    
                    string[] output = { name, msg };
                    return output;
                }
                else
                {
                    string[] temp = { null, message };
                    return temp;
                }
            }
            else
            {
                string[] temp = { null, buf };
                return temp;
            }
        }
    };
}