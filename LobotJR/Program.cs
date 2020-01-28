using LobotJR.Client;
using LobotJR.Modules.Betting;
using LobotJR.Modules.Classes;
using LobotJR.Modules.Dungeons;
using LobotJR.Modules.Fishing;
using LobotJR.Modules.Group;
using LobotJR.Modules.Items;
using LobotJR.Modules.Pets;
using LobotJR.Modules.TwitchPlays;
using LobotJR.Modules.Wolfcoins;
using LobotJR.Shared.Authentication;
using LobotJR.Shared.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LobotJR
{

    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool twitchPlays = false;
            bool connected = true;
            bool broadcasting = false;

            // How often to award Wolfcoins in minutes
            const int SUCCEED = 0;
            const int FAIL = 1;

            const int LOW_DETAIL = 0;
            const int HIGH_DETAIL = 1;

            int awardMultiplier = 1;
            int awardInterval = 30;
            int awardAmount = 1;
            int awardTotal = 0;
            int gloatCost = 25;
            int pryCost = 1;

            Dictionary<int, Item> itemDatabase = new Dictionary<int, Item>();
            Dictionary<int, Pet> petDatabase = new Dictionary<int, Pet>();
            List<Fish> fishDatabase = new List<Fish>();

            var subathonPath = "C:/Users/Lobos/Dropbox/Stream/subathon.txt";
            IEnumerable<string> subathonFile;
            if (File.Exists(subathonPath))
            {
                subathonFile = File.ReadLines(subathonPath, UTF8Encoding.Default);
            }
            else
            {
                subathonFile = new List<string>();
                Console.WriteLine($"Failed to load subathon file, {subathonPath} not found.");
            }

            Dictionary<int, string> dungeonList = new Dictionary<int, string>();
            string dungeonListPath = "content/dungeonlist.ini";

            Dictionary<int, string> itemList = new Dictionary<int, string>();
            string itemListPath = "content/itemlist.ini";

            Dictionary<int, string> petList = new Dictionary<int, string>();
            string petListPath = "content/petlist.ini";

            Dictionary<int, string> fishingList = new Dictionary<int, string>();
            string fishingListPath = "content/fishlist.ini";

            Dictionary<int, Party> parties = new Dictionary<int, Party>();
            int maxPartyID = 0;

            GroupFinderQueue groupFinder;

            const int baseDungeonCost = 25;
            //const int baseRaidCost = 150;
            const int baseRespecCost = 250;

            Dictionary<string, Better> betters = new Dictionary<string, Better>();
            //string betStatement = "";
            bool betActive = false;
            bool betsAllowed = false;

            var clientData = FileUtils.ReadClientData();
            var tokenData = FileUtils.ReadTokenData();
            IrcClient irc = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);
            IrcClient group = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);
            // 199.9.253.119
            connected = irc.connected;

            if (connected)
            {
                UpdateTokens(tokenData, clientData);
                Console.WriteLine($"Logged in as {tokenData.ChatUser}");
                irc.SendIrcMessage("twitch.tv/membership");
            }
            if (group.connected)
            {
                group.SendIrcMessage("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            }
            if (!twitchPlays)
            {
                #region NormalBot
                string channel = tokenData.BroadcastUser;
                irc.JoinRoom(channel);
                group.JoinRoom("jtv");
                DateTime awardLast = DateTime.Now;
                Currency wolfcoins = new Currency(clientData);
                wolfcoins.UpdateViewers(channel);
                wolfcoins.UpdateSubs(tokenData.BroadcastToken.AccessToken);


                DungeonModule.UpdateDungeons(dungeonListPath, ref dungeonList);

                ItemModule.UpdateItems(itemListPath, ref itemList, ref itemDatabase);
                PetModule.UpdatePets(petListPath, ref petList, ref petDatabase);
                FishingModule.UpdateFish(fishingListPath, ref fishingList, ref fishDatabase);

                groupFinder = new GroupFinderQueue(dungeonList);

                foreach (var member in wolfcoins.classList)
                {
                    member.Value.ClearQueue();
                }
                wolfcoins.SaveClassData();

                DateTime lastConnectAttempt = DateTime.Now;

                while (true)
                {
                    if (!irc.connected)
                    {
                        UpdateTokens(tokenData, clientData);
                        if ((DateTime.Now - lastConnectAttempt).TotalSeconds > 5)
                        {
                            irc = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);
                            group = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);

                            lastConnectAttempt = DateTime.Now;
                            if (!connected)
                            {
                                Console.WriteLine("Disconnected from server. Attempting to reconnect in 30 seconds...");
                            }
                            else
                            {
                                Console.WriteLine("Reconnected to server.");
                            }
                        }
                        continue;
                    }

                    // message[0] has username, message[1] has message
                    string[] message = irc.ReadMessage(wolfcoins, channel);
                    string[] whispers = group.ReadMessage(wolfcoins, channel);
                    string whisperSender;
                    string whisperMessage;

                    if (irc.messageQueue.Count > 0)
                    {
                        irc.ProcessQueue();
                    }

                    if (group.messageQueue.Count > 0)
                    {
                        group.ProcessQueue();
                    }

                    FishingModule.Process(wolfcoins, group, fishDatabase);
                    var toRemove = GroupModule.Process(parties, itemDatabase, petDatabase, wolfcoins, irc, group);
                    GroupModule.RemoveParties(toRemove, parties, wolfcoins, group);                    

                    if (((DateTime.Now - awardLast).TotalMinutes > awardInterval))
                    {
                        if (broadcasting)
                        {
                            awardTotal = awardAmount * awardMultiplier;
                            wolfcoins.UpdateViewers(channel);

                            // Halloween Treats
                            //Random rnd = new Random();
                            //int numViewers = wolfcoins.viewers.chatters.viewers.Count;
                            //int winner = rnd.Next(0, (numViewers - 1));
                            //string winnerName = wolfcoins.viewers.chatters.viewers.ElementAt(winner);
                            //int coinsToAward = (rnd.Next(5, 10)) * 50;
                            //wolfcoins.AddCoins(winnerName, coinsToAward.ToString());

                            wolfcoins.AwardCoins(awardTotal * 3); // Give 3x as many coins as XP
                            wolfcoins.AwardXP(awardTotal, group);
                            //string path2 = "C:/Users/Lobos/AppData/Roaming/DarkSoulsII/01100001004801af/`s" + DateTime.Now.Ticks + ".sl2";
                            //File.Copy(@"C:\Users\Lobos\AppData\Roaming\DarkSoulsII\01100001004801af\DS2SOFS0000.sl2", @path2);
                            //string path = "C:/Users/Lobos/AppData/Roaming/DarkSoulsIII/01100001004801af/Backups/DS30000_" + DateTime.Now.Ticks + ".sl2";
                            //File.Copy(@"C:/Users/Lobos/AppData/Roaming/DarkSoulsIII/01100001004801af/DS30000.sl2", @path);
                            irc.SendChatMessage("Thanks for watching! Viewers awarded " + awardTotal + " XP & " + (awardTotal * 3) + " Wolfcoins. Subscribers earn double that amount!");
                            //irc.sendChatMessage("Happy Halloween! Viewer " + winnerName + " just won a treat of " + coinsToAward + " wolfcoins!");
                        }

                        wolfcoins.SaveCoins();
                        wolfcoins.SaveXP();
                        wolfcoins.SaveClassData();
                        awardLast = DateTime.Now;
                    }

                    #region whisperRegion
                    if (whispers.Length > 1)
                    {
                        if (whispers[0] != null && whispers[1] != null)
                        {

                            whisperSender = whispers[0];
                            whisperMessage = whispers[1];

                            group.Whisper(whisperSender, "Ping!");
                            if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                            {
                                if (wolfcoins.DetermineLevel(whisperSender) >= 3 && wolfcoins.DetermineClass(whisperSender) == "INVALID CLASS" && !whisperMessage.StartsWith("c") && !whisperMessage.StartsWith("C"))
                                {
                                    group.Whisper(whisperSender, "ATTENTION! You are high enough level to pick a class, but have not picked one yet! Whisper me one of the following to choose your class: ");
                                    group.Whisper(whisperSender, "'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)");
                                }
                            }
                            if (whisperMessage == "?" || whisperMessage == "help" || whisperMessage == "!help" || whisperMessage == "faq" || whisperMessage == "!faq")
                            {
                                //Whisper(whisperSender, "Help command coming soon. For now, know that only viewers Level 2 & higher can post hyperlinks. This helps keep chat free of bots!");

                                group.Whisper(whisperSender, "Hi I'm LobotJR! I'm a chat bot written by LobosJR to help out with things.  To ask me about a certain topic, whisper me the number next to what you want to know about! (Ex: Whisper me 1 for information on Wolfcoins)");
                                group.Whisper(whisperSender, "Here's a list of things you can ask me about: Wolfcoins (1) - Leveling System (2)");

                            }
                            else if (whisperMessage == "!cleartesters" && whisperSender == "lobosjr")
                            {
                                //string[] users = { "lobosjr", "spectrumknight", "floogoss", "shoumpaloumpa", "nemesis_of_green", "donotgogently", "twitchmage", "kidgreen4", "cuddling", "androsv", "jaranous94", "lambchop2559", "hockeyboy1257", "dumj00", "stennisberetheon", "bionicmeech", "blargh201", "arampizzatime"};
                                //for(int i = 0; i < users.Length; i++)
                                //{
                                //    if (wolfcoins.Exists(wolfcoins.classList, users[i]))
                                //    {
                                //        wolfcoins.classList.Remove(users[i]);
                                //        wolfcoins.SetXP(1, users[i], group);
                                //        wolfcoins.SetXP(600, users[i], group);
                                //    }
                                //    else
                                //    {
                                //        wolfcoins.SetXP(1, users[i], group);
                                //        wolfcoins.SetXP(600, users[i], group);
                                //    }
                                //}

                            }
                            else if (whisperMessage.StartsWith("!dungeon"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    string[] msgData = whispers[1].Split(' ');
                                    int dungeonID = -1;
                                    if (msgData.Count() > 1)
                                    {
                                        int.TryParse(msgData[1], out dungeonID);
                                        if (dungeonID != -1 && dungeonList.ContainsKey(dungeonID))
                                        {
                                            string dungeonPath = "content/dungeons/" + dungeonList[dungeonID];
                                            Dungeon tempDungeon = new Dungeon(dungeonPath);
                                            group.Whisper(whisperSender, tempDungeon.dungeonName + " (Levels " + tempDungeon.minLevel + " - " + tempDungeon.maxLevel + ") -- " + tempDungeon.description);
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Invalid Dungeon ID provided.");
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!bug"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    string[] msgData = whispers[1].Split(' ');
                                    if (msgData.Count() > 1)
                                    {
                                        string bugMessage = "";
                                        for (int i = 1; i < msgData.Count(); i++)
                                        {
                                            bugMessage += msgData[i] + " ";
                                        }

                                        string logPath = "bugreports.log";
                                        System.IO.File.AppendAllText(logPath, whisperSender + ": " + bugMessage + Environment.NewLine);
                                        System.IO.File.AppendAllText(logPath, "------------------------------------------" + Environment.NewLine);

                                        group.Whisper(whisperSender, "Bug report submitted.");
                                        group.Whisper("lobosjr", DateTime.Now + ": " + whisperSender + " submitted a bug report.");
                                        Console.WriteLine(DateTime.Now + ": " + whisperSender + " submitted a bug report.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!item"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myItems.Count == 0)
                                    {
                                        group.Whisper(whisperSender, "You have no items.");
                                        continue;
                                    }

                                    string[] msgData = whispers[1].Split(' ');
                                    int invID = -1;
                                    if (msgData.Count() > 1)
                                    {
                                        int.TryParse(msgData[1], out invID);
                                        if (invID != -1)
                                        {
                                            bool itemFound = false;
                                            foreach (var item in wolfcoins.classList[whisperSender].myItems)
                                            {
                                                if (item.inventoryID == invID)
                                                {
                                                    string desc = itemDatabase[item.itemID - 1].description;
                                                    string name = itemDatabase[item.itemID - 1].itemName;
                                                    group.Whisper(whisperSender, name + " -- " + desc);
                                                    itemFound = true;
                                                    break;
                                                }
                                            }
                                            if (!itemFound)
                                            {
                                                group.Whisper(whisperSender, "Invalid Inventory ID provided.");
                                            }
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Invalid Inventory ID provided.");
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!updateviewers")
                            {
                                if (whisperSender != "lobosjr")
                                {
                                    continue;
                                }

                                wolfcoins.UpdateViewers(channel);
                            }
                            else if (whisperMessage == "!updateitems")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                ItemModule.UpdateItems(itemListPath, ref itemList, ref itemDatabase);


                                foreach (var player in wolfcoins.classList)
                                {
                                    if (player.Value.myItems.Count == 0)
                                        continue;

                                    foreach (var item in player.Value.myItems)
                                    {
                                        Item newItem = itemDatabase[item.itemID - 1];

                                        item.itemName = newItem.itemName;
                                        item.itemRarity = newItem.itemRarity;
                                        item.itemFind = newItem.itemFind;
                                        item.successChance = newItem.successChance;
                                        item.coinBonus = newItem.coinBonus;
                                        item.preventDeathBonus = newItem.preventDeathBonus;
                                        item.xpBonus = newItem.xpBonus;
                                    }
                                }
                            }
                            else if (whisperMessage == "!godmode")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                wolfcoins.classList["lobosjr"].successChance = 1000;

                            }
                            else if (whisperMessage.StartsWith("!addplayer"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() == 2)
                                {
                                    string name = msgData[1];
                                    string toSend = "";
                                    if (!wolfcoins.classList.ContainsKey(name))
                                    {
                                        wolfcoins.classList.Add(name.ToLower(), new CharClass());
                                        toSend += "class, ";
                                    }

                                    if (!wolfcoins.coinList.ContainsKey(name))
                                    {
                                        wolfcoins.coinList.Add(name, 0);
                                        toSend += "coin, ";
                                    }

                                    if (!wolfcoins.xpList.ContainsKey(name))
                                    {
                                        wolfcoins.xpList.Add(name, 0);
                                        toSend += "xp";
                                    }

                                    group.Whisper("lobosjr", name + " added to the following lists: " + toSend);
                                }
                            }
                            else if (whisperMessage.StartsWith("!transfer"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 2 && msgData.Count() < 4)
                                {
                                    string prevName = msgData[1];
                                    string newName = msgData[2];


                                    if (!wolfcoins.coinList.ContainsKey(prevName) || !wolfcoins.xpList.ContainsKey(prevName))
                                    {
                                        group.Whisper(whisperSender, prevName + " has no stats to transfer.");
                                        continue;
                                    }

                                    if (!wolfcoins.coinList.ContainsKey(newName))
                                    {
                                        wolfcoins.coinList.Add(newName, 0);
                                    }

                                    if (!wolfcoins.xpList.ContainsKey(newName))
                                    {
                                        wolfcoins.xpList.Add(newName, 0);
                                    }

                                    int prevCoins = wolfcoins.coinList[prevName];
                                    int prevXP = wolfcoins.xpList[prevName];

                                    wolfcoins.coinList[newName] += prevCoins;
                                    wolfcoins.xpList[newName] += prevXP;

                                    if (!wolfcoins.classList.ContainsKey(newName))
                                    {
                                        CharClass playerClass = new CharClass();
                                        wolfcoins.classList.Add(newName.ToLower(), new CharClass());
                                    }

                                    group.Whisper(whisperSender, "Transferred " + prevName + "'s xp/coins to " + newName + ".");
                                    group.Whisper(newName, "Your xp/coin total has been updated by Lobos! Thanks for playing the RPG lobosHi");

                                    wolfcoins.SaveCoins();
                                    wolfcoins.SaveXP();

                                }
                            }
                            else if (whisperMessage.StartsWith("!checkpets"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 1)
                                {
                                    string toCheck = msgData[1];
                                    foreach (var pet in wolfcoins.classList[toCheck].myPets)
                                    {
                                        PetModule.WhisperPet(whisperSender, pet, group, LOW_DETAIL);
                                    }

                                }
                                else
                                {
                                    group.Whisper(whisperSender, "!checkpets <username>");
                                }
                            }
                            else if (whisperMessage.StartsWith("!grantpet"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 1)
                                {
                                    int rarity = -1;
                                    if (int.TryParse(msgData[1], out rarity))
                                    {
                                        wolfcoins.classList[whisperSender].petEarned = rarity;
                                    }
                                }
                                else
                                {
                                    Random rng = new Random();
                                    wolfcoins.classList[whisperSender].petEarned = rng.Next(1, 6);
                                }
                                Dictionary<int, Pet> allPets = petDatabase;
                                PetModule.GrantPet(whisperSender, wolfcoins, allPets, irc, group);
                                //Random RNG = new Random();

                                //Pet newPet = new Pet();

                                //int petToAward = RNG.Next(1, petDatabase.Count);
                                //newPet = petDatabase[petToAward];
                                //int sparklyCheck = RNG.Next(1, 100);

                                //if (sparklyCheck == 1)
                                //    newPet.isSparkly = true;

                                //wolfcoins.classList[whisperSender].myPets.Count++;
                                //newPet.stableID = wolfcoins.classList[whisperSender].myPets.Count;

                                //wolfcoins.classList[whisperSender].myPets.Add(newPet);

                                //group.Whisper(whisperSender, "Added a random pet.");

                            }
                            else if (whisperMessage == "!clearpets")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                wolfcoins.classList[whisperSender].myPets = new List<Pet>();
                                wolfcoins.classList[whisperSender].toRelease = new Pet();
                                wolfcoins.classList[whisperSender].pendingPetRelease = false;

                                group.Whisper(whisperSender, "Pets cleared.");

                            }
                            else if (whisperMessage == "!updatedungeons")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                DungeonModule.UpdateDungeons(dungeonListPath, ref dungeonList);
                            }
                            else if (whisperMessage.StartsWith("/p") || whisperMessage.StartsWith("/party"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].groupID != -1)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() > 1)
                                        {
                                            string partyMessage = "";
                                            for (int i = 1; i < msgData.Count(); i++)
                                            {
                                                partyMessage += msgData[i] + " ";
                                            }
                                            int partyID = wolfcoins.classList[whisperSender].groupID;
                                            foreach (var member in parties[partyID].members)
                                            {
                                                if (member.name == whisperSender)
                                                {
                                                    group.Whisper(member.name, "You whisper: \" " + partyMessage + "\" ");
                                                    continue;
                                                }

                                                group.Whisper(member.name, whisperSender + " says: \" " + partyMessage + "\" ");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!respec")
                            {
                                if (wolfcoins.classList != null)
                                {
                                    if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()) && wolfcoins.DetermineClass(whisperSender) != "INVALID_CLASS")
                                    {
                                        if (wolfcoins.classList[whisperSender].groupID == -1)
                                        {
                                            if (wolfcoins.Exists(wolfcoins.coinList, whisperSender))
                                            {

                                                int respecCost = (baseRespecCost * (wolfcoins.classList[whisperSender].level - 4));
                                                if (respecCost < baseRespecCost)
                                                    respecCost = baseRespecCost;

                                                if (wolfcoins.coinList[whisperSender] <= respecCost)
                                                {
                                                    group.Whisper(whisperSender, "It costs " + respecCost + " Wolfcoins to respec at your level. You have " + wolfcoins.coinList[whisperSender] + " coins.");
                                                }
                                                int classNumber = wolfcoins.classList[whisperSender].classType * 10;
                                                wolfcoins.classList[whisperSender].classType = classNumber;

                                                group.Whisper(whisperSender, "You've chosen to respec your class! It will cost you " + respecCost + " coins to respec and you will lose all your items. Reply 'Nevermind' to cancel or one of the following codes to select your new class: ");
                                                group.Whisper(whisperSender, "'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)");
                                            }
                                            else
                                            {
                                                group.Whisper(whisperSender, "You have no coins to respec with.");
                                            }
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "You can't respec while in a party!");
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!inventory" || whisperMessage == "!inv" || whisperMessage == "inv" || whisperMessage == "inventory")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myItems.Count > 0)
                                    {
                                        group.Whisper(whisperSender, "You have " + wolfcoins.classList[whisperSender].myItems.Count + " items: ");
                                        foreach (var item in wolfcoins.classList[whisperSender].myItems)
                                        {
                                            ItemModule.WhisperItem(whisperSender, item, group, itemDatabase);
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You have no items.");
                                    }
                                }
                            }
                            else if (whisperMessage == "!fishleaders" || whisperMessage == "!leaderboards")
                            {

                                foreach (var fish in wolfcoins.fishingLeaderboard)
                                {
                                    group.Whisper(whisperSender, "Largest " + fish.name + " caught by " + fish.caughtBy + " at " + fish.weight + " lbs.");
                                }
                            }
                            else if (whisperMessage == "!fish")
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        group.Whisper(whisperSender, "You've caught " + wolfcoins.fishingList[whisperSender].biggestFish.Count + " different types of fish: ");
                                        FishingModule.WhisperFish(whisperSender, wolfcoins.fishingList[whisperSender].biggestFish, group);

                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You haven't caught any fish yet!");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!fish"))
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>");
                                            continue;
                                        }
                                        int fishID = -1;
                                        if (int.TryParse(msgData[1], out fishID))
                                        {
                                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                                            {
                                                FishingModule.WhisperFish(whisperSender, wolfcoins.fishingList, fishID, group);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!releasefish"))
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>");
                                            continue;
                                        }
                                        int fishID = -1;
                                        if (int.TryParse(msgData[1], out fishID))
                                        {
                                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                                            {
                                                string fishName = wolfcoins.fishingList[whisperSender].biggestFish[fishID - 1].name;
                                                wolfcoins.fishingList[whisperSender].biggestFish.RemoveAt(fishID - 1);

                                                group.Whisper(whisperSender, "You released your " + fishName + ". Bye bye!");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!");
                                    }
                                }
                            }
                            else if (whisperMessage == "!pets" || whisperMessage == "!stable" || whisperMessage == "pets" || whisperMessage == "stable")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        group.Whisper(whisperSender, "You have " + wolfcoins.classList[whisperSender].myPets.Count + " pets: ");
                                        foreach (var pet in wolfcoins.classList[whisperSender].myPets)
                                        {
                                            PetModule.WhisperPet(whisperSender, pet, group, LOW_DETAIL);
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You have no pets.");
                                    }
                                }
                            }

                            else if (whisperMessage == "!pethelp")
                            {
                                group.Whisper(whisperSender, "View all your pets by whispering me '!pets'. View individual pet stats using '!pet <stable id>' where the id is the number next to your pet's name in brackets [].");
                                group.Whisper(whisperSender, "A summoned/active pet will join you on dungeon runs and possibly even bring benefits! But this will drain its energy, which you can restore by feeding it.");
                                group.Whisper(whisperSender, "You can !dismiss, !summon, !release, !feed, and !hug* your pets using their stable id (ex: !summon 2)");
                                group.Whisper(whisperSender, "*: In development, available soon!");
                            }
                            else if (whisperMessage.StartsWith("!fixpets"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                //string[] msgData = whispers[1].Split(' ');
                                //if (msgData.Count() != 2)
                                //{
                                //    group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !feed <stable ID>");
                                //    continue;
                                //}

                                //string playerToFix = msgData[1];
                                foreach (var player in wolfcoins.classList)
                                {
                                    if (player.Value.myPets.Count == 0)
                                    {
                                        continue;
                                    }
                                    int stableIDFix = 1;
                                    foreach (var pet in player.Value.myPets)
                                    {
                                        pet.stableID = stableIDFix;
                                        stableIDFix++;
                                    }
                                    group.Whisper(whisperSender, "Fixed " + player.Value.name + "'s pet IDs.");
                                }
                            }
                            else if (whisperMessage.StartsWith("!feed"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.Exists(wolfcoins.coinList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !feed <stable ID>");
                                            continue;
                                        }
                                        int petToFeed = -1;
                                        if (int.TryParse(msgData[1], out petToFeed))
                                        {

                                            if (petToFeed > wolfcoins.classList[whisperSender].myPets.Count || petToFeed < 1)
                                            {
                                                group.Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!");
                                                continue;
                                            }

                                            if (wolfcoins.coinList[whisperSender] < 5)
                                            {
                                                group.Whisper(whisperSender, "You lack the 5 wolfcoins to feed your pet! Hop in a Lobos stream soon!");
                                                continue;
                                            }

                                            // build a dummy pet to do calculations
                                            Pet tempPet = wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1);

                                            if (tempPet.hunger >= Pet.HUNGER_MAX)
                                            {
                                                group.Whisper(whisperSender, tempPet.name + " is full and doesn't need to eat!");
                                                continue;
                                            }

                                            int currentHunger = tempPet.hunger;
                                            int currentXP = tempPet.xp;
                                            int currentLevel = tempPet.level;
                                            int currentAffection = tempPet.affection + Pet.FEEDING_AFFECTION;

                                            // Charge the player for pet food
                                            wolfcoins.coinList[whisperSender] = wolfcoins.coinList[whisperSender] - Pet.FEEDING_COST;

                                            group.Whisper(whisperSender, "You were charged " + Pet.FEEDING_COST + " wolfcoins to feed " + tempPet.name + ". They feel refreshed!");
                                            // earn xp equal to amount of hunger 'fed'
                                            currentXP += (Pet.HUNGER_MAX - currentHunger);

                                            // check if pet leveled
                                            if (currentXP >= Pet.XP_TO_LEVEL && currentLevel < Pet.LEVEL_MAX)
                                            {
                                                currentLevel++;
                                                currentXP = currentXP - Pet.XP_TO_LEVEL;
                                                group.Whisper(whisperSender, tempPet.name + " leveled up! They are now level " + currentLevel + ".");
                                            }
                                            // refill hunger value
                                            currentHunger = Pet.HUNGER_MAX;

                                            // update temp pet w/ new data
                                            tempPet.affection = currentAffection;
                                            tempPet.hunger = currentHunger;
                                            tempPet.xp = currentXP;
                                            tempPet.level = currentLevel;

                                            // update actual pet data
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).affection = currentAffection;
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).hunger = currentHunger;
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).xp = currentXP;
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).level = currentLevel;

                                            wolfcoins.SaveClassData();
                                            wolfcoins.SaveCoins();
                                        }

                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!sethunger"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 3)
                                {
                                    group.Whisper(whisperSender, "Too many parameters. Syntax: !sethunger <stable ID>");
                                    continue;
                                }
                                int petToSet = -1;
                                int amount = -1;
                                if (int.TryParse(msgData[1], out petToSet) && int.TryParse(msgData[2], out amount))
                                {
                                    wolfcoins.classList[whisperSender].myPets.ElementAt(petToSet - 1).hunger = amount;
                                    group.Whisper(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(petToSet - 1).name + "'s energy set to " + amount + ".");
                                }
                                else
                                {
                                    group.Whisper(whisperSender, "Ya dun fucked somethin' up.");
                                }
                            }
                            else if (whisperMessage.StartsWith("!release"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !release <stable ID>");
                                            continue;
                                        }
                                        int petToRelease = -1;
                                        if (int.TryParse(msgData[1], out petToRelease))
                                        {

                                            if (petToRelease > wolfcoins.classList[whisperSender].myPets.Count || petToRelease < 1)
                                            {
                                                group.Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!");
                                                continue;
                                            }
                                            string petName = wolfcoins.classList[whisperSender].myPets.ElementAt(petToRelease - 1).name;
                                            //wolfcoins.classList[whisperSender].toRelease = petToRelease;
                                            wolfcoins.classList[whisperSender].pendingPetRelease = true;
                                            wolfcoins.classList[whisperSender].toRelease = new Pet
                                            {
                                                stableID = wolfcoins.classList[whisperSender].myPets.ElementAt(petToRelease - 1).stableID
                                            };
                                            group.Whisper(whisperSender, "If you release " + petName + ", they will be gone forever. Are you sure you want to release them? (y/n)");
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have a pet.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!dismiss"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !dismiss <stable ID>");
                                            continue;
                                        }
                                        int petToDismiss = -1;
                                        if (int.TryParse(msgData[1], out petToDismiss))
                                        {
                                            if (petToDismiss > wolfcoins.classList[whisperSender].myPets.Count || petToDismiss < 1)
                                            {
                                                group.Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!");
                                                continue;
                                            }
                                            if (wolfcoins.classList[whisperSender].myPets.ElementAt(petToDismiss - 1).isActive)
                                            {
                                                wolfcoins.classList[whisperSender].myPets.ElementAt(petToDismiss - 1).isActive = false;
                                                group.Whisper(whisperSender, "You dismissed " + wolfcoins.classList[whisperSender].myPets.ElementAt(petToDismiss - 1).name + ".");
                                                wolfcoins.SaveClassData();
                                            }
                                            else
                                            {
                                                group.Whisper(whisperSender, "That pet is not currently summoned.");
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have a pet.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!summon"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !summon <stable ID>");
                                            continue;
                                        }
                                        int petToSummon = -1;
                                        int currentlyActivePet = -1;
                                        if (int.TryParse(msgData[1], out petToSummon))
                                        {
                                            if (petToSummon > wolfcoins.classList[whisperSender].myPets.Count || petToSummon < 1)
                                            {
                                                group.Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!");
                                                continue;
                                            }
                                            foreach (var pet in wolfcoins.classList[whisperSender].myPets)
                                            {
                                                if (pet.isActive)
                                                {
                                                    currentlyActivePet = pet.stableID;
                                                }
                                            }
                                            if (currentlyActivePet > wolfcoins.classList[whisperSender].myPets.Count)
                                            {
                                                group.Whisper(whisperSender, "Sorry, your stableID is corrupt. Lobos is working on this issue :(");
                                                continue;
                                            }
                                            if (!wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).isActive)
                                            {
                                                wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).isActive = true;
                                                group.Whisper(whisperSender, "You summoned " + wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).name + ".");
                                                if (currentlyActivePet != -1)
                                                {
                                                    wolfcoins.classList[whisperSender].myPets.ElementAt(currentlyActivePet - 1).isActive = false;
                                                    group.Whisper(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(currentlyActivePet - 1).name + " was dismissed.");
                                                }
                                            }
                                            else
                                            {
                                                group.Whisper(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).name + " is already summoned!");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have a pet.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!pet"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !pet <stable ID>");
                                            continue;
                                        }
                                        int petToSend = -1;
                                        if (int.TryParse(msgData[1], out petToSend))
                                        {
                                            if (petToSend > wolfcoins.classList[whisperSender].myPets.Count || petToSend < 1)
                                            {
                                                group.Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!");
                                                continue;
                                            }
                                            PetModule.WhisperPet(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(petToSend - 1), group, HIGH_DETAIL);
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any pets.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!rename"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 3)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Note: names cannot contain spaces.");
                                            continue;
                                        }
                                        else if (msgData.Count() == 3)
                                        {
                                            int petToRename = -1;
                                            if (int.TryParse(msgData[1], out petToRename))
                                            {
                                                if (petToRename > (wolfcoins.classList[whisperSender].myPets.Count) || petToRename < 1)
                                                {
                                                    group.Whisper(whisperSender, "Sorry, the Stable ID given was invalid. Please try again.");
                                                    continue;
                                                }
                                                string newName = msgData[2];
                                                if (newName.Length > 16)
                                                {
                                                    group.Whisper(whisperSender, "Name can only be 16 characters max.");
                                                    continue;
                                                }
                                                string prevName = wolfcoins.classList[whisperSender].myPets.ElementAt(petToRename - 1).name;
                                                wolfcoins.classList[whisperSender].myPets.ElementAt(petToRename - 1).name = newName;
                                                group.Whisper(whisperSender, prevName + " was renamed to " + newName + "!");
                                            }
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Sorry, the data you provided didn't work. Syntax: !rename <stable id> <new name>");
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any pets to rename. :(");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!catch") || whisperMessage.StartsWith("!reel"))
                            {
                                if ((wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                                {
                                    if (wolfcoins.fishingList[whisperSender].fishHooked && wolfcoins.fishingList[whisperSender].hookedFishID != -1)
                                    {
                                        Fish myCatch = new Fish();

                                        foreach (var fish in fishDatabase)
                                        {
                                            if (fish.ID == wolfcoins.fishingList[whisperSender].hookedFishID)
                                            {
                                                myCatch = (wolfcoins.fishingList[whisperSender].Catch(new Fish(fish), group));
                                            }
                                        }

                                        // update leaderboard
                                        bool matchFound = false;
                                        for (int i = 0; i < wolfcoins.fishingLeaderboard.Count; i++)
                                        {
                                            if (wolfcoins.fishingLeaderboard.ElementAt(i).ID == myCatch.ID)
                                            {
                                                matchFound = true;
                                                if (myCatch.weight > wolfcoins.fishingLeaderboard.ElementAt(i).weight)
                                                {
                                                    wolfcoins.fishingLeaderboard[i] = new Fish(myCatch);
                                                    irc.SendChatMessage(whisperSender + " just caught the heaviest " + myCatch.name + " ever! It weighs " + myCatch.weight + " pounds!");
                                                    break;
                                                }
                                            }
                                        }
                                        if (!matchFound)
                                        {
                                            wolfcoins.fishingLeaderboard.Add(new Fish(myCatch));
                                            irc.SendChatMessage(whisperSender + " just caught the heaviest " + myCatch.name + " ever! It weighs " + myCatch.weight + " pounds!");

                                        }

                                        wolfcoins.SaveFishingList();

                                        group.Whisper(whisperSender, "Congratulations! You caught a " + myCatch.length + " inch, " +
                                            myCatch.weight + " pound " + myCatch.name + "!");

                                        wolfcoins.fishingList[whisperSender].fishHooked = false;
                                        wolfcoins.fishingList[whisperSender].hookedFishID = -1;



                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!cast"))
                            {
                                if (!(wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                                {
                                    // first time fishing! initialize all necessary values
                                    Fisherman temp = new Fisherman
                                    {
                                        username = whisperSender,
                                        level = 1,
                                        XP = 0,
                                        lure = 0
                                    };

                                    // add new fisherman and save
                                    wolfcoins.fishingList.Add(whisperSender, temp);
                                    wolfcoins.SaveFishingList();

                                }
                                if (wolfcoins.fishingList[whisperSender].isFishing)
                                {
                                    group.Whisper(whisperSender, "Your line is already cast! I'm sure a fish'll be along soon...");
                                    continue;
                                }

                                if (wolfcoins.fishingList[whisperSender].fishHooked)
                                {
                                    group.Whisper(whisperSender, "Something's already bit your line! Quick, type !catch to snag it!");
                                    continue;
                                }

                                // min/max time, in seconds, before a fish will bite
                                int minimumCastTime = 60;
                                int maximumCastTime = 600;
                                // determine when a fish will bite
                                Random rng = new Random();
                                int elapsedTime = rng.Next(minimumCastTime, maximumCastTime);

                                wolfcoins.fishingList[whisperSender].timeOfCatch = DateTime.Now.AddSeconds(elapsedTime);
                                wolfcoins.fishingList[whisperSender].isFishing = true;

                                group.Whisper(whisperSender, "You cast your line out into the water.");
                            }
                            else if (whisperMessage == "!debugcast")
                            {
                                // min/max time, in seconds, before a fish will bite
                                if (whisperSender == "lobosjr")
                                {
                                    wolfcoins.fishingList[whisperSender].timeOfCatch = DateTime.Now.AddSeconds(2);
                                    wolfcoins.fishingList[whisperSender].isFishing = true;

                                    group.Whisper(whisperSender, "You cast your line out into the water.");
                                }
                            }
                            else if (whisperMessage.StartsWith("!start"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    int partyID = wolfcoins.classList[whisperSender].groupID;
                                    if (parties.Count() > 0 && partyID != -1 && parties.ContainsKey(partyID))
                                    {
                                        if (parties[partyID].status == GroupModule.PARTY_READY)
                                        {
                                            if (!(parties[partyID].partyLeader == whisperSender))
                                            {
                                                group.Whisper(whisperSender, "You are not the party leader!");
                                                continue;
                                            }
                                            string[] msgData = whispers[1].Split(' ');
                                            int dungeonID = -1;
                                            if (msgData.Count() > 1)
                                            {
                                                int.TryParse(msgData[1], out dungeonID);
                                            }
                                            else if (wolfcoins.classList[whisperSender].groupFinderDungeon != -1)
                                            {
                                                dungeonID = wolfcoins.classList[whisperSender].groupFinderDungeon;
                                                wolfcoins.classList[whisperSender].groupFinderDungeon = -1;
                                            }
                                            //else if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                            //{
                                            //    dungeonID = wolfcoins.classList[whisperSender].queueDungeons.ElementAt(0);
                                            //}
                                            else
                                            {
                                                group.Whisper(whisperSender, "Invalid Dungeon ID provided.");
                                                continue;
                                            }
                                            if (dungeonList.Count() >= dungeonID && dungeonID > 0)
                                            {
                                                string dungeonPath = "content/dungeons/" + dungeonList[dungeonID];
                                                IEnumerable<string> fileText = System.IO.File.ReadLines(dungeonPath, UTF8Encoding.Default);
                                                string[] type = fileText.ElementAt(0).Split('=');
                                                if (type[1] == "Dungeon" && parties[partyID].NumMembers() > 3)
                                                {
                                                    group.Whisper(whisperSender, "You can't have more than 3 party members for a Dungeon.");
                                                    continue;
                                                }

                                                if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                                {
                                                    foreach (var member in parties[partyID].members)
                                                    {
                                                        wolfcoins.classList[member.name].queueDungeons = new List<int>();
                                                    }
                                                }

                                                Dungeon newDungeon = new Dungeon(dungeonPath, channel, itemDatabase);
                                                bool outOfLevelRange = false;
                                                foreach (var member in parties[partyID].members)
                                                {
                                                    member.level = wolfcoins.DetermineLevel(member.name);
                                                    //if (member.level < newDungeon.minLevel)
                                                    //{
                                                    //    group.Whisper(parties[partyID], member.name + " is not high enough level for the requested dungeon. (Min Level: " + newDungeon.minLevel + ")");
                                                    //    outOfLevelRange = true;
                                                    //}
                                                }
                                                int minLevel = 3;
                                                List<string> brokeBitches = new List<string>();
                                                bool enoughMoney = true;
                                                foreach (var member in parties[partyID].members)
                                                {

                                                    if (wolfcoins.Exists(wolfcoins.coinList, member.name))
                                                    {
                                                        if (wolfcoins.coinList[member.name] < (baseDungeonCost + ((member.level - minLevel) * 10)))
                                                        {
                                                            brokeBitches.Add(member.name);
                                                            enoughMoney = false;
                                                        }
                                                    }
                                                }

                                                if (!enoughMoney)
                                                {
                                                    string names = "";
                                                    foreach (var bitch in brokeBitches)
                                                    {
                                                        names += bitch + " ";
                                                    }
                                                    group.WhisperParty(parties[partyID], "The following party members do not have enough money to run " + newDungeon.dungeonName + ": " + names);
                                                }

                                                if (!outOfLevelRange && enoughMoney)
                                                {
                                                    foreach (var member in parties[partyID].members)
                                                    {
                                                        wolfcoins.coinList[member.name] -= (baseDungeonCost + ((member.level - minLevel) * 10));
                                                    }
                                                    group.WhisperParty(parties[partyID], "Successfully initiated " + newDungeon.dungeonName + "! Wolfcoins deducted.");
                                                    string memberInfo = "";
                                                    foreach (var member in parties[partyID].members)
                                                    {
                                                        memberInfo += member.name + " (Level " + member.level + " " + member.className + ") ";
                                                    }

                                                    group.WhisperParty(parties[partyID], "Your party consists of: " + memberInfo);
                                                    parties[partyID].status = GroupModule.PARTY_STARTED;
                                                    parties[partyID].myDungeon = newDungeon;
                                                    parties[partyID] = parties[partyID].myDungeon.RunDungeon(parties[partyID], ref group);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "y")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].pendingPetRelease)
                                    {
                                        int toRelease = wolfcoins.classList[whisperSender].toRelease.stableID;
                                        if (toRelease > wolfcoins.classList[whisperSender].myPets.Count)
                                        {
                                            group.Whisper(whisperSender, "Stable ID mismatch. Try !release again.");
                                            continue;
                                        }
                                        string petName = wolfcoins.classList[whisperSender].myPets.ElementAt(toRelease - 1).name;
                                        if (wolfcoins.classList[whisperSender].ReleasePet(toRelease))
                                        {
                                            group.Whisper(whisperSender, "You released " + petName + ". Goodbye, " + petName + "!");
                                            wolfcoins.SaveClassData();
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Something went wrong. " + petName + " is still with you!");
                                        }
                                        wolfcoins.classList[whisperSender].pendingPetRelease = false;
                                        wolfcoins.classList[whisperSender].toRelease = new Pet();
                                    }

                                    if (wolfcoins.classList[whisperSender].pendingInvite)
                                    {
                                        int partyID = wolfcoins.classList[whisperSender].groupID;
                                        wolfcoins.classList[whisperSender].pendingInvite = false;
                                        string partyLeader = parties[partyID].partyLeader;
                                        int partySize = parties[partyID].NumMembers();
                                        string myClass = wolfcoins.classList[whisperSender].className;
                                        int myLevel = wolfcoins.DetermineLevel(wolfcoins.xpList[whisperSender]);
                                        string myMembers = "";
                                        foreach (var member in parties[partyID].members)
                                        {
                                            myMembers += member.name + " ";
                                        }
                                        group.Whisper(whisperSender, "You successfully joined a party with the following members: " + myMembers);
                                        foreach (var member in parties[partyID].members)
                                        {
                                            if (member.name == whisperSender)
                                                continue;

                                            if (member.pendingInvite)
                                                continue;

                                            group.Whisper(member.name, whisperSender + ", Level " + myLevel + " " + myClass + " has joined your party! (" + partySize + "/" + GroupModule.DUNGEON_MAX + ")");
                                        }

                                        if (partySize == GroupModule.DUNGEON_MAX)
                                        {
                                            group.Whisper(partyLeader, "Your party is now full.");
                                            parties[partyID].status = GroupModule.PARTY_FULL;
                                        }

                                        if (partySize == 3)
                                        {
                                            group.Whisper(partyLeader, "You've reached 3 party members! You're ready to dungeon!");
                                            parties[partyID].status = GroupModule.PARTY_READY;
                                        }
                                        Console.WriteLine(DateTime.Now.ToString() + ": " + whisperSender + " added to Group " + partyID);
                                        string temp = "Updated Member List: ";
                                        foreach (var member in parties[partyID].members)
                                        {
                                            temp += member.name + " ";
                                        }
                                        Console.WriteLine(DateTime.Now.ToString() + ": " + temp);
                                    }
                                }
                            }
                            else if (whisperMessage == "!unready")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    CharClass myClass = wolfcoins.classList[whisperSender];
                                    if (myClass.isPartyLeader)
                                    {
                                        if (parties[myClass.groupID].status == GroupModule.PARTY_READY && parties[myClass.groupID].members.Count <= GroupModule.DUNGEON_MAX)
                                        {
                                            parties[myClass.groupID].status = GroupModule.PARTY_FORMING;
                                            group.WhisperParty(parties[myClass.groupID], "Party 'Ready' status has been revoked.");
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!ready")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    CharClass myClass = wolfcoins.classList[whisperSender];
                                    if (myClass.isPartyLeader)
                                    {
                                        if (parties.ContainsKey(myClass.groupID) && parties[myClass.groupID].status == GroupModule.PARTY_FORMING)
                                        {
                                            parties[myClass.groupID].status = GroupModule.PARTY_READY;
                                            group.WhisperParty(parties[myClass.groupID], "Party set to 'Ready'. Be careful adventuring without a full party!");
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "n")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].pendingPetRelease)
                                    {
                                        string petName = wolfcoins.classList[whisperSender].toRelease.name;
                                        wolfcoins.classList[whisperSender].pendingPetRelease = false;
                                        wolfcoins.classList[whisperSender].toRelease = new Pet();

                                        group.Whisper(whisperSender, "You decided to keep " + petName + ".");
                                    }

                                    if (wolfcoins.classList[whisperSender].pendingInvite)
                                    {
                                        wolfcoins.classList[whisperSender].pendingInvite = false;
                                        string partyLeader = parties[wolfcoins.classList[whisperSender].groupID].partyLeader;
                                        parties[wolfcoins.classList[whisperSender].groupID].RemoveMember(whisperSender);
                                        wolfcoins.classList[whisperSender].groupID = -1;
                                        group.Whisper(whisperSender, "You declined " + partyLeader + "'s invite.");
                                        group.Whisper(partyLeader, whisperSender + " has declined your party invite.");
                                        wolfcoins.classList[partyLeader].numInvitesSent--;
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!kick"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.classList[whisperSender].isPartyLeader)
                                {
                                    if (wolfcoins.classList[whisperSender].groupID != -1)
                                    {
                                        if (parties[wolfcoins.classList[whisperSender].groupID].status == GroupModule.PARTY_STARTED)
                                        {
                                            group.Whisper(whisperSender, "You can't kick a party member in the middle of a dungoen!");
                                            continue;
                                        }
                                        if (whispers[1] != null)
                                        {
                                            string[] msgData = whispers[1].Split(' ');

                                            if (msgData.Count() > 1)
                                            {
                                                string toKick = msgData[1];
                                                if (whisperSender == toKick)
                                                {
                                                    group.Whisper(whisperSender, "You can't kick yourself from a group! Do !leaveparty instead.");
                                                    continue;
                                                }
                                                toKick = toKick.ToLower();
                                                if (wolfcoins.classList.Keys.Contains(toKick.ToLower()))
                                                {
                                                    if (wolfcoins.classList[whisperSender].isPartyLeader)
                                                    {
                                                        int myID = wolfcoins.classList[whisperSender].groupID;
                                                        for (int i = 0; i < parties[myID].members.Count(); i++)
                                                        {
                                                            if (parties[myID].members.ElementAt(i).name == toKick)
                                                            {
                                                                parties[myID].RemoveMember(toKick);
                                                                wolfcoins.classList[toKick].groupID = -1;
                                                                wolfcoins.classList[toKick].pendingInvite = false;
                                                                wolfcoins.classList[toKick].numInvitesSent = 0;
                                                                wolfcoins.classList[whisperSender].numInvitesSent--;
                                                                group.Whisper(toKick, "You were removed from " + whisperSender + "'s party.");
                                                                group.WhisperParty(parties[myID], toKick + " was removed from the party.");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        group.Whisper(whisperSender, "You are not the party leader.");
                                                    }
                                                }
                                                else
                                                {
                                                    group.Whisper(whisperSender, "Couldn't find that party member to remove.");
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            else if (whisperMessage.StartsWith("!add"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.classList[whisperSender].isPartyLeader)
                                {
                                    if (wolfcoins.classList[whisperSender].groupID != -1)
                                    {
                                        if (parties[wolfcoins.classList[whisperSender].groupID].status == GroupModule.PARTY_STARTED)
                                            continue;

                                        if (wolfcoins.classList[whisperSender].usedGroupFinder && parties[wolfcoins.classList[whisperSender].groupID].NumMembers() == 3)
                                        {
                                            group.Whisper(whisperSender, "You can't have more than 3 party members for a Group Finder dungeon.");
                                            continue;
                                        }

                                        if (whispers[1] != null)
                                        {
                                            string[] msgData = whispers[1].Split(' ');

                                            if (msgData.Count() > 1)
                                            {
                                                string invitee = msgData[1];
                                                if (whisperSender == invitee)
                                                {
                                                    group.Whisper(whisperSender, "You can't invite yourself to a group!");
                                                    continue;
                                                }
                                                if (wolfcoins.Exists(wolfcoins.classList, invitee) && wolfcoins.classList[invitee].queueDungeons.Count > 0)
                                                {
                                                    group.Whisper(whisperSender, invitee + " is currently queued for Group Finder and cannot be added to the group.");
                                                    group.Whisper(invitee, whisperSender + " tried to invite you to a group, but you are queued in the Group Finder. Type '!leavequeue' to leave the queue.");
                                                    continue;
                                                }

                                                invitee = invitee.ToLower();
                                                if (wolfcoins.classList.Keys.Contains(invitee.ToLower()))
                                                {
                                                    int myID = wolfcoins.classList[whisperSender].groupID;
                                                    if (wolfcoins.classList[invitee].classType != -1 && wolfcoins.classList[invitee].groupID == -1
                                                        && !wolfcoins.classList[invitee].pendingInvite && wolfcoins.classList[whisperSender].numInvitesSent < GroupModule.DUNGEON_MAX
                                                        && parties.ContainsKey(myID))
                                                    {
                                                        if (parties[myID].status != GroupModule.PARTY_FORMING && parties[myID].status != GroupModule.PARTY_READY)
                                                        {
                                                            continue;
                                                        }

                                                        wolfcoins.classList[whisperSender].numInvitesSent++;
                                                        wolfcoins.classList[invitee].pendingInvite = true;
                                                        wolfcoins.classList[invitee].groupID = myID;
                                                        wolfcoins.classList[invitee].ClearQueue();
                                                        string myClass = wolfcoins.classList[whisperSender].className;
                                                        int myLevel = wolfcoins.classList[whisperSender].level;
                                                        parties[myID].AddMember(wolfcoins.classList[invitee]);
                                                        string msg = whisperSender + ", Level " + myLevel + " " + myClass + ", has invited you to join a party. Accept? (y/n)";
                                                        group.Whisper(whisperSender, "You invited " + invitee + " to a group.");
                                                        group.Whisper(invitee, msg);
                                                    }
                                                    else if (wolfcoins.classList[whisperSender].numInvitesSent >= GroupModule.DUNGEON_MAX)
                                                    {
                                                        group.Whisper(whisperSender, "You have the max number of invites already pending.");
                                                    }
                                                    else if (wolfcoins.classList[invitee].groupID != -1)
                                                    {
                                                        group.Whisper(whisperSender, invitee + " is already in a group.");
                                                        group.Whisper(invitee, whisperSender + " tried to invite you to a group, but you are already in one! Type '!leaveparty' to abandon your current group.");
                                                    }
                                                }
                                                else
                                                {
                                                    if (wolfcoins.Exists(wolfcoins.xpList, invitee))
                                                    {
                                                        int level = wolfcoins.DetermineLevel(invitee);
                                                        if (level < 3)
                                                        {
                                                            //group.Whisper(whisperSender, invitee + " is not high enough level. (" + level + ")");
                                                        }
                                                        else
                                                        {
                                                            group.Whisper(whisperSender, invitee + " is high enough level, but has not picked a class!");
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!promote"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    int partyID = wolfcoins.classList[whisperSender].groupID;
                                    if (partyID == -1)
                                    {
                                        continue;
                                    }

                                    if (!wolfcoins.classList[whisperSender].isPartyLeader)
                                    {
                                        group.Whisper(whisperSender, "You must be the party leader to promote.");
                                        continue;
                                    }

                                    if (whispers[1] != null)
                                    {
                                        string[] msgData = whispers[1].Split(' ');

                                        if (msgData.Count() > 1 && msgData.Count() <= 3)
                                        {
                                            string newLeader = msgData[1].ToLower();
                                            bool newLeaderCreated = false;

                                            foreach (var member in parties[partyID].members)
                                            {
                                                if (newLeaderCreated)
                                                    continue;

                                                if (member.name == whisperSender)
                                                    member.isPartyLeader = false;

                                                if (member.name == newLeader)
                                                {

                                                    wolfcoins.classList[whisperSender].isPartyLeader = false;
                                                    parties[partyID].partyLeader = newLeader;
                                                    wolfcoins.classList[newLeader].isPartyLeader = true;
                                                    member.isPartyLeader = true;

                                                    newLeaderCreated = true;
                                                }
                                            }

                                            if (newLeaderCreated)
                                            {
                                                foreach (var member in parties[partyID].members)
                                                {
                                                    if (member.name != newLeader && member.name != whisperSender)
                                                    {
                                                        group.Whisper(member.name, whisperSender + " has promoted " + newLeader + " to Party Leader.");
                                                    }
                                                }
                                                group.Whisper(newLeader, whisperSender + " has promoted you to Party Leader.");
                                                group.Whisper(whisperSender, "You have promoted " + newLeader + " to Party Leader.");
                                            }
                                            else
                                            {
                                                group.Whisper(whisperSender, "Party member '" + newLeader + "' not found. You are still party leader.");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!leaveparty")
                            {
                                if (parties.Count() > 0 && wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    int myID = wolfcoins.classList[whisperSender].groupID;
                                    if (myID != -1 && parties[myID].status == GroupModule.PARTY_STARTED)
                                    {
                                        group.Whisper(whisperSender, "You can't leave your party while a dungeon is in progress!");
                                        continue;
                                    }
                                    if (myID != -1 && !wolfcoins.classList[whisperSender].pendingInvite)
                                    {
                                        if (parties.Count() > 0 && parties.ContainsKey(myID))
                                        {
                                            if (wolfcoins.classList[whisperSender].isPartyLeader)
                                            {
                                                wolfcoins.classList[whisperSender].groupID = -1;
                                                wolfcoins.classList[whisperSender].numInvitesSent = 0;
                                                wolfcoins.classList[whisperSender].isPartyLeader = false;
                                                wolfcoins.classList[whisperSender].ClearQueue();

                                                parties[myID].RemoveMember(whisperSender);
                                                Console.WriteLine("Party Leader " + whisperSender + " left group #" + myID);
                                                string myMembers = "";
                                                foreach (var member in parties[myID].members)
                                                {
                                                    myMembers += member.name + " ";
                                                }
                                                Console.WriteLine(DateTime.Now.ToString() + ": Remaining members: " + myMembers);
                                                group.WhisperParty(parties[myID], "The party leader (" + whisperSender + ") has left. Your party has been disbanded.");
                                                for (int i = 0; i < parties[myID].members.Count(); i++)
                                                {
                                                    string dude = parties[myID].members.ElementAt(i).name;
                                                    wolfcoins.classList[dude].groupID = -1;
                                                    wolfcoins.classList[dude].pendingInvite = false;
                                                    wolfcoins.classList[dude].numInvitesSent = 0;
                                                    wolfcoins.classList[dude].ClearQueue();

                                                }
                                                parties.Remove(myID);
                                                group.Whisper(whisperSender, "Your party has been disbanded.");

                                            }
                                            else if (parties.ContainsKey(myID) && (parties[myID].RemoveMember(whisperSender)))
                                            {
                                                if (parties[myID].status == GroupModule.PARTY_FORMING)
                                                {
                                                    string partyleader = parties[myID].partyLeader;
                                                    wolfcoins.classList[partyleader].numInvitesSent--;
                                                }
                                                else if (parties[myID].status == GroupModule.PARTY_FULL)
                                                {
                                                    string partyleader = parties[myID].partyLeader;
                                                    parties[myID].status = GroupModule.PARTY_FORMING;
                                                    wolfcoins.classList[partyleader].numInvitesSent--;
                                                }

                                                group.WhisperParty(parties[myID], whisperSender + " has left the party.");
                                                group.Whisper(whisperSender, "You left the party.");
                                                wolfcoins.classList[whisperSender].groupID = -1;
                                                wolfcoins.classList[whisperSender].ClearQueue();
                                                Console.WriteLine(DateTime.Now.ToString() + ": " + whisperSender + " left group with ID " + myID);
                                                string myMembers = "";
                                                foreach (var member in parties[myID].members)
                                                {
                                                    myMembers += member.name + " ";
                                                }
                                                Console.WriteLine(DateTime.Now.ToString() + ": Remaining Members: " + myMembers);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!daily")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    double minutes = (DateTime.Now - wolfcoins.classList[whisperSender].lastDailyGroupFinder).TotalMinutes;
                                    double totalHours = (DateTime.Now - wolfcoins.classList[whisperSender].lastDailyGroupFinder).TotalHours;
                                    double totalDays = (DateTime.Now - wolfcoins.classList[whisperSender].lastDailyGroupFinder).TotalDays;
                                    if (totalDays >= 1)
                                    {
                                        group.Whisper(whisperSender, "You are eligible for daily Group Finder rewards! Go queue up!");
                                        continue;
                                    }
                                    else
                                    {
                                        double minutesLeft = Math.Truncate(60 - (minutes % 60));
                                        double hoursLeft = Math.Truncate(24 - (totalHours));
                                        group.Whisper(whisperSender, "Your daily Group Finder reward resets in " + hoursLeft + " hours and " + minutesLeft + " minutes.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!queue"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.classList[whisperSender].name != "NAMELESS ONE")
                                {
                                    if (whisperMessage == "!queuetime")
                                    {
                                        if (wolfcoins.classList[whisperSender].queueDungeons.Count == 0)
                                            continue;

                                        string myQueuedDungeons = "You are queued for the following dungeons: ";
                                        bool firstAdded = false;
                                        foreach (var dung in wolfcoins.classList[whisperSender].queueDungeons)
                                        {
                                            if (!firstAdded)
                                            {
                                                firstAdded = true;
                                            }
                                            else
                                            {
                                                myQueuedDungeons += ",";
                                            }
                                            myQueuedDungeons += dung;
                                        }

                                        double timeSpan = ((DateTime.Now - wolfcoins.classList[whisperSender].queueTime)).TotalSeconds;
                                        double seconds = timeSpan % 60;
                                        seconds = Math.Truncate(seconds);
                                        double minutes = ((DateTime.Now - wolfcoins.classList[whisperSender].queueTime)).TotalMinutes % 60;
                                        minutes = Math.Truncate(minutes);
                                        if (minutes >= 60)
                                            minutes = minutes % 60;

                                        double hours = ((DateTime.Now - wolfcoins.classList[whisperSender].queueTime)).TotalHours;
                                        string timeMessage = "You've been waiting in the Group Finder queue for ";
                                        string lastFormed = "The last group was formed ";
                                        hours = Math.Truncate(hours);
                                        if (hours > 0)
                                            timeMessage += hours + " hours, ";

                                        if (minutes > 0)
                                            timeMessage += minutes + " minutes, and ";

                                        timeMessage += seconds + " seconds.";

                                        timeSpan = ((DateTime.Now - groupFinder.lastFormed).TotalSeconds);
                                        seconds = timeSpan % 60;
                                        seconds = Math.Truncate(seconds);
                                        minutes = timeSpan / 60;
                                        minutes = Math.Truncate(minutes);
                                        hours = minutes / 60;
                                        hours = Math.Truncate(hours);

                                        if (minutes >= 60)
                                            minutes = minutes % 60;

                                        if (hours > 0)
                                            lastFormed += hours + " hours, ";

                                        if (minutes > 0)
                                            lastFormed += minutes + " minutes, and ";

                                        lastFormed += seconds + " seconds ago.";

                                        group.Whisper(whisperSender, myQueuedDungeons);
                                        group.Whisper(whisperSender, timeMessage);
                                        group.Whisper(whisperSender, lastFormed);
                                        continue;
                                    }
                                    if (whisperMessage == "!queuestatus" && whisperSender == "lobosjr")
                                    {
                                        if (groupFinder.queue.Count == 0)
                                        {
                                            group.Whisper(whisperSender, "No players in queue.");
                                        }

                                        group.Whisper(whisperSender, groupFinder.queue.Count + " players in queue.");
                                        Dictionary<int, int> queueData = new Dictionary<int, int>();
                                        foreach (var player in groupFinder.queue)
                                        {
                                            foreach (var dungeonID in player.queueDungeons)
                                            {
                                                if (!queueData.ContainsKey(dungeonID))
                                                {
                                                    queueData.Add(dungeonID, 1);
                                                }
                                                else
                                                {
                                                    queueData[dungeonID]++;
                                                }
                                            }
                                        }

                                        foreach (var dataPoint in queueData)
                                        {
                                            group.Whisper(whisperSender, "Dungeon ID <" + dataPoint.Key + ">: " + dataPoint.Value + " players");
                                        }

                                        continue;
                                    }

                                    if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                    {
                                        group.Whisper(whisperSender, "You are already queued in the Group Finder! Type !queuetime for more information.");
                                        continue;
                                    }

                                    if (!wolfcoins.classList[whisperSender].pendingInvite && wolfcoins.classList[whisperSender].groupID == -1)
                                    {

                                        string[] msgData = whispers[1].Split(' ');
                                        string[] tempDungeonData;
                                        bool didRequest = false;
                                        if (msgData.Count() > 1)
                                        {
                                            tempDungeonData = msgData[1].Split(',');
                                            didRequest = true;
                                        }
                                        else
                                        {
                                            tempDungeonData = DungeonModule.GetEligibleDungeons(whisperSender, wolfcoins, dungeonList).Split(',');
                                        }
                                        List<int> requestedDungeons = new List<int>();
                                        //string[] tempDungeonData = msgData[1].Split(',');
                                        string errorMessage = "Unable to join queue. Reason(s): ";
                                        bool eligible = true;
                                        for (int i = 0; i < tempDungeonData.Count(); i++)
                                        {
                                            int tempInt = -1;
                                            int.TryParse(tempDungeonData[i], out tempInt);
                                            int eligibility = DungeonModule.DetermineEligibility(whisperSender, tempInt, dungeonList, baseDungeonCost, wolfcoins);
                                            switch (eligibility)
                                            {
                                                case 0: // player not high enough level
                                                    {
                                                        eligible = false;
                                                        errorMessage += "Not appropriate level. (ID: " + tempInt + ") ";
                                                    }
                                                    break;

                                                case -1: // invalid dungeon id
                                                    {
                                                        eligible = false;
                                                        errorMessage += "Invalid Dungeon ID provided. (ID: " + tempInt + ") ";
                                                    }
                                                    break;

                                                case -2: // not enough money
                                                    {
                                                        eligible = false;
                                                        errorMessage += "You don't have enough money!";
                                                    }
                                                    break;
                                                case 1:
                                                    {
                                                        if (!didRequest)
                                                        {
                                                            if (i < 9)
                                                                requestedDungeons.Add(i);
                                                        }

                                                    }
                                                    break;

                                                default: break;
                                            }

                                            if (eligibility == -2)
                                                break;

                                            //if (tempInt != -1)
                                            //    requestedDungeons.Add(tempInt);

                                        }

                                        if (!eligible)
                                        {
                                            group.Whisper(whisperSender, errorMessage);
                                            continue;
                                        }

                                        wolfcoins.classList[whisperSender].queuePriority = groupFinder.priority;
                                        groupFinder.priority++;
                                        wolfcoins.classList[whisperSender].usedGroupFinder = true;
                                        wolfcoins.classList[whisperSender].queueDungeons = requestedDungeons;

                                        Party myParty = groupFinder.Add(wolfcoins.classList[whisperSender]);
                                        if (myParty.members.Count != 3)
                                        {
                                            wolfcoins.classList[whisperSender].queueTime = DateTime.Now;
                                            group.Whisper(whisperSender, "You have been placed in the Group Finder queue.");
                                            continue;
                                        }

                                        myParty.members.ElementAt(0).isPartyLeader = true;
                                        myParty.partyLeader = myParty.members.ElementAt(0).name;
                                        myParty.status = GroupModule.PARTY_FULL;
                                        myParty.members.ElementAt(0).numInvitesSent = 3;
                                        myParty.myID = maxPartyID;
                                        myParty.usedDungeonFinder = true;

                                        Random RNG = new Random();
                                        int availableDungeons = myParty.members.ElementAt(0).queueDungeons.Count();
                                        //choose a random dungeon out of the available options
                                        int randDungeon = RNG.Next(0, (availableDungeons - 1));
                                        // set the id based on that random dungeon
                                        int dungeonID = myParty.members.ElementAt(0).queueDungeons.ElementAt(randDungeon);
                                        dungeonID++;
                                        myParty.members.ElementAt(0).groupFinderDungeon = dungeonID;
                                        string dungeonName = DungeonModule.GetDungeonName(dungeonID, dungeonList);
                                        string members = "Group Finder group created for " + dungeonName + ": ";
                                        foreach (var member in myParty.members)
                                        {
                                            member.groupID = maxPartyID;
                                            member.usedGroupFinder = true;
                                            members += member.name + ", " + member.className + "; ";
                                            string otherMembers = "";
                                            foreach (var player in myParty.members)
                                            {
                                                if (player.name == member.name)
                                                    continue;

                                                otherMembers += player.name + " (" + player.className + ") ";
                                            }

                                            group.Whisper(member.name, "You've been matched for " + dungeonName + " with: " + otherMembers + ".");
                                            if (member.isPartyLeader)
                                                group.Whisper(member.name, "You are the party leader. Whisper me '!start' to begin!");
                                        }
                                        parties.Add(maxPartyID, myParty);
                                        Console.WriteLine(DateTime.Now.ToString() + ": " + members);
                                        maxPartyID++;

                                    }
                                    else if (wolfcoins.classList[whisperSender].isPartyLeader)
                                    {
                                        string reason = "";
                                        if (parties.ContainsKey(wolfcoins.classList[whisperSender].groupID))
                                        {
                                            switch (parties[wolfcoins.classList[whisperSender].groupID].status)
                                            {
                                                case GroupModule.PARTY_FORMING:
                                                    {
                                                        reason = "Party is currently forming. Add members with '!add <username>'";
                                                    }
                                                    break;

                                                case GroupModule.PARTY_READY:
                                                    {
                                                        reason = "Party is filled and ready to adventure! Type '!start' to begin!";
                                                    }
                                                    break;

                                                case GroupModule.PARTY_STARTED:
                                                    {
                                                        reason = "Your party is currently on an adventure!";
                                                    }
                                                    break;

                                                case GroupModule.PARTY_COMPLETE:
                                                    {
                                                        reason = "Your party just finished an adventure!";
                                                    }
                                                    break;

                                                default:
                                                    {
                                                        reason = "I have no idea the status of your party.";
                                                    }
                                                    break;
                                            }
                                        }
                                        group.Whisper(whisperSender, "You already have a party created! " + reason);
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You currently have an outstanding invite to another party. Couldn't create new party!");
                                    }
                                }
                            }
                            else if (whisperMessage == "!classes")
                            {
                                if (wolfcoins.classList != null)
                                {
                                    double numClasses = 0;
                                    double numWarriors = 0;
                                    double numMages = 0;
                                    double numRogues = 0;
                                    double numRangers = 0;
                                    double numClerics = 0;
                                    foreach (var member in wolfcoins.classList)
                                    {
                                        numClasses++;
                                        switch (member.Value.classType)
                                        {
                                            case CharClass.WARRIOR:
                                                {
                                                    numWarriors++;
                                                }
                                                break;

                                            case CharClass.MAGE:
                                                {
                                                    numMages++;
                                                }
                                                break;

                                            case CharClass.ROGUE:
                                                {
                                                    numRogues++;
                                                }
                                                break;

                                            case CharClass.RANGER:
                                                {
                                                    numRangers++;
                                                }
                                                break;

                                            case CharClass.CLERIC:
                                                {
                                                    numClerics++;
                                                }
                                                break;

                                            default: break;
                                        }


                                    }

                                    double percentWarriors = (numWarriors / numClasses) * 100;
                                    percentWarriors = Math.Round(percentWarriors, 1);
                                    double percentMages = (numMages / numClasses) * 100;
                                    percentMages = Math.Round(percentMages, 1);
                                    double percentRogues = (numRogues / numClasses) * 100;
                                    percentRogues = Math.Round(percentRogues, 1);
                                    double percentRangers = (numRangers / numClasses) * 100;
                                    percentRangers = Math.Round(percentRangers, 1);
                                    double percentClerics = (numClerics / numClasses) * 100;
                                    percentClerics = Math.Round(percentClerics, 1);

                                    group.Whisper(whisperSender, "Class distribution for the Wolfpack RPG: ");
                                    group.Whisper(whisperSender, "Warriors: " + percentWarriors + "%");
                                    group.Whisper(whisperSender, "Mages: " + percentMages + "%");
                                    group.Whisper(whisperSender, "Rogues: " + percentRogues + "%");
                                    group.Whisper(whisperSender, "Rangers: " + percentRangers + "%");
                                    group.Whisper(whisperSender, "Clerics " + percentClerics + "%");
                                }
                            }
                            else if (whisperMessage == "!leavequeue")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                    {
                                        groupFinder.RemoveMember(whisperSender);
                                        wolfcoins.classList[whisperSender].ClearQueue();

                                        group.Whisper(whisperSender, "You were removed from the Group Finder.");
                                    }
                                }
                            }
                            else if (whisperMessage == "!createparty")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (!wolfcoins.classList[whisperSender].pendingInvite && wolfcoins.classList[whisperSender].groupID == -1)
                                    {
                                        if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                        {
                                            group.Whisper(whisperSender, "Can't create a party while queued with the Group Finder. Message me '!leavequeue' to exit.");
                                            continue;
                                        }
                                        wolfcoins.classList[whisperSender].isPartyLeader = true;
                                        wolfcoins.classList[whisperSender].numInvitesSent = 1;
                                        Party myParty = new Party
                                        {
                                            status = GroupModule.PARTY_FORMING,
                                            partyLeader = whisperSender
                                        };
                                        int myLevel = wolfcoins.DetermineLevel(whisperSender);
                                        wolfcoins.classList[whisperSender].groupID = maxPartyID;
                                        myParty.AddMember(wolfcoins.classList[whisperSender]);
                                        myParty.myID = maxPartyID;
                                        parties.Add(maxPartyID, myParty);

                                        group.Whisper(whisperSender, "Party created! Use '!add <username>' to invite party members.");
                                        Console.WriteLine(DateTime.Now.ToString() + ": Party created: ");
                                        Console.WriteLine(DateTime.Now.ToString() + ": ID: " + maxPartyID);
                                        Console.WriteLine(DateTime.Now.ToString() + ": Total number of parties: " + parties.Count());
                                        maxPartyID++;
                                    }
                                    else if (wolfcoins.classList[whisperSender].isPartyLeader)
                                    {
                                        string reason = "";
                                        if (parties.ContainsKey(wolfcoins.classList[whisperSender].groupID))
                                        {
                                            switch (parties[wolfcoins.classList[whisperSender].groupID].status)
                                            {
                                                case GroupModule.PARTY_FORMING:
                                                    {
                                                        reason = "Party is currently forming. Add members with '!add <username>'";
                                                    }
                                                    break;

                                                case GroupModule.PARTY_READY:
                                                    {
                                                        reason = "Party is filled and ready to adventure! Type '!start' to begin!";
                                                    }
                                                    break;

                                                case GroupModule.PARTY_STARTED:
                                                    {
                                                        reason = "Your party is currently on an adventure!";
                                                    }
                                                    break;

                                                case GroupModule.PARTY_COMPLETE:
                                                    {
                                                        reason = "Your party just finished an adventure!";
                                                    }
                                                    break;

                                                default:
                                                    {
                                                        reason = "I have no idea the status of your party.";
                                                    }
                                                    break;
                                            }
                                        }
                                        group.Whisper(whisperSender, "You already have a party created! " + reason);
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You currently have an outstanding invite to another party. Couldn't create new party!");
                                    }
                                }
                            }

                            else if (whisperMessage.Equals("nevermind", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].classType >= 10)
                                    {

                                        int oldClass = wolfcoins.classList[whisperSender].classType / 10;
                                        wolfcoins.classList[whisperSender].classType = oldClass;
                                        group.Whisper(whisperSender, "Respec cancelled. No Wolfcoins deducted from your balance.");
                                    }

                                }
                            }

                            else if (whisperMessage == "!partydata")
                            {
                                if (parties.Count() > 0)
                                {
                                    if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                    {
                                        int myID = wolfcoins.classList[whisperSender].groupID;
                                        if (myID != -1)
                                        {
                                            string partyMembers = "";
                                            if (parties.ContainsKey(myID))
                                            {
                                                for (int i = 0; i < parties[myID].members.Count(); i++)
                                                {
                                                    partyMembers += parties[myID].members.ElementAt(i).name + " ";
                                                }
                                                string status = "";
                                                switch (parties[myID].status)
                                                {
                                                    case GroupModule.PARTY_FORMING:
                                                        {
                                                            status = "GroupModule.PARTY_FORMING";
                                                        }
                                                        break;

                                                    case GroupModule.PARTY_READY:
                                                        {
                                                            status = "GroupModule.PARTY_READY";
                                                        }
                                                        break;

                                                    case GroupModule.PARTY_STARTED:
                                                        {
                                                            status = "GroupModule.PARTY_STARTED";
                                                        }
                                                        break;

                                                    case GroupModule.PARTY_COMPLETE:
                                                        {
                                                            status = "GroupModule.PARTY_COMPLETE";
                                                        }
                                                        break;

                                                    default:
                                                        {
                                                            status = "UNKNOWN_STATUS";
                                                        }
                                                        break;
                                                }
                                                irc.SendChatMessage(whisperSender + " requested his Party Data. Group ID: " + wolfcoins.classList[whisperSender].groupID + "; Members: " + partyMembers + "; Status: " + status);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!clearitems"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {
                                    string target = whisperMSG[1];
                                    if (wolfcoins.Exists(wolfcoins.classList, target))
                                    {
                                        wolfcoins.classList[target].totalItemCount = 0;
                                        wolfcoins.classList[target].myItems = new List<Item>();
                                        wolfcoins.SaveClassData();
                                        group.Whisper(whisperSender, "Cleared " + target + "'s item list.");
                                    }
                                }
                            }
                            else if (whisperMessage == "!fixstats")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                if (wolfcoins.classList != null)
                                {
                                    foreach (var user in wolfcoins.classList)
                                    {
                                        if (user.Value.name == "NAMELESS ONE")
                                            continue;

                                        int classType = wolfcoins.classList[user.Value.name].classType;
                                        CharClass defaultClass;
                                        switch (classType)
                                        {
                                            case CharClass.WARRIOR:
                                                {
                                                    defaultClass = new Warrior();
                                                }
                                                break;

                                            case CharClass.MAGE:
                                                {
                                                    defaultClass = new Mage();
                                                }
                                                break;

                                            case CharClass.ROGUE:
                                                {
                                                    defaultClass = new Rogue();
                                                }
                                                break;

                                            case CharClass.RANGER:
                                                {
                                                    defaultClass = new Ranger();
                                                }
                                                break;

                                            case CharClass.CLERIC:
                                                {
                                                    defaultClass = new Cleric();
                                                }
                                                break;

                                            default:
                                                {
                                                    defaultClass = new CharClass();
                                                }
                                                break;
                                        }

                                        wolfcoins.classList[user.Value.name].coinBonus = defaultClass.coinBonus;
                                        wolfcoins.classList[user.Value.name].xpBonus = defaultClass.xpBonus;
                                        wolfcoins.classList[user.Value.name].itemFind = defaultClass.itemFind;
                                        wolfcoins.classList[user.Value.name].successChance = defaultClass.successChance;
                                        wolfcoins.classList[user.Value.name].preventDeathBonus = defaultClass.preventDeathBonus;
                                        wolfcoins.classList[user.Value.name].itemEarned = -1;
                                        wolfcoins.classList[user.Value.name].ClearQueue();

                                    }
                                    wolfcoins.SaveClassData();
                                    group.Whisper(whisperSender, "Reset all user's stats to default.");
                                }

                            }
                            else if (whisperMessage.StartsWith("!giveitem"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    string temp = whisperMSG[2];
                                    int id = -1;
                                    int.TryParse(temp, out id);
                                    if (id < 1 || id > itemDatabase.Count())
                                    {
                                        group.Whisper(whisperSender, "Invalid ID was attempted to be given.");
                                    }

                                    string user = whisperMSG[1];
                                    bool hasItem = false;
                                    if (wolfcoins.Exists(wolfcoins.classList, user))
                                    {
                                        if (wolfcoins.classList[user].myItems.Count > 0)
                                        {

                                            foreach (var item in wolfcoins.classList[user].myItems)
                                            {
                                                if (item.itemID == id)
                                                {
                                                    hasItem = true;
                                                    group.Whisper(whisperSender, user + " already has " + itemDatabase[id - 1].itemName + ".");
                                                }
                                            }

                                        }
                                        if (!hasItem && itemDatabase.ContainsKey(id))
                                        {
                                            ItemModule.GrantItem(id, wolfcoins, user, itemDatabase);
                                            wolfcoins.SaveClassData();
                                            //if(wolfcoins.classList[user].totalItemCount != -1)
                                            //{
                                            //    wolfcoins.classList[user].totalItemCount++;
                                            //}
                                            //else
                                            //{
                                            //    wolfcoins.classList[user].totalItemCount = 1;
                                            //}
                                            //wolfcoins.classList[user].myItems.Add(itemDatabase[id]);
                                            group.Whisper(whisperSender, "Gave " + user + " a " + itemDatabase[id - 1].itemName + ".");
                                        }

                                    }
                                }
                            }
                            // player requests to equip an item. make sure they actually *have* items
                            // check if their item is equippable ('other' type items are not), and that it isn't already active
                            // set it to true, then iterate through item list. if inventoryID does *not* match and it *IS* active, deactivate it
                            else if (whisperMessage.StartsWith("!activate") || whisperMessage.StartsWith("activate") || whisperMessage.StartsWith("!equip")
                                || whisperMessage.StartsWith("equip"))
                            {
                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {

                                    string temp = whisperMSG[1];
                                    int id = -1;
                                    int.TryParse(temp, out id);
                                    if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                    {
                                        if (wolfcoins.classList[whisperSender].myItems.Count > 0)
                                        {
                                            Item toActivate = wolfcoins.classList[whisperSender].GetItem(id);
                                            int itemPos = wolfcoins.classList[whisperSender].GetItemPos(id);

                                            if (toActivate.itemType == Item.TYPE_ARMOR || toActivate.itemType == Item.TYPE_WEAPON)
                                            {
                                                if (toActivate.isActive)
                                                {
                                                    group.Whisper(whisperSender, toActivate.itemName + " is already equipped.");
                                                    continue;
                                                }
                                                wolfcoins.classList[whisperSender].myItems.ElementAt(itemPos).isActive = true;
                                                foreach (var itm in wolfcoins.classList[whisperSender].myItems)
                                                {
                                                    if (itm.inventoryID == id)
                                                        continue;

                                                    if (itm.itemType != toActivate.itemType)
                                                        continue;

                                                    if (itm.isActive)
                                                    {
                                                        itm.isActive = false;
                                                        group.Whisper(whisperSender, "Unequipped " + itm.itemName + ".");
                                                    }
                                                }
                                                group.Whisper(whisperSender, "Equipped " + toActivate.itemName + ".");
                                            }
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "You have no items.");
                                        }
                                    }
                                }
                            }
                            // player requests to unequip an item. make sure they actually *have* items
                            // check if their item is equippable ('other' type items are not), and that it isn't already inactive
                            else if (whisperMessage.StartsWith("!deactivate") || whisperMessage.StartsWith("deactivate") || whisperMessage.StartsWith("!unequip")
                            || whisperMessage.StartsWith("unequip"))
                            {
                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {

                                    string temp = whisperMSG[1];
                                    int id = -1;
                                    int.TryParse(temp, out id);
                                    if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && id != -1)
                                    {
                                        if (wolfcoins.classList[whisperSender].myItems.Count > 0)
                                        {
                                            Item toDeactivate = wolfcoins.classList[whisperSender].GetItem(id);
                                            int itemPos = wolfcoins.classList[whisperSender].GetItemPos(id);

                                            if (toDeactivate.itemType == Item.TYPE_ARMOR || toDeactivate.itemType == Item.TYPE_WEAPON)
                                            {
                                                if (toDeactivate.isActive)
                                                {
                                                    wolfcoins.classList[whisperSender].myItems.ElementAt(itemPos).isActive = false;
                                                    group.Whisper(whisperSender, "Unequipped " + toDeactivate.itemName + ".");
                                                }

                                            }
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "You have no items.");
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!printinfo"))
                            {
                                if (whisperSender != "lobosjr")
                                    break;
                                // first[1] is the user to print info for
                                if (whispers.Length >= 2 && whispers[1] != null)
                                {
                                    string[] whisperMSG = whispers[1].Split();
                                    string player = whisperMSG[1].ToString();
                                    if (wolfcoins.Exists(wolfcoins.classList, player))
                                    {

                                        // print out all the user's info

                                        int numItems = wolfcoins.classList[player].totalItemCount;

                                        Console.WriteLine("Name: " + player);
                                        Console.WriteLine("Level: " + wolfcoins.classList[player].level);
                                        Console.WriteLine("Prestige Level: " + wolfcoins.classList[player].prestige);
                                        Console.WriteLine("Class: " + wolfcoins.classList[player].className);
                                        Console.WriteLine("Dungeon success chance: " + wolfcoins.classList[player].GetTotalSuccessChance());
                                        Console.WriteLine("Number of Items: " + numItems);
                                        Console.WriteLine(wolfcoins.classList[player].PrintItems());

                                    }
                                    else
                                    {
                                        Console.WriteLine("Player name not found.");
                                    }

                                }
                            }
                            else if (whisperMessage.StartsWith("!setxp"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    if (int.TryParse(whisperMSG[2], out int value))
                                    {
                                        int newXp = wolfcoins.SetXP(value, whisperMSG[1], group);
                                        if (newXp != -1)
                                        {
                                            group.Whisper(whisperSender, "Set " + whisperMSG[1] + "'s XP to " + newXp + ".");
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Error updating XP amount.");
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "Invalid data provided for !setxp command.");
                                    }
                                }
                                else
                                {
                                    group.Whisper(whisperSender, "Not enough data provided for !setxp command.");
                                }
                            }
                            else if (whisperMessage.StartsWith("!setprestige"))
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    int value = -1;
                                    if (int.TryParse(whisperMSG[2], out value))
                                    {

                                        if (value != -1 && wolfcoins.classList.ContainsKey(whisperMSG[1]))
                                        {
                                            wolfcoins.classList[whisperMSG[1].ToString()].prestige = value;
                                            group.Whisper(whisperSender, "Set " + whisperMSG[1] + "'s Prestige to " + value + ".");
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Error updating Prestige Level.");
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "Invalid data provided for !setprestige command.");
                                    }
                                }
                                else
                                {
                                    group.Whisper(whisperSender, "Not enough data provided for !setprestige command.");
                                }
                            }
                            else if (whisperMessage.StartsWith("C") || whisperMessage.StartsWith("c"))
                            {
                                if (wolfcoins.classList != null)
                                {
                                    if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()))
                                    {
                                        if (wolfcoins.classList[whisperSender].classType == -1)
                                        {
                                            wolfcoins.SetClass(whisperSender, whisperMessage, group);
                                        }

                                        if (wolfcoins.classList[whisperSender].classType.ToString().EndsWith("0"))
                                        {
                                            char c = whisperMessage.Last();
                                            int newClass = -1;
                                            int.TryParse(c.ToString(), out newClass);
                                            wolfcoins.ChangeClass(whisperSender, newClass, group);
                                        }

                                    }
                                }
                            }
                            // 
                            //
                            //               COMMANDS TO FIX STUFF
                            //
                            //
                            else if (whisperMessage == "!patch1")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                if (wolfcoins.xpList != null)
                                {
                                    CharClass emptyClass = new CharClass
                                    {
                                        classType = -1,
                                        totalItemCount = -1
                                    };

                                    foreach (var viewer in wolfcoins.xpList)
                                    {
                                        int myLevel = wolfcoins.DetermineLevel(viewer.Key);
                                        if (myLevel >= 3 && !wolfcoins.classList.ContainsKey(viewer.Key))
                                        {
                                            emptyClass.name = viewer.Key;
                                            emptyClass.level = myLevel;
                                            wolfcoins.classList.Add(viewer.Key, emptyClass);
                                            Console.WriteLine("Added " + viewer.Key + " to the Class List.");
                                        }
                                    }
                                    wolfcoins.SaveClassData();
                                }
                            }
                            // command to fix multiple inventory ids and active states
                            else if (whisperMessage == "!fixinventory")
                            {
                                if (whisperSender != "lobosjr")
                                    continue;

                                foreach (var player in wolfcoins.classList)
                                {
                                    player.Value.FixItems();
                                }

                                //Console.WriteLine(wolfcoins.classList["kraad_"].FixItems());

                                wolfcoins.SaveClassData();
                            }

                            else if (whisperMessage == "1")
                            {
                                group.Whisper(whisperSender, "Wolfcoins are a currency you earn by watching the stream! You can check your coins by whispering me '!coins' or '!stats'. To find out what you can spend coins on, message me '!shop'.");
                            }

                            else if (whisperMessage == "2")
                            {
                                group.Whisper(whisperSender, "Did you know you gain experience by watching the stream? You can level up as you get more XP! Max level is 20. To check your level & xp, message me '!xp' '!level' or '!stats'. Only Level 2+ viewers can post links. This helps prevent bot spam!");
                            }

                            else if (whisperMessage == "!shop")
                            {
                                group.Whisper(whisperSender, "Whisper me '!stats <username>' to check another users stats! (Cost: 1 coin)   Whisper me '!gloat' to spend 10 coins and show off your level! (Cost: 10 coins)");
                            }

                            else if (whisperMessage == "!dungeonlist")
                            {
                                group.Whisper(whisperSender, "List of Wolfpack RPG Adventures: http://tinyurl.com/WolfpackAdventureList");
                            }
                            else if (whisperMessage == "!debugcatch")
                            {
                                if (whisperSender == "lobosjr")
                                {
                                    for (int i = 0; i < 50; i++)
                                    {
                                        Fish randomFish = new Fish(FishingModule.WeightedRandomFish(ref fishDatabase));
                                        Console.WriteLine(randomFish.name + " (Rarity " + randomFish.rarity + ") caught.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!debuglevel5"))
                            {
                                if (whisperSender == "lobosjr")
                                {
                                    string[] whisperMSG = whispers[1].Split();
                                    if (whisperMSG.Length > 1)
                                    {
                                        string user = whisperMSG[1];
                                        if (wolfcoins.Exists(wolfcoins.classList, user))
                                        {
                                            wolfcoins.classList.Remove(user);
                                            wolfcoins.SetXP(1, user, group);
                                            wolfcoins.SetXP(600, user, group);
                                        }
                                        else
                                        {
                                            wolfcoins.SetXP(1, user, group);
                                            wolfcoins.SetXP(600, user, group);
                                        }
                                    }
                                }
                            }

                            else if (whisperMessage.StartsWith("!clearclass"))
                            {
                                if (whisperSender == "lobosjr")
                                {
                                    if (wolfcoins.classList != null)
                                    {
                                        string[] whisperMSG = whispers[1].Split();
                                        if (whisperMSG.Length > 1)
                                        {
                                            string user = whisperMSG[1];
                                            if (wolfcoins.classList.Keys.Contains(user.ToLower()))
                                            {
                                                wolfcoins.classList.Remove(user);
                                                wolfcoins.SaveClassData();
                                                group.Whisper(whisperSender, "Cleared " + user + "'s class.");
                                            }
                                            else
                                            {
                                                group.Whisper(whisperSender, "Couldn't find you in the class table.");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!setcoins"))
                            {
                                if (whisperSender != "lobosjr")
                                    break;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    if (int.TryParse(whisperMSG[2], out int value))
                                    {
                                        if (!wolfcoins.Exists(wolfcoins.coinList, whisperMSG[1]))
                                        {
                                            wolfcoins.coinList.Add(whisperMSG[1], 0);
                                        }
                                        int newCoins = wolfcoins.SetCoins(value, whisperMSG[1]);
                                        if (newCoins != -1)
                                        {
                                            group.Whisper(whisperSender, "Set " + whisperMSG[1] + "'s coins to " + newCoins + ".");
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "Error updating Coin amount.");
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "Invalid data provided for !setcoins command.");
                                    }
                                }
                                else
                                {
                                    group.Whisper(whisperSender, "Not enough data provided for !setcoins command.");
                                }
                            }

                            if (whisperMessage == "!coins" || whisperMessage == "coins")
                            {
                                if (wolfcoins.coinList != null)
                                {
                                    if (wolfcoins.coinList.ContainsKey(whisperSender))
                                    {

                                        group.Whisper(whisperSender, "You have: " + wolfcoins.coinList[whisperSender] + " coins.");
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any coins yet! Stick around during the livestream to earn coins.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!gloatpet") || whisperMessage.StartsWith("!petgloat"))
                            {
                                if (wolfcoins.classList.ContainsKey(whisperSender))
                                {
                                    if (wolfcoins.coinList[whisperSender] < gloatCost)
                                    {
                                        group.Whisper(whisperSender, "You don't have enough coins to gloat!");
                                        continue;
                                    }

                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        bool hasActive = false;
                                        Pet toGloat = new Pet();
                                        foreach (var pet in wolfcoins.classList[whisperSender].myPets)
                                        {
                                            if (pet.isActive)
                                            {
                                                hasActive = true;
                                                toGloat = pet;
                                                break;
                                            }
                                        }

                                        if (!hasActive)
                                        {
                                            group.Whisper(whisperSender, "You don't have an active pet to show off! Activate one with !summon <id>");
                                            continue;
                                        }

                                        string temp = gloatCost.ToString();
                                        wolfcoins.RemoveCoins(whisperSender, temp);


                                        string petType = "";
                                        if (toGloat.isSparkly)
                                            petType += "SPARKLY " + toGloat.type;
                                        else
                                            petType = toGloat.type;

                                        irc.SendChatMessage(whisperSender + " watches proudly as their level " + toGloat.level + " " + petType + " named " + toGloat.name + " struts around!");
                                        group.Whisper(whisperSender, "You spent " + temp + " wolfcoins to brag about " + toGloat.name + ".");
                                    }
                                }

                            }
                            else if (whisperMessage.StartsWith("!gloatfish") || whisperMessage.StartsWith("!fishgloat"))
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.coinList[whisperSender] < gloatCost)
                                    {
                                        group.Whisper(whisperSender, "You don't have enough coins to gloat!");
                                        continue;
                                    }

                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        Fish toGloat = new Fish();
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            group.Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>");
                                            continue;
                                        }
                                        int fishID = -1;
                                        if (int.TryParse(msgData[1], out fishID))
                                        {
                                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                                            {
                                                string temp = gloatCost.ToString();
                                                wolfcoins.RemoveCoins(whisperSender, temp);

                                                Fish tempFish = new Fish(wolfcoins.fishingList[whisperSender].biggestFish.ElementAt(fishID - 1));

                                                irc.SendChatMessage(whisperSender + " gloats about the time they caught a  " + tempFish.length + " in. long, " + tempFish.weight + " pound " + tempFish.name + " lobosSmug");
                                                group.Whisper(whisperSender, "You spent " + temp + " wolfcoins to brag about your biggest" + tempFish.name + ".");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!");
                                    }


                                }
                            }
                            else if (whisperMessage.StartsWith("!gloat") || whisperMessage.StartsWith("gloat"))
                            {
                                if (wolfcoins.coinList != null && wolfcoins.xpList != null)
                                {
                                    if (wolfcoins.coinList.ContainsKey(whisperSender) && wolfcoins.xpList.ContainsKey(whisperSender))
                                    {

                                        if (wolfcoins.coinList[whisperSender] >= gloatCost)
                                        {
                                            string temp = gloatCost.ToString();
                                            string gloatMessage = "";
                                            int level = wolfcoins.DetermineLevel(whisperSender);
                                            string levelWithPrestige = wolfcoins.GloatWithPrestige(whisperSender);
                                            wolfcoins.RemoveCoins(whisperSender, temp);
                                            #region gloatMessages
                                            switch (level)
                                            {
                                                case 1:
                                                    {
                                                        gloatMessage = "Just a baby! lobosMindBlank";
                                                    }
                                                    break;

                                                case 2:
                                                    {
                                                        gloatMessage = "Scrubtastic!";
                                                    }
                                                    break;

                                                case 3:
                                                    {
                                                        gloatMessage = "Pretty weak!";
                                                    }
                                                    break;

                                                case 4:
                                                    {
                                                        gloatMessage = "Not too shabby.";
                                                    }
                                                    break;

                                                case 5:
                                                    {
                                                        gloatMessage = "They can hold their own!";
                                                    }
                                                    break;

                                                case 6:
                                                    {
                                                        gloatMessage = "Getting pretty strong Kreygasm";
                                                    }
                                                    break;

                                                case 7:
                                                    {
                                                        gloatMessage = "A formidable opponent!";
                                                    }
                                                    break;

                                                case 8:
                                                    {
                                                        gloatMessage = "A worthy adversary!";
                                                    }
                                                    break;

                                                case 9:
                                                    {
                                                        gloatMessage = "A most powerful combatant!";
                                                    }
                                                    break;

                                                case 10:
                                                    {
                                                        gloatMessage = "A seasoned war veteran!";
                                                    }
                                                    break;

                                                case 11:
                                                    {
                                                        gloatMessage = "A fearsome champion of the Wolfpack!";
                                                    }
                                                    break;

                                                case 12:
                                                    {
                                                        gloatMessage = "A vicious pack leader!";
                                                    }
                                                    break;

                                                case 13:
                                                    {
                                                        gloatMessage = "A famed Wolfpack Captain!";
                                                    }
                                                    break;

                                                case 14:
                                                    {
                                                        gloatMessage = "A brutal commander of the Wolfpack!";
                                                    }
                                                    break;

                                                case 15:
                                                    {
                                                        gloatMessage = "Decorated Chieftain of the Wolfpack!";
                                                    }
                                                    break;

                                                case 16:
                                                    {
                                                        gloatMessage = "A War Chieftain of the Wolfpack!";
                                                    }
                                                    break;

                                                case 17:
                                                    {
                                                        gloatMessage = "A sacred Wolfpack Justicar!";
                                                    }
                                                    break;

                                                case 18:
                                                    {
                                                        gloatMessage = "Demigod of the Wolfpack!";
                                                    }
                                                    break;

                                                case 19:
                                                    {
                                                        gloatMessage = "A legendary Wolfpack demigod veteran!";
                                                    }
                                                    break;

                                                case 20:
                                                    {
                                                        gloatMessage = "The Ultimate Wolfpack God Rank. A truly dedicated individual.";
                                                    }
                                                    break;

                                                default: break;
                                            }
                                            #endregion

                                            irc.SendChatMessage(whisperSender + " has spent " + gloatCost + " Wolfcoins to show off that they are " + levelWithPrestige + "! " + gloatMessage);
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "You don't have enough coins to gloat (Cost: " + gloatCost + " Wolfcoins)");
                                        }
                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have coins and/or xp yet!");
                                    }
                                }
                            }
                            else if ((whisperMessage.StartsWith("!bet") || whisperMessage.StartsWith("bet")) && betsAllowed && betActive && wolfcoins.Exists(wolfcoins.coinList, whisperSender))
                            {
                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {
                                    Better betInfo = new Better();
                                    string user = whispers[1];
                                    string vote = whispers[2].ToLower();
                                    int betAmount = -1;
                                    if (int.TryParse(whispers[3], out betAmount))
                                    {
                                        if (!wolfcoins.CheckCoins(user, betAmount))
                                        {
                                            group.Whisper(user, "There was an error placing your bet. (not enough coins?)");
                                            continue;
                                        }
                                        betInfo.betAmount = betAmount;
                                        if (!betters.ContainsKey(user))
                                        {
                                            wolfcoins.RemoveCoins(user, betAmount.ToString());
                                            if (vote == "succeed")
                                            {
                                                betInfo.vote = SUCCEED;
                                                betters.Add(user, betInfo);

                                            }
                                            else if (vote == "fail")
                                            {
                                                betInfo.vote = FAIL;
                                                betters.Add(user, betInfo);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!givexp"))
                            {
                                if (whisperSender != "lobosjr")
                                    break;

                                string[] whisperMSG = whispers[1].Split();

                                if (whisperMSG[0] != null && whisperMSG[1] != null && whisperMSG[2] != null)
                                {
                                    if (!(int.TryParse(whisperMSG[2].ToString(), out int value)))
                                    {
                                        break;
                                    }
                                    string user = whisperMSG[1].ToString();

                                    wolfcoins.AwardXP(value, user, group);
                                    wolfcoins.SaveClassData();
                                    wolfcoins.SaveXP();
                                    group.Whisper(whisperSender, "Gave " + user + " " + value + " XP.");
                                }
                                else
                                {
                                    irc.SendChatMessage("Not enough data provided for !givexp command.");
                                }
                            }
                            else if (whisperMessage.StartsWith("!xp") || whisperMessage.StartsWith("xp") || whisperMessage.StartsWith("level") || whisperMessage.StartsWith("!level") ||
                                whisperMessage.StartsWith("!lvl") || whisperMessage.StartsWith("lvl"))
                            {
                                if (wolfcoins.xpList != null)
                                {
                                    if (wolfcoins.xpList.ContainsKey(whisperSender))
                                    {

                                        int myLevel = wolfcoins.DetermineLevel(whisperSender);
                                        int xpToNextLevel = wolfcoins.XpToNextLevel(whisperSender);
                                        int myPrestige = -1;
                                        if (wolfcoins.classList.ContainsKey(whisperSender))
                                        {
                                            myPrestige = wolfcoins.classList[whisperSender].prestige;
                                        }
                                        //if(!wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                        //{
                                        //    group.Whisper(whisperSender, "You are Level " + myLevel + " (Total XP: " + wolfcoins.xpList[whisperSender] + ")");
                                        //}
                                        //else
                                        //{
                                        //    string myClass = wolfcoins.determineClass(whisperSender);
                                        //    group.Whisper(whisperSender, "You are a Level " + myLevel + " " + myClass + " (Total XP: " + wolfcoins.xpList[whisperSender] + ")");
                                        //}

                                        if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()))
                                        {
                                            string myClass = wolfcoins.DetermineClass(whisperSender);
                                            group.Whisper(whisperSender, "You are a Level " + myLevel + " " + myClass + ", and you are Prestige Level " + myPrestige + ". (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")");
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "You are Level " + myLevel + " (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")");
                                        }

                                    }
                                    else
                                    {
                                        group.Whisper(whisperSender, "You don't have any XP yet! Hang out in chat during the livestream to earn XP & coins.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!shutdown"))
                            {
                                if (whisperSender != "lobosjr")
                                    break;

                                string[] temp = whispers[1].Split(' ');
                                if (temp.Count() > 1)
                                {
                                    int numMinutes = -1;
                                    if (int.TryParse(temp[1], out numMinutes))
                                    {
                                        foreach (var player in groupFinder.queue)
                                        {
                                            group.Whisper(player.name, "Attention! Wolfpack RPG will be coming down for maintenance in about " + numMinutes + " minutes. If you are dungeoning while the bot shuts down, your progress may not be saved.");
                                        }
                                    }

                                }
                            }
                            else if (whisperMessage.StartsWith("!stats") || whisperMessage.StartsWith("stats"))
                            {
                                if (wolfcoins.coinList != null && wolfcoins.xpList != null)
                                {
                                    string[] temp = whispers[1].Split(' ');
                                    if (temp.Count() > 1)
                                    {
                                        string desiredUser = temp[1].ToLower();
                                        if (wolfcoins.xpList.ContainsKey(desiredUser) && wolfcoins.coinList.ContainsKey(desiredUser))
                                        {

                                            wolfcoins.RemoveCoins(whisperSender, pryCost.ToString());
                                            if (wolfcoins.Exists(wolfcoins.classList, desiredUser))
                                            {
                                                group.Whisper(whisperSender, "" + desiredUser + " is a Level " + wolfcoins.DetermineLevel(desiredUser) + " " + wolfcoins.DetermineClass(desiredUser) + " (" + wolfcoins.xpList[desiredUser] + " XP), Prestige Level " + wolfcoins.classList[desiredUser].prestige + ", and has " +
                                                    wolfcoins.coinList[desiredUser] + " Wolfcoins.");
                                            }
                                            else
                                            {
                                                group.Whisper(whisperSender, "" + desiredUser + " is Level " + " " + wolfcoins.DetermineLevel(desiredUser) + " (" + wolfcoins.xpList[desiredUser] + " XP) and has " +
                                                    wolfcoins.coinList[desiredUser] + " Wolfcoins.");
                                            }
                                            group.Whisper(whisperSender, "It cost you " + pryCost + " Wolfcoins to discover this information.");
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "User does not exist in database. You were charged no coins.");
                                        }
                                    }
                                    else if (wolfcoins.coinList.ContainsKey(whisperSender) && wolfcoins.xpList.ContainsKey(whisperSender))
                                    {

                                        int myLevel = wolfcoins.DetermineLevel(whisperSender);
                                        int myPrestige = -1;

                                        if (wolfcoins.classList.ContainsKey(whisperSender))
                                        {
                                            myPrestige = wolfcoins.classList[whisperSender].prestige;
                                        }

                                        int xpToNextLevel = wolfcoins.XpToNextLevel(whisperSender);
                                        group.Whisper(whisperSender, "You have: " + wolfcoins.coinList[whisperSender] + " coins.");
                                        if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()))
                                        {
                                            string myClass = wolfcoins.DetermineClass(whisperSender);
                                            group.Whisper(whisperSender, "You are a Level " + myLevel + " " + myClass + ", and you are Prestige Level " + myPrestige + ". (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")");
                                        }
                                        else
                                        {
                                            group.Whisper(whisperSender, "You are Level " + myLevel + " (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")");
                                        }
                                    }
                                    if (!(wolfcoins.coinList.ContainsKey(whisperSender)) || !(wolfcoins.xpList.ContainsKey(whisperSender)))
                                    {
                                        group.Whisper(whisperSender, "You either don't have coins or xp yet. Hang out in chat during the livestream to earn them!");
                                    }
                                }
                            }

                        }
                    }
                    #endregion
                    #region messageRegion
                    if (message.Length > 1)
                    {
                        if (message[0] != null && message[1] != null)
                        {
                            string[] first = message[1].Split(' ');
                            string sender = message[0];

                            if (sender == "twitchnotify" && first.Last() == "subscribed!")
                            {
                                // This code updates subcounter.txt for Subathon. Problem is having to recode the modifier update when the goal is met
                                //var text = File.ReadAllText(@"C:\Users\Lobos\Dropbox\Stream\subcounter.txt");
                                //int count = int.Parse(text);
                                //count--;
                                //File.WriteAllText(@"C:\Users\Lobos\Dropbox\Stream\subcounter.txt", count.ToString());
                                string newSub = first[0].ToLower();
                                if (!wolfcoins.subSet.Contains(newSub))
                                {
                                    wolfcoins.subSet.Add(newSub);
                                    Console.WriteLine("Added " + first[0] + " to the subs list.");
                                }
                            }
                            if (first[0] == "!stats" || first[0] == "!xp" || first[0] == "!lvl"
                                || first[0] == "!level" || first[0] == "!exp")
                            {
                                irc.SendChatMessage("/timeout " + sender + " 1");
                                group.Whisper(sender, "I see you're trying to check your stats! You'll need to WHISPER to me to get any information. Type '/w lobotjr' and then stats, xp, coins, etc. for that information.");
                                group.Whisper(sender, "Sorry for purging you. Just trying to do my job to keep chat clear! <3");
                            }

                            switch (first[0])
                            {
                                case "!nextaward":
                                    {
                                        if (sender != "lobosjr")
                                            break;
                                        double totalSec = (DateTime.Now - awardLast).TotalSeconds;
                                        int timeRemaining = (awardInterval * 60) - (int)(DateTime.Now - awardLast).TotalSeconds;
                                        int secondsRemaining = timeRemaining % 60;
                                        int minutesRemaining = timeRemaining / 60;
                                        irc.SendChatMessage(minutesRemaining + " minutes and " + secondsRemaining + " seconds until next coins/xp are awarded.");

                                    }
                                    break;

                                case "!setinterval":
                                    {
                                        if (sender != "lobosjr")
                                            break;

                                        int newAmount = 0;
                                        if (first.Count() > 1)
                                        {
                                            if (int.TryParse(first[1], out newAmount))
                                            {
                                                awardInterval = newAmount;
                                                irc.SendChatMessage("XP & Coins will now be awarded every " + newAmount + " minutes.");
                                            }
                                        }

                                    }
                                    break;

                                case "!setmultiplier":
                                    {
                                        if (sender != "lobosjr")
                                            break;

                                        int newAmount = 0;
                                        if (first.Count() > 1)
                                        {
                                            if (int.TryParse(first[1], out newAmount))
                                            {
                                                awardMultiplier = newAmount;
                                                irc.SendChatMessage(newAmount + "x XP & Coins will now be awarded.");
                                            }
                                        }

                                    }
                                    break;

                                //case "!endbet":
                                //    {
                                //        if (!betActive)
                                //            break;

                                //        if (wolfcoins.viewers.chatters.moderators.Contains(sender))
                                //        {
                                //            if (first.Count() > 1 && !betActive)
                                //            {
                                //                string result = first[1].ToLower();
                                //                if(result == "succeed")
                                //                {
                                //                    foreach(var element in betters)
                                //                    {
                                //                        //wolfcoins.AddCoins()
                                //                    }
                                //                }
                                //                else if(result == "fail")
                                //                {

                                //                }
                                //            }
                                //        }


                                //    } break;

                                //case "!closebet":
                                //    {
                                //        if(!betActive || !betsAllowed)
                                //            break;

                                //        if (wolfcoins.viewers.chatters.moderators.Contains(sender))
                                //        {
                                //            betsAllowed = false;
                                //            irc.sendChatMessage("Bets are now closed! Good luck FrankerZ");
                                //        }
                                //    } break;

                                //case "!startbet":
                                //    {

                                //        betStatement = "";
                                //        if (wolfcoins.viewers.chatters.moderators.Contains(sender))
                                //        {
                                //            if (first.Count() > 1 && !betActive)
                                //            {
                                //                betActive = true;
                                //                betsAllowed = true;
                                //                for (int i = 0; i < first.Count() - 1; i++)
                                //                {
                                //                    betStatement += first[i + 1];
                                //                }
                                //                irc.sendChatMessage("New bet started: " + betStatement + " Type '!bet succeed' or '!bet fail' to bet.");

                                //            }
                                //        }
                                //    } break;

                                case "!xpon":
                                    {
                                        wolfcoins.UpdateViewers(channel);
                                        if ((wolfcoins.viewers.chatters.moderators.Contains(sender) || sender == "lobosjr" || sender == "lan5432") && !broadcasting)
                                        {
                                            broadcasting = true;
                                            awardLast = DateTime.Now;
                                            irc.SendChatMessage("Wolfcoins & XP will be awarded.");
                                        }
                                    }
                                    break;

                                case "!xpoff":
                                    {
                                        wolfcoins.UpdateViewers(channel);
                                        if ((wolfcoins.viewers.chatters.moderators.Contains(sender) || sender == "lobosjr" || sender == "lan5432") && broadcasting)
                                        {
                                            broadcasting = false;
                                            irc.SendChatMessage("Wolfcoins & XP will no longer be awarded.");
                                            wolfcoins.BackupData();
                                        }
                                    }
                                    break;

                                case "!setxp":
                                    {
                                        if (sender != "lobosjr")
                                            break;

                                        if (first.Length >= 3 && first[1] != null && first[2] != null)
                                        {
                                            if (int.TryParse(first[2], out int value))
                                            {
                                                int newXp = wolfcoins.SetXP(value, first[1], group);
                                                if (newXp != -1)
                                                {
                                                    irc.SendChatMessage("Set " + first[1] + "'s XP to " + newXp + ".");
                                                }
                                                else
                                                {
                                                    irc.SendChatMessage("Error updating XP amount.");
                                                }
                                            }
                                            else
                                            {
                                                irc.SendChatMessage("Invalid data provided for !setxp command.");
                                            }
                                        }
                                        else
                                        {
                                            irc.SendChatMessage("Not enough data provided for !setxp command.");
                                        }

                                    }
                                    break;

                                case "!grantxp":
                                    {
                                        if (sender != "lobosjr")
                                            break;

                                        if (first[0] != null && first[1] != null)
                                        {
                                            if (int.TryParse(first[1], out int value))
                                            {
                                                wolfcoins.AwardXP(value, group);
                                            }
                                            else
                                            {
                                                irc.SendChatMessage("Invalid data provided for !givexp command.");
                                            }
                                        }
                                        else
                                        {
                                            irc.SendChatMessage("Not enough data provided for !givexp command.");
                                        }

                                    }
                                    break;

                                case "!setcoins":
                                    {

                                    }
                                    break;

                                #region NormalBotStuff
                                //case "!hug":
                                //    {
                                //        irc.sendChatMessage("/me gives " + sender + " a big hug!");
                                //    } break;

                                //case "!playlist":
                                //    {
                                //        irc.sendChatMessage("Lobos' Spotify Playlist: http://open.spotify.com/user/1251282601/playlist/2j1FVSjJ4zdJiqGQgXgW3t");
                                //    } break;

                                //case "!opinion":
                                //    {
                                //        irc.sendChatMessage("Opinions go here: http:////i.imgur.com/3jRQ2fa.jpg");
                                //    } break;

                                //case "!quote":
                                //    {
                                //        string path = @"C:\Users\Owner\Dropbox\Stream\quotes.txt";
                                //        string myFile = "";
                                //        if (File.Exists(path))
                                //        {
                                //            myFile = File.ReadAllText(path);
                                //            string[] quotes = myFile.Split('\n');
                                //            int numQuotes = quotes.Length;
                                //            Random random = new Random();
                                //            int randomNumber = random.Next(0, numQuotes);
                                //            irc.sendChatMessage(quotes[randomNumber]);
                                //        }
                                //        else
                                //        {
                                //            irc.sendChatMessage("Quotes file does not exist.");
                                //        }
                                //    } break;

                                //case "!pun":
                                //    {
                                //        string path = @"C:\Users\Owner\Dropbox\Stream\puns.txt";
                                //        string myFile = "";
                                //        if (File.Exists(path))
                                //        {
                                //            myFile = File.ReadAllText(path);
                                //            string[] puns = myFile.Split('\n');
                                //            int numPuns = puns.Length;
                                //            Random random = new Random();
                                //            int randomNumber = random.Next(0, numPuns);
                                //            irc.sendChatMessage(puns[randomNumber]);
                                //        }
                                //        else
                                //        {
                                //            irc.sendChatMessage("Puns file does not exist.");
                                //        }
                                //    } break;

                                //case "!whisper":
                                //    {
                                //        if (group.connected)
                                //        {
                                //            group.sendChatMessage(".w " + sender + " Psssssst!");
                                //        }
                                //    } break;
                                #endregion
                                // remove coins from a target. Ex: !removecoins lobosjr 200
                                case "!removecoins":
                                    {

                                        if (first.Length < 3)
                                        {
                                            irc.SendChatMessage("Not enough information provided.");
                                            break;
                                        }

                                        string target = first[1];
                                        string coins = first[2];

                                        if (sender == "lobosjr")
                                        {
                                            if (wolfcoins.RemoveCoins(target, coins))
                                            {
                                                Console.WriteLine(sender + " removed " + coins + " coins from " + target + ".");
                                                irc.SendChatMessage(sender + " removed " + coins + " coins from " + target + ".");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Coin add operation failed with the following information: ");
                                                Console.WriteLine("Sender: " + sender);
                                                Console.WriteLine("Target: " + target);
                                                Console.WriteLine("Amount: " + coins + " coins.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Non-Moderator attempted to add coins.");
                                            irc.SendChatMessage("Sorry, " + sender + ", AddCoins is a moderator-only command.");
                                        }

                                    }
                                    break;

                                case "!addcoins":
                                    {

                                        if (first.Length < 3)
                                        {
                                            irc.SendChatMessage("Not enough information provided.");
                                            break;
                                        }

                                        string target = first[1];
                                        string coins = first[2];

                                        if (sender == "lobosjr")
                                        {
                                            if (wolfcoins.AddCoins(target, coins))
                                            {
                                                Console.WriteLine(sender + " granted " + target + " " + coins + " coins.");
                                                irc.SendChatMessage(sender + " granted " + target + " " + coins + " coins.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Coin add operation failed with the following information: ");
                                                Console.WriteLine("Sender: " + sender);
                                                Console.WriteLine("Target: " + target);
                                                Console.WriteLine("Amount: " + coins + " coins.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Non-Moderator attempted to add coins.");
                                            irc.SendChatMessage("Sorry, " + sender + ", AddCoins is a moderator-only command.");
                                        }

                                    }
                                    break;

                                default: break;

                            }
                        }
                    }
                    #endregion
                }

                Console.WriteLine("Connection terminated.");
                UpdateTokens(tokenData, clientData, true);
                connected = false;
                #endregion
            }
            else
            {
                TwitchPlaysModule.Run(irc);
            }
        }

        static void UpdateTokens(TokenData tokenData, Shared.Client.ClientData clientData, bool force = false)
        {
            bool tokenUpdated = false;
            if (force || DateTime.Now >= tokenData.ChatToken.ExpirationDate)
            {
                tokenUpdated = true;
                try
                {
                    tokenData.ChatToken = AuthToken.Refresh(clientData.ClientId, clientData.ClientSecret, tokenData.ChatToken.RefreshToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred refreshing the chat token: {e.Message}\n{e.StackTrace}");
                }
            }
            if (force || DateTime.Now >= tokenData.BroadcastToken.ExpirationDate)
            {
                tokenUpdated = true;
                try
                {
                    tokenData.BroadcastToken = AuthToken.Refresh(clientData.ClientId, clientData.ClientSecret, tokenData.BroadcastToken.RefreshToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred refreshing the streamer token: {e.Message}\n{e.StackTrace}");
                }
            }
            if (tokenUpdated)
            {
                try
                {
                    FileUtils.WriteTokenData(tokenData);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred writing the updated token data: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}