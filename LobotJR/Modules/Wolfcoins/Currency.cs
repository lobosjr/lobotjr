using LobotJR.Client;
using LobotJR.Modules.Classes;
using LobotJR.Modules.Fishing;
using LobotJR.Modules.Items;
using LobotJR.Shared.Client;
using LobotJR.Shared.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace LobotJR.Modules.Wolfcoins
{
    public class Currency
    {

        public Dictionary<string, int> coinList = new Dictionary<string, int>();
        public Dictionary<string, int> xpList = new Dictionary<string, int>();
        public Dictionary<string, CharClass> classList = new Dictionary<string, CharClass>();
        public Dictionary<string, Fisherman> fishingList = new Dictionary<string, Fisherman>();
        public List<Fish> fishingLeaderboard = new List<Fish>();

        public Data viewers = new Data();
        List<string> viewerList = new List<string>();
        public HashSet<string> subSet = new HashSet<string>();
        public List<SubscriberData.Subscription> subsList = new List<SubscriberData.Subscription>();
        private readonly string path = "wolfcoins.json";
        private readonly string fishingPath = "fishing.json";
        private readonly string fishingLeaderboardPath = "fishingLeaderboard.json";
        private readonly string xpPath = "XP.json";
        private readonly string classPath = "classData.json";

        private readonly ClientData clientData;

        private const int COINMAX = Int32.MaxValue;

        public const int MAX_XP = 37094;
        public const int MAX_LEVEL = 20;

        public const int WARRIOR = 1;
        public const int MAGE = 2;
        public const int ROGUE = 3;
        public const int RANGER = 4;
        public const int CLERIC = 5;

        public const string clientID = "c95v57t6nfrpts7dqk2urruyc8d0ln1";
        public const int baseRespecCost = 250;

        public Currency(ClientData clientData)
        {
            this.clientData = clientData;
            Init();
        }

        public int XPForLevel(int level)
        {
            int xp = (int)(4 * (Math.Pow(level, 3)) + 50);
            return xp;
        }

        // algorithm is XP = 4 * (level^3) + 50
        public int DetermineLevel(string user)
        {
            if (Exists(xpList, user))
            {


                float xp = (float)xpList[user];

                if (xp <= 81)
                    return 1;

                float level = (float)Math.Pow((xp - 50.0f) / 4.0f, (1.0f / 3.0f));

                return (int)level;
            }
            return 0;
        }

        public string GloatWithPrestige(string user)
        {

            if (Exists(xpList, user) && Exists(classList, user))
            {


                float xp = (float)xpList[user];

                if (xp <= 81)
                    return "1";

                float level = (float)Math.Pow((xp - 50.0f) / 4.0f, (1.0f / 3.0f));

                string ret = " Level " + (int)level;

                if (classList[user].prestige > -1)
                {
                    int prestigeLevel = classList[user].prestige;
                    ret += ", Prestige Level " + (int)prestigeLevel;
                }

                return ret;
            }
            return "0";
        }

        public int DetermineLevel(int xp)
        {
            if (xp <= 54)
                return 1;

            float level = (float)Math.Pow((float)(xp - 50.0f) / 4.0f, (1.0f / 3.0f));
            return (int)level;
        }

        public string DetermineClass(string user)
        {
            if (classList != null)
            {
                if (classList.Keys.Contains(user))
                {
                    if (classList[user].classType != -1)
                    {
                        int userClass = classList[user].classType;
                        switch (userClass)
                        {
                            case 1:
                                {
                                    return "Warrior";
                                }

                            case 2:
                                {
                                    return "Mage";
                                }

                            case 3:
                                {
                                    return "Rogue";
                                }

                            case 4:
                                {
                                    return "Ranger";
                                }

                            case 5:
                                {
                                    return "Cleric";
                                }

                            default: break;
                        }
                    }
                }
            }
            return "INVALID CLASS";
        }

        public void AwardCoins(int coins)
        {

            for (int i = 0; i <= viewerList.Count - 1; i++)
            {
                //int value = 0;
                if (coinList != null)
                {
                    AddCoins(viewerList[i], coins.ToString());
                }

            }
            Console.WriteLine("Added " + coins + " coins to current viewers.");
        }

        public void AwardCoins(int coins, string user)
        {
            if (Exists(coinList, user))
            {

                if (coinList.ContainsKey(user) && int.TryParse(coins.ToString(), out int value))
                {
                    try
                    {
                        int prevCoins = coinList[user];
                        checked
                        {
                            coinList[user] += value;
                        }
                        if (coinList[user] > COINMAX)
                        {
                            coinList[user] = COINMAX;
                        }

                        if (coinList[user] < 0)
                            coinList[user] = 0;


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error adding coins.");
                        Console.WriteLine(e);
                    }

                }
            }
        }

        public void AwardXP(int xp, string user, IrcClient whisperClient)
        {
            //int value = 0;
            if (Exists(xpList, user))
            {
                int prevLevel = DetermineLevel(user);
                if (xpList == null)
                    return;

                if (xpList.ContainsKey(user) && int.TryParse(xp.ToString(), out int value))
                {
                    try
                    {
                        int prevXP = xpList[user];
                        checked
                        {
                            xpList[user] += value;
                        }


                        if (xpList[user] < 0)
                            xpList[user] = 0;

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error adding xp.");
                        Console.WriteLine(e);
                    }

                    int newLevel = DetermineLevel(user);


                    if (newLevel > MAX_LEVEL)
                    {
                        newLevel = 3;
                        classList[user].prestige++; //prestige code
                        xpList[user] = 200;
                        whisperClient.SendChatMessage(".w " + user + " You have earned a Prestige level! You are now Prestige " + classList[user].prestige + " and your level has been set to 3. XP to next level: " + XpToNextLevel(user) + ".");
                        return;
                    }

                    int myPrestige = classList[user].prestige; //prestige code

                    if (newLevel > prevLevel && newLevel != 3 && newLevel > 1)
                    {
                        //prestige code
                        if (myPrestige > 0)
                        {
                            whisperClient.SendChatMessage(".w " + user + " DING! You just reached Level " + newLevel + "! You are Prestige " + myPrestige + ". XP to next level: " + XpToNextLevel(user) + ".");
                        } //prestige code
                        else
                        {
                            whisperClient.SendChatMessage(".w " + user + " DING! You just reached Level " + newLevel + "! XP to next level: " + XpToNextLevel(user) + ".");
                        }

                        if (newLevel > 5)
                        {
                            classList[user].level = newLevel;
                        }
                    }

                    if (!(classList.ContainsKey(user)) && newLevel > prevLevel && newLevel == 3)
                    {
                        CharClass newClass = new CharClass
                        {
                            classType = -1
                        };
                        classList.Add(user.ToLower(), newClass);
                        whisperClient.SendChatMessage(".w " + user + " You've reached LEVEL 3! You get to choose a class for your character! Choose by whispering me one of the following: ");
                        whisperClient.SendChatMessage(".w " + user + " 'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)");
                    }

                }
            }
        }

        public void AwardXP(int xp, IrcClient whisperClient)
        {
            for (int i = 0; i <= viewerList.Count - 1; i++)
            {
                //int value = 0;
                if (xpList != null)
                {

                    string user = viewerList[i];
                    int prevLevel = DetermineLevel(user);
                    AddXP(user, xp.ToString());
                    int newLevel = DetermineLevel(user);
                    if (newLevel > MAX_LEVEL)
                    {
                        newLevel = 3;
                        classList[user].prestige++; //prestige code
                        xpList[user] = 0;
                        whisperClient.SendChatMessage(".w " + user + " You have earned a Prestige level! You are now Prestige " + classList[user].prestige + " and your level has been reset to 1. XP to next level: " + XpToNextLevel(user) + ".");
                        return;
                    }
                    int myPrestige = 0;
                    if (classList.ContainsKey(user))
                        myPrestige = classList[user].prestige; //prestige code

                    if (newLevel > prevLevel && newLevel != 3 && newLevel > 1)
                    {
                        //prestige code
                        if (myPrestige > 0)
                        {
                            whisperClient.SendChatMessage(".w " + user + " DING! You just reached Level " + newLevel + "! You are Prestige " + myPrestige + ". XP to next level: " + XpToNextLevel(user) + ".");
                        } //prestige code
                        else
                        {
                            whisperClient.SendChatMessage(".w " + user + " DING! You just reached Level " + newLevel + "! XP to next level: " + XpToNextLevel(user) + ".");
                        }
                        if (newLevel > 5)
                        {
                            if (classList.ContainsKey(user))
                                classList[user].level = newLevel;
                        }
                    }

                    if (newLevel > prevLevel && newLevel == 3)
                    {

                        CharClass newClass = new CharClass
                        {
                            classType = -1
                        };
                        if (classList.ContainsKey(user))
                            continue;

                        classList.Add(user.ToLower(), newClass);
                        whisperClient.SendChatMessage(".w " + user + " You've reached LEVEL 3! You get to choose a class for your character! Choose by whispering me one of the following: ");
                        whisperClient.SendChatMessage(".w " + user + " 'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)");
                    }
                }

            }
            Console.WriteLine("Granted " + xp + " xp to current viewers.");
        }

        public void SetClass(string user, string choice, IrcClient whisperClient)
        {
            switch (choice.ToLower())
            {
                case "c1":
                    {
                        classList[user] = new Warrior
                        {
                            name = user,
                            level = DetermineLevel(user),
                            itemEarned = -1
                        };
                        SaveClassData();
                        whisperClient.SendChatMessage(".w " + user + " You successfully selected the Warrior class!");
                    }
                    break;

                case "c2":
                    {
                        classList[user] = new Mage
                        {
                            name = user,
                            level = DetermineLevel(user),
                            itemEarned = -1
                        };
                        SaveClassData();
                        whisperClient.SendChatMessage(".w " + user + " You successfully selected the Mage class!");
                    }
                    break;

                case "c3":
                    {
                        classList[user] = new Rogue
                        {
                            name = user,
                            level = DetermineLevel(user),
                            itemEarned = -1
                        };
                        SaveClassData();
                        whisperClient.SendChatMessage(".w " + user + " You successfully selected the Rogue class!");
                    }
                    break;

                case "c4":
                    {
                        classList[user] = new Ranger
                        {
                            name = user,
                            level = DetermineLevel(user),
                            itemEarned = -1
                        };
                        SaveClassData();
                        whisperClient.SendChatMessage(".w " + user + " You successfully selected the Ranger class!");
                    }
                    break;

                case "c5":
                    {
                        classList[user] = new Cleric
                        {
                            name = user,
                            level = DetermineLevel(user),
                            itemEarned = -1
                        };
                        SaveClassData();
                        whisperClient.SendChatMessage(".w " + user + " You successfully selected the Cleric class!");
                    }
                    break;

                default: break;
            }
        }
        public int SetXP(int xp, string user, IrcClient whisperClient)
        {
            if (xpList != null)
            {
                if (xp > MAX_XP)
                    xp = MAX_XP - 1;

                if (xp < 0)
                    xp = 0;

                if (xpList.Keys.Contains(user))
                {
                    int prevLevel = DetermineLevel(user);
                    xpList[user] = xp;
                    int newLevel = DetermineLevel(user);

                    if (newLevel > MAX_LEVEL)
                    {
                        newLevel = MAX_LEVEL;
                    }

                    if (newLevel > prevLevel && newLevel != 3 && Exists(classList, user))
                    {
                        whisperClient.SendChatMessage(".w " + user + " DING! You just reached Level " + newLevel + "!  XP to next level: " + XpToNextLevel(user) + ".");
                        if (newLevel > 3)
                        {
                            if (Exists(classList, user))
                            {
                                classList[user].level = newLevel;
                                SaveClassData();
                            }
                        }
                    }

                    if (newLevel > prevLevel && newLevel >= 3 && classList != null & !classList.ContainsKey(user))
                    {
                        CharClass newChar = new CharClass
                        {
                            classType = -1,
                            level = newLevel
                        };
                        classList.Add(user.ToLower(), newChar);
                        whisperClient.SendChatMessage(".w " + user + " You've reached LEVEL " + newLevel + "! You get to choose a class for your character! Choose by whispering me one of the following: ");
                        whisperClient.SendChatMessage(".w " + user + " 'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)");
                        SaveClassData();
                    }

                    if (newLevel < prevLevel)
                    {
                        whisperClient.SendChatMessage(".w " + user + " You lost a level! :( You're now level: " + newLevel);
                        if (Exists(classList, user))
                        {
                            classList[user].level = newLevel;
                            SaveClassData();
                        }
                    }
                }
                else
                {
                    xpList.Add(user, xp);
                    Console.WriteLine("Added user " + user + " and set their XP to " + xp + ".");
                }
                SaveXP();

                return xp;
            }
            return -1;
        }

        public int SetCoins(int coins, string user)
        {

            if (coinList != null)
            {
                if (coins > COINMAX)
                    coins = COINMAX - 1;

                if (coins < 0)
                    coins = 0;

                if (coinList.Keys.Contains(user))
                {
                    coinList[user] = coins;
                    Console.WriteLine("Set " + user + "'s coins to " + coins + ".");
                }
                else
                {
                    coinList.Add(user, coins);
                    Console.WriteLine("Added user " + user + " and set their coins to " + coins + ".");
                }
                SaveCoins();

                return coins;
            }
            return -1;
        }

        public void UpdateSubs(string broadcastToken)
        {
            // ALERT: You can get rid of the dynamic lookup and just use a static channel id
            // Your channel id is 28640725
            var userData = Users.Get(broadcastToken, "lobosjr");
            var channelId = userData.Data.First().Id;
            var nextLink = $"https://api.twitch.tv/kraken/channels/{channelId}/subscriptions?limit=100&offset=0";
            var offset = 0;
            do
            {
                var request = (HttpWebRequest)WebRequest.Create(nextLink);
                request.Accept = "application/vnd.twitchtv.v5+json";
                request.Headers.Add("Client-ID", "c95v57t6nfrpts7dqk2urruyc8d0ln1");
                request.Headers.Add("Authorization", string.Format("OAuth {0}", broadcastToken));
                request.UserAgent = "LobosJrBot";

                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Console.WriteLine($"Unauthorized response retrieving subscribers using broadcast token {broadcastToken}");
                            break;
                        }
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            var data = reader.ReadToEnd();
                            var subList = JsonConvert.DeserializeObject<SubscriberData.RootObject>(data);
                            if (subList.subscriptions.Count > 0)
                            {
                                if (!string.IsNullOrWhiteSpace(subList._cursor))
                                {
                                    nextLink = $"https://api.twitch.tv/kraken/channels/{channelId}/subscriptions?cursor={subList._cursor}";
                                }
                                else
                                {
                                    offset += subList.subscriptions.Count;
                                    nextLink = $"https://api.twitch.tv/kraken/channels/{channelId}/subscriptions?limit=100&offset={offset}";
                                }
                                subsList.AddRange(subList.subscriptions);
                            }
                            else
                            {
                                nextLink = "";
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to retrieve full sub list.");
                    Console.WriteLine(e);
                    nextLink = "";
                }
            } while (!string.IsNullOrWhiteSpace(nextLink));

            foreach (var sub in subsList)
            {
                subSet.Add(sub.user.name);
            }
            Console.WriteLine("Subscriber list may or may not have been updated!");
        }

        public void UpdateViewers(string channel)
        {
            viewerList = new List<string>();
            bool updated = false;
            while (!updated)
            {
                using (var w = new WebClient())
                {
                    w.Proxy = null;
                    string url = "https://tmi.twitch.tv/group/user/" + channel + "/chatters";
                    try
                    {
                        var json = w.DownloadString(string.Format(url));
                        viewers = JsonConvert.DeserializeObject<Data>(json);

                        for (int i = 0; i < viewers.chatters.vips.Count; i++)
                        {
                            if (!viewerList.Contains(viewers.chatters.vips[i]))
                                viewerList.Add(viewers.chatters.vips[i]);
                        }
                        for (int i = 0; i < viewers.chatters.admins.Count; i++)
                        {
                            if (!viewerList.Contains(viewers.chatters.admins[i]))
                                viewerList.Add(viewers.chatters.admins[i]);
                        }

                        for (int i = 0; i < viewers.chatters.moderators.Count; i++)
                        {
                            if (!viewerList.Contains(viewers.chatters.moderators[i]))
                                viewerList.Add(viewers.chatters.moderators[i]);
                        }

                        for (int i = 0; i < viewers.chatters.viewers.Count; i++)
                        {
                            if (!viewerList.Contains(viewers.chatters.viewers[i]))
                                viewerList.Add(viewers.chatters.viewers[i]);
                        }

                        for (int i = 0; i < viewers.chatters.staff.Count; i++)
                        {
                            if (!viewerList.Contains(viewers.chatters.staff[i]))
                                viewerList.Add(viewers.chatters.staff[i]);
                        }

                        //Console.WriteLine("Updated viewer list.");
                        updated = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error updating viewers: " + e);
                    }
                }
            }
        }

        public bool Exists(Dictionary<string, int> dic)
        {
            return (dic != null);
        }

        public bool Exists(Dictionary<string, int> dic, string user)
        {
            if (dic != null)
            {
                return dic.Keys.Contains(user);
            }

            return false;
        }

        public bool Exists(Dictionary<string, Fisherman> dic, string user)
        {
            if (dic != null)
            {
                return dic.Keys.Contains(user);
            }

            return false;
        }

        public bool Exists(Dictionary<string, CharClass> dic, string user)
        {
            if (dic != null)
            {
                return dic.ContainsKey(user);
            }

            return false;
        }

        public bool CheckCoins(string user, int amount)
        {
            if (Exists(coinList, user))
            {
                if (amount <= coinList[user])
                {
                    return true;
                }
            }
            return false;
        }

        public bool AddCoins(string user, string coins)
        {
            if (coinList == null)
                return false;

            if (coinList.ContainsKey(user) && int.TryParse(coins, out int value))
            {
                try
                {
                    int prevCoins = coinList[user];
                    checked
                    {
                        if (subSet.Contains(user))
                        {
                            value *= 2;
                        }
                        coinList[user] += value;
                    }
                    if (coinList[user] > COINMAX)
                    {
                        coinList[user] = COINMAX;
                    }

                    if (coinList[user] < 0)
                        coinList[user] = 0;


                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error adding coins.");
                    Console.WriteLine(e);
                }

            }
            {
                if (!coinList.ContainsKey(user) && int.TryParse(coins, out value))
                {
                    coinList.Add(user, value);

                }
                else
                {
                    return false;
                }

            }
            return false;

        }

        public int XpToNextLevel(string user)
        {
            if (Exists(xpList, user))
            {
                int myXP = xpList[user];
                int myLevel = DetermineLevel(myXP);
                int xpNextLevel = (int)(4 * (Math.Pow(myLevel + 1, 3)) + 50);
                if (myLevel == 1)
                    return (82 - myXP);

                return (xpNextLevel - myXP);
            }
            else
            {
                return -1;
            }
        }

        public bool AddXP(string user, string xp)
        {
            if (xpList == null)
                return false;

            if (xpList.ContainsKey(user) && int.TryParse(xp, out int value))
            {
                try
                {
                    int prevXP = xpList[user];
                    if (subSet.Contains(user.ToLower()))
                    {
                        value *= 2;
                    }
                    checked
                    {
                        xpList[user] += value;
                    }
                    //if (xpList[user] > MAX_XP)
                    //{
                    //    xpList[user] = MAX_XP - 1;
                    //}

                    if (xpList[user] < 0)
                        xpList[user] = 0;

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error adding xp.");
                    Console.WriteLine(e);
                }


            }
            if (!xpList.ContainsKey(user) && int.TryParse(xp, out value))
            {
                if (value > MAX_XP)
                    value = MAX_XP - 1;

                if (value < 0)
                    value = 0;

                xpList.Add(user, value);

            }
            else
            {
                return false;
            }

            return false;

        }

        public bool RemoveCoins(string user, string coins)
        {
            if (coinList == null)
                return false;

            if (coinList.ContainsKey(user) && int.TryParse(coins, out int value))
            {
                if (value > 0)
                {
                    try
                    {
                        int prevCoins = coinList[user];
                        checked
                        {
                            coinList[user] -= value;
                        }
                        if (coinList[user] > COINMAX)
                            coinList[user] = COINMAX;

                        if (coinList[user] < 0)
                            coinList[user] = 0;

                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error removing coins.");
                        Console.WriteLine(e);
                    }
                }
            }
            return false;
        }

        public bool SaveCoins()
        {
            var json = JsonConvert.SerializeObject(coinList);
            var bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                File.WriteAllBytes(path, bytes);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving coins file: ");
                Console.WriteLine(e);
                return false;
            }

        }

        public void ChangeClass(string user, int newClass, IrcClient whisperClient)
        {
            if (classList != null && coinList != null)
            {
                if (classList.Keys.Contains(user) && coinList.Keys.Contains(user))
                {
                    int respecCost = (baseRespecCost * (classList[user].level - 4));
                    if (respecCost < baseRespecCost)
                        respecCost = baseRespecCost;

                    if (classList[user].classType != -1 && coinList[user] >= respecCost)
                    {
                        classList[user].myItems = new List<Item>();
                        classList[user].classType = newClass;
                        RemoveCoins(user, respecCost.ToString());

                        string myClass = DetermineClass(user);
                        classList[user].className = myClass;
                        whisperClient.SendChatMessage(".w " + user + " Class successfully updated to " + myClass + "! " + respecCost + " deducted from your Wolfcoin balance.");

                        SaveClassData();
                        SaveCoins();
                    }
                    else if (coinList[user] < respecCost)
                    {
                        whisperClient.SendChatMessage(".w " + user + " It costs " + respecCost + " Wolfcoins to respec at your level. You have " + coinList[user] + " coins.");
                    }
                }
            }
        }

        public bool BackupData()
        {
            var json = JsonConvert.SerializeObject(xpList);
            var bytes = Encoding.UTF8.GetBytes(json);
            string backupPath = "backup/XP";
            var json2 = JsonConvert.SerializeObject(classList);
            var bytes2 = Encoding.UTF8.GetBytes(json2);
            string backupPath2 = "backup/ClassData";
            var json3 = JsonConvert.SerializeObject(coinList);
            var bytes3 = Encoding.UTF8.GetBytes(json3);
            string backupPath3 = "backup/Coins";
            var json4 = JsonConvert.SerializeObject(fishingList);
            var bytes4 = Encoding.UTF8.GetBytes(json4);
            string backupPath4 = "backup/Fishing";
            var json5 = JsonConvert.SerializeObject(fishingLeaderboard);
            var bytes5 = Encoding.UTF8.GetBytes(json4);
            string backupPath5 = "backup/FishingLeaderboard";
            DateTime now = DateTime.Now;
            backupPath = backupPath + now.Day + now.Month + now.Year + now.Hour + now.Minute + now.Second;
            backupPath2 = backupPath2 + now.Day + now.Month + now.Year + now.Hour + now.Minute + now.Second;
            backupPath3 = backupPath3 + now.Day + now.Month + now.Year + now.Hour + now.Minute + now.Second;
            backupPath4 = backupPath4 + now.Day + now.Month + now.Year + now.Hour + now.Minute + now.Second;
            backupPath5 = backupPath5 + now.Day + now.Month + now.Year + now.Hour + now.Minute + now.Second;
            try
            {
                File.WriteAllBytes(backupPath, bytes);
                File.WriteAllBytes(backupPath2, bytes2);
                File.WriteAllBytes(backupPath3, bytes3);
                File.WriteAllBytes(backupPath4, bytes4);
                File.WriteAllBytes(backupPath5, bytes5);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving backup files: ");
                Console.WriteLine(e);
                return false;
            }

        }
        public bool SaveXP()
        {
            var json = JsonConvert.SerializeObject(xpList);
            var bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                File.WriteAllBytes(xpPath, bytes);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving XP file: ");
                Console.WriteLine(e);
                return false;
            }

        }

        public bool SaveFishingList()
        {
            var json = JsonConvert.SerializeObject(fishingList);
            var json2 = JsonConvert.SerializeObject(fishingLeaderboard);
            var bytes = Encoding.UTF8.GetBytes(json);
            var bytes2 = Encoding.UTF8.GetBytes(json2);
            try
            {
                File.WriteAllBytes(fishingPath, bytes);
                File.WriteAllBytes(fishingLeaderboardPath, bytes2);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving Fishing file: ");
                Console.WriteLine(e);
                return false;
            }

        }

        public bool SaveClassData()
        {
            var json = JsonConvert.SerializeObject(classList);
            var bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                File.WriteAllBytes(classPath, bytes);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving Class data file: ");
                Console.WriteLine(e);
                return false;
            }

        }

        public void Init()
        {
            if (File.Exists(path))
            {
                coinList = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(path));
                Console.WriteLine("Wolfcoins collection loaded.");
            }
            else
            {
                coinList = new Dictionary<string, int>();
                Console.WriteLine("Path not found. Coins initialized to default.");
            }

            if (File.Exists(xpPath))
            {
                xpList = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(xpPath));
                Console.WriteLine("Viewer XP loaded.");
            }
            else
            {
                xpList = new Dictionary<string, int>();
                Console.WriteLine("Path not found. XP file initialized to default.");
            }

            if (File.Exists(fishingPath))
            {
                fishingList = JsonConvert.DeserializeObject<Dictionary<string, Fisherman>>(File.ReadAllText(fishingPath));
                foreach (var player in fishingList)
                {
                    player.Value.isFishing = false;
                    player.Value.fishHooked = false;
                }
                Console.WriteLine("Fishing data loaded.");
            }
            if (File.Exists(fishingLeaderboardPath))
            {
                fishingLeaderboard = JsonConvert.DeserializeObject<List<Fish>>(File.ReadAllText(fishingLeaderboardPath));
                Console.WriteLine("Fishing data loaded.");
            }
            else
            {
                fishingLeaderboard = new List<Fish>();
                Console.WriteLine("Path not found. Fishing Leaderboard data initialized to default.");
            }

            if (File.Exists(classPath))
            {
                classList = JsonConvert.DeserializeObject<Dictionary<string, CharClass>>(File.ReadAllText(classPath));
                foreach (var player in classList)
                {
                    player.Value.groupID = -1;
                    player.Value.isPartyLeader = false;
                    player.Value.numInvitesSent = 0;
                    player.Value.pendingInvite = false;
                }
                Console.WriteLine("Class data loaded.");
            }
            else
            {
                classList = new Dictionary<string, CharClass>();
                Console.WriteLine("Path not found. Class data file initialized to default.");
            }
        }


    }
}
