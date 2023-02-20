using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Wolfcoins;

namespace TwitchBot
{
    public class IrcClient
    {
        public DateTime timeLast;
        public DateTime dungeonTimeLast;
        public DateTime lastReconnect;
        public bool connected;
        public Queue<string> messageQueue = new Queue<string>();
        public Queue<Dictionary<Party, string>> dungeonQueue = new Queue<Dictionary<Party, string>>();
        //private int dungeonCooldown = 7500;
        private readonly int cooldown = 65;
        private readonly string username;
        private string channel;
        private readonly string myIp;
        private readonly int myPort;
        private readonly string myPassword;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string username, string password)
        {
            timeLast = DateTime.Now;
            dungeonTimeLast = DateTime.Now;
            lastReconnect = DateTime.Now;
            this.username = username;
            myIp = ip;
            myPort = port;
            myPassword = password;
            tcpClient = new TcpClient(ip, port);
            connected = tcpClient.Connected;

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
            if (!IsConnected())
            {
                Reconnect();
            }

            if ((DateTime.Now - timeLast).TotalMilliseconds > cooldown)
            {
                string temp = messageQueue.Dequeue();
                string msg = ":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + temp;

                try
                {
                    sendIrcMessage(msg);
                }
                catch (Exception e)
                {
                    messageQueue.Enqueue(temp);
                    //Console.WriteLine("Error occured: " + e);
                    Reconnect();
                }
                timeLast = DateTime.Now;
            }

        }

        public bool IsConnected()
        {
            {
                try
                {
                    if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
                    {
                        /* pear to the documentation on Poll:
                         * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                         * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                         * -or- true if data is available for reading; 
                         * -or- true if the connection has been closed, reset, or terminated; 
                         * otherwise, returns false
                         */

                        // Detect if client disconnected
                        if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Reconnect()
        {
            if (connected)
            {
                connected = false;
                Console.WriteLine("Disconnected from server. Retrying connection...");
            }

            if ((DateTime.Now - lastReconnect).TotalSeconds > 5)
            {
                tcpClient = new TcpClient(myIp, myPort);
                connected = tcpClient.Connected;

                if (connected)
                {
                    connected = true;
                    Console.WriteLine("Successfully reconnected to IRC server: " + myIp);
                    inputStream = new StreamReader(tcpClient.GetStream());
                    outputStream = new StreamWriter(tcpClient.GetStream());

                    outputStream.WriteLine("PASS " + myPassword);
                    outputStream.WriteLine("NICK " + username);
                    outputStream.WriteLine("USER " + username + " 8 * :" + username);
                    outputStream.Flush();
                }
                else
                {
                    Console.WriteLine("Failed to reconnect to IRC server: " + myIp);
                    lastReconnect = DateTime.Now;
                }
                lastReconnect = DateTime.Now;
            }
        }

        public string[] readMessage(Currency userList, string channel)
        {
            if (!IsConnected())
            {
                Reconnect();
            }

            try
            {
                if (tcpClient.GetStream().DataAvailable)
                {
                    string message = inputStream.ReadLine();
                    string[] temp = parseMessage(message);
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
                //Console.WriteLine("Bot disconnected. Reason: " + ex);
                string[] buf = { "Bot disconnected. Reason: " + ex };
                Reconnect();
                return buf;
                // Todo: Open a new connection, or otherwise gracefully close bot.
            }


        }

        public string[] readMessage()
        {
            if (!IsConnected())
            {
                Reconnect();
            }

            if (tcpClient.GetStream().DataAvailable)
            {
                string message = inputStream.ReadLine();
                string[] temp = parseMessage(message);

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
            if (!IsConnected())
            {
                Reconnect();
            }

            if (message.StartsWith("PING"))
            {
                //PONG
                sendIrcMessage("PONG tmi.twitch.tv\r\n");
            }
            if (message.Contains("•"))
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
                else if (message[i] == ':')
                {
                    count++;
                }
                else if (!serverParsed)
                {
                    if (message[i] == ' ')
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
                    string[] output = { name, buf };
                    return output;
                }
                else if (message.Contains("WHISPER"))
                {
                    string[] temp = message.Split(new char[] { ' ' }, 5);
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