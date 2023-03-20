using Classes;
using Equipment;
using LobotJR.Twitch;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wolfcoins;

namespace Adventures
{
    public class DungeonMessager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string DUNGEON_COMPLETE = "doneguid74293847562934";
        public static int MSG_INSTANT = 0;
        public static int MSG_QUEUED = 1;
        public static int MSG_DUNGEON_COMPLETE = 2;

        DateTime lastMessage;
        readonly HashSet<string> receivers;
        readonly TwitchClient twitchClient;
        public Queue<List<string>> messageQueue;
        static readonly int cooldown = 9000;
        readonly string myChannel = "";

        public void sendIrcMessage(string user, string message)
        {
            twitchClient.QueueWhisper(user, message);
        }

        public DungeonMessager(ref TwitchClient twitchClient, string channel, Party myParty)
        {
            HashSet<string> temp = new HashSet<string>();
            foreach (var member in myParty.members)
            {
                temp.Add(member.name);
            }
            receivers = temp;
            this.twitchClient = twitchClient;
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

        public void sendChatMessage(int msgType, string message, string user)
        {
            List<string> temp = new List<string>();
            temp.Add(msgType.ToString());
            temp.Add(message);
            temp.Add(user);
            messageQueue.Enqueue(temp);
        }

        public void sendChatMessage(string message, string user)
        {
            List<string> temp = new List<string>();
            temp.Add(MSG_INSTANT.ToString());
            temp.Add(message);
            temp.Add(user);
            messageQueue.Enqueue(temp);
        }

        public void sendChatMessage(string message, Party myParty)
        {
            List<string> temp = new List<string>();
            temp.Add(MSG_QUEUED.ToString());
            temp.Add(message);
            foreach (var member in myParty.members)
            {
                temp.Add(member.name);
            }
            messageQueue.Enqueue(temp);
        }

        // if there's no chat cooldown, lobot tries to send a message from the queue and then remove it. otherwise, do nothing
        // to determine dungeon complete, if msg = DUNGEON_COMPLETE, return 1, otherwise return 0
        public int processQueue()
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
                    try
                    {
                        sendIrcMessage(receiver, toSend);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
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

    class Rewards
    {
        public int xpReward = 0;
        public int coinReward = 0;
        public string name = "";

        public Rewards(string myName, int myXP, int myCoins)
        {
            xpReward = myXP;
            name = myName;
            coinReward = myCoins;
        }
    }

    public class Dungeon
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public int dungeonID = -1;
        public int adventureType = -1;
        public string adventureTitle = "";
        public DungeonMessager messenger;
        readonly Random RNG = new Random();
        public int maxPlayers = 3;
        public List<string> dungeonText = new List<string>();
        public Dictionary<string, int> encounters = new Dictionary<string, int>();
        public int numEncounters = -1;
        public string dungeonName = "";
        public string description = "";
        public string victoryText = "";
        public string defeatText = "";
        public float deathChance = 25.0f;
        public int minLevel = -1;
        public int maxLevel = -1;
        readonly DateTime lastMessage = DateTime.Now;
        public string myChannel = "";
        public const int numDungeonsPerLevel = 50;
        public const int baseDungeonCost = 50;
        public const int baseCoinReward = 50;
        public const int randomRewardMod = 5;
        public float levelRewardModifier = 0.05f;
        public List<int> loot = new List<int>();
        public Dictionary<int, Item> itemDB = new Dictionary<int, Item>();
        public float partyCoinBonus = 0;
        public float partyXpBonus = 0;
        public float partyItemFindBonus = 0;
        public float partySuccessRate = 0;
        public float partyDeathAvoidance = 0;

        public string logPath = "dungeonlog.txt";

        public Dungeon(string path)
        {
            IEnumerable<string> fileText = System.IO.File.ReadLines(path, UTF8Encoding.Default);
            int textIter = 1;
            string[] header = fileText.ElementAt(textIter).Split(',');
            dungeonName = header[0];
            int.TryParse(header[1], out numEncounters);
            float.TryParse(header[2], out partySuccessRate);
            int.TryParse(header[3], out minLevel);
            int.TryParse(header[4], out maxLevel);

            textIter++;
            string[] enemies = fileText.ElementAt(textIter).Split(',');
            if ((enemies.Count() / 2) != numEncounters)
                Logger.Error($"Dungeon at {path} has a mismatch for # of encounters & encounter data.");

            for (int i = 0; i < enemies.Count(); i += 2)
            {
                int.TryParse(enemies[i + 1], out int difficulty);
                encounters.Add(enemies[i], difficulty);
            }
            textIter++;
            description = fileText.ElementAt(textIter);
            textIter++;
            victoryText = fileText.ElementAt(textIter);
            textIter++;
            defeatText = fileText.ElementAt(textIter);
            textIter++;
            if (fileText.ElementAt(textIter).StartsWith("Loot="))
            {
                string[] temp = fileText.ElementAt(textIter).Split('=');
                string[] ids = temp[1].Split(',');
                for (int i = 0; i < ids.Length; i++)
                {
                    int toAdd = -1;
                    int.TryParse(ids[i], out toAdd);
                    loot.Add(toAdd);
                }
                textIter++;
            }
            int iter = 0;
            foreach (var line in fileText.Skip(textIter))
            {
                dungeonText.Add(line);
                iter++;
            }
        }

        public Dungeon(string path, string channel, Dictionary<int, Item> itemDatabase)
        {
            itemDB = itemDatabase;
            IEnumerable<string> fileText = System.IO.File.ReadLines(path, UTF8Encoding.Default);
            int textIter = 1;
            string[] header = fileText.ElementAt(textIter).Split(',');
            dungeonName = header[0];
            int.TryParse(header[1], out numEncounters);
            float.TryParse(header[2], out partySuccessRate);
            int.TryParse(header[3], out minLevel);
            int.TryParse(header[4], out maxLevel);

            textIter++;
            string[] enemies = fileText.ElementAt(textIter).Split(',');
            if ((enemies.Count() / 2) != numEncounters)
                Logger.Error($"Dungeon at {path} has a mismatch for # of encounters & encounter data.");

            for (int i = 0; i < enemies.Count(); i += 2)
            {
                int.TryParse(enemies[i + 1], out int difficulty);
                encounters.Add(enemies[i], difficulty);
            }
            textIter++;
            description = fileText.ElementAt(textIter);
            textIter++;
            victoryText = fileText.ElementAt(textIter);
            textIter++;
            defeatText = fileText.ElementAt(textIter);
            textIter++;
            if (fileText.ElementAt(textIter).StartsWith("Loot="))
            {
                string[] temp = fileText.ElementAt(textIter).Split('=');
                string[] ids = temp[1].Split(',');
                for (int i = 0; i < ids.Length; i++)
                {
                    int toAdd = -1;
                    int.TryParse(ids[i], out toAdd);
                    loot.Add(toAdd);
                }
                textIter++;
            }
            int iter = 0;
            foreach (var line in fileText.Skip(textIter))
            {
                dungeonText.Add(line);
                iter++;
            }
            myChannel = channel;
        }

        public Party RunDungeon(Party myParty, ref TwitchClient whisper)
        {
            messenger = new DungeonMessager(ref whisper, myChannel, myParty);
            List<Rewards> partyRewards = new List<Rewards>();
            messenger.sendChatMessage("Loading Dungeon Information (" + dungeonName + ")...", myParty);
            float percentPartyFull = (float)myParty.NumMembers() / maxPlayers;
            List<CharClass> partyMembers = new List<CharClass>();
            for (int i = 0; i < myParty.NumMembers(); i++)
            {
                partyMembers.Add(myParty.members.ElementAt(i));
            }

            int playerIter = 0;
            foreach (var player in partyMembers)
            {
                // add item bonuses
                if (player.myItems.Count > 0)
                {
                    ApplyItemBuffs(player);
                }
                playerIter++;
            }

            float numWarriors = 0;
            float numMages = 0;
            float numRogues = 0;
            float numRangers = 0;
            float numClerics = 0;

            // calculate all bonuses from party members into the dungeon 'stats'. 
            // diminishing returns for multiple classes
            foreach (var player in partyMembers)
            {
                switch (player.classType)
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



                //float updatedSuccess = (player.successChance * ((float)player.level / (float)maxLevel));
                //if (updatedSuccess < (player.successChance * 0.75f))
                //    updatedSuccess = player.successChance * 0.75f;

                //this.partySuccessRate += updatedSuccess;

                //partyDeathAvoidance += player.preventDeathBonus;
                //partyCoinBonus += player.coinBonus;
                //partyItemFindBonus += player.itemFind;
            }

            foreach (var player in partyMembers)
            {
                switch (player.classType)
                {
                    case CharClass.WARRIOR:
                        {
                            UpdateDungeonStats(numWarriors, player);
                        }
                        break;

                    case CharClass.MAGE:
                        {
                            UpdateDungeonStats(numMages, player);
                        }
                        break;

                    case CharClass.ROGUE:
                        {
                            UpdateDungeonStats(numRogues, player);
                        }
                        break;

                    case CharClass.RANGER:
                        {
                            UpdateDungeonStats(numRangers, player);
                        }
                        break;

                    case CharClass.CLERIC:
                        {
                            UpdateDungeonStats(numClerics, player);
                        }
                        break;

                    default: break;
                }
            }

            foreach (var player in partyMembers)
            {
                player.coinBonus = (int)Math.Round(partyCoinBonus);
                player.xpBonus = (int)Math.Round(partyXpBonus);
                player.itemFind = (int)Math.Round(partyItemFindBonus);
                player.preventDeathBonus = (int)Math.Round(partyDeathAvoidance);
            }
            // calculate prevent death bonus for party

            partySuccessRate *= percentPartyFull;
            //this.successRate = 200.0f; // for debug purposes

            int doubleI = 0;
            for (int i = 0; i < numEncounters; i++)
            {
                messenger.sendChatMessage(dungeonText.ElementAt(doubleI), myParty);
                doubleI++;
                messenger.sendChatMessage(dungeonText.ElementAt(doubleI), myParty);
                float encounterSuccess = partySuccessRate - (float)encounters.ElementAt(i).Value;
                doubleI++;
                //if encounter fails, people can die and the dungeon ends
                if (!CalculateEncounterOutcome(encounterSuccess))
                {
                    string defeatLogText = dungeonName + " failed by ";
                    messenger.sendChatMessage(defeatText, myParty);
                    int partyIter = 0;
                    foreach (var player in myParty.members)
                    {
                        if (partyMembers.Count < (partyIter + 1))
                            continue;

                        if (CalculateDeath(partyMembers.ElementAt(partyIter), partyDeathAvoidance))
                        {
                            int xp = CalculateXPReward(player);
                            xp += (int)((xp * levelRewardModifier) * player.level);
                            int randomAmount = RNG.Next((randomRewardMod * -1), randomRewardMod);
                            xp += randomAmount;
                            int coins = baseCoinReward;
                            coins += (int)((coins * levelRewardModifier) * player.level);
                            randomAmount = RNG.Next((randomRewardMod * -1), randomRewardMod);
                            coins += randomAmount;
                            if (xp < 3)
                                xp = 3;

                            if (coins < 3)
                                coins = 3;

                            player.coinsEarned = (coins * -1);
                            player.xpEarned = (xp * -1);

                            messenger.sendChatMessage("Sadly, you have died. You lost " + xp + " XP and " + coins + " Coins.", player.name);
                            foreach (var member in partyMembers)
                            {
                                if (member.name == player.name)
                                    continue;
                                messenger.sendChatMessage("In the chaos, " + player.name + " lost their life. Seek vengeance in their honor!", member.name);
                            }

                            Logger.Info($"{player.name} has died in a dungeon.");
                            defeatLogText += player.name + ", " + player.className + " (Died. -" + xp + " xp, -" + coins + " coins.) ";
                        }
                        else
                        {
                            defeatLogText += player.name + ", " + player.className + " ";
                        }
                        partyIter++;
                    }

                    System.IO.File.AppendAllText(logPath, defeatLogText + Environment.NewLine);
                    messenger.sendChatMessage("It's a sad thing your adventure has ended here. No XP or Coins have been awarded.", myParty);
                    messenger.sendChatMessage(DungeonMessager.MSG_DUNGEON_COMPLETE, DungeonMessager.DUNGEON_COMPLETE, "");
                    foreach (var member in partyMembers)
                    {
                        if (member.myItems.Count > 0)
                        {
                            RemoveItemBuffs(member, partyMembers);
                        }
                    }
                    ResetClassStats(partyMembers);
                    return myParty;
                }
                else
                {
                    messenger.sendChatMessage("Your party successfully defeated the " + encounters.ElementAt(i).Key + "!", myParty);
                }
            }
            messenger.sendChatMessage(victoryText, myParty);
            string members = "";
            foreach (var member in myParty.members)
            {
                int xp = CalculateXPReward(member);
                int coins = baseCoinReward;
                xp += ((member.xpBonus * xp) / 100);
                coins += ((member.coinBonus * coins) / 100);
                xp += (int)((xp * levelRewardModifier) * member.level);
                coins += (int)((coins * levelRewardModifier) * member.level);
                int randomAmount = RNG.Next((randomRewardMod * -1), randomRewardMod);
                xp += randomAmount;
                if (xp < 5)
                    xp = 5;


                randomAmount = RNG.Next((randomRewardMod * -1), randomRewardMod);
                coins += randomAmount;
                if (coins < 5)
                    coins = 5;

                member.coinsEarned = coins;
                member.xpEarned = xp;


                members += member.name + ", " + member.className + " (" + xp + " xp, " + coins + " coins.) ";

                int myLoot = awardLoot(member);
                int petLoot = awardPet(member);
                if (myLoot == -1 && petLoot == -1)
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + member.name + " completed a dungeon and earned " + xp + " xp and " + coins + " Wolfcoins.", whisper);
                    continue;
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + member.name + " completed a dungeon and earned " + xp + " xp and " + coins + " Wolfcoins.", whisper);
                    member.itemEarned = myLoot;
                    member.petEarned = petLoot;
                }

            }

            messenger.sendChatMessage("Dungeon complete. Your party remains intact.", myParty);

            messenger.sendChatMessage(DungeonMessager.MSG_DUNGEON_COMPLETE, DungeonMessager.DUNGEON_COMPLETE, "");
            //Whisper(myParty, "You've earned " + reward.xpReward + " XP and " + reward.coinReward + " Wolfcoins!", whisper);
            string writeToLog = dungeonName + " completed by " + members;
            System.IO.File.AppendAllText(logPath, dungeonName + " victory by " + members + Environment.NewLine);

            foreach (var member in partyMembers)
            {
                if (member.myItems.Count > 0)
                {
                    RemoveItemBuffs(member, partyMembers);
                }
            }
            ResetClassStats(partyMembers);
            return myParty;
        }

        public void ApplyItemBuffs(CharClass player)
        {
            foreach (var itm in player.myItems)
            {
                if (!itm.isActive)
                    continue;

                player.itemFind += itm.itemFind;
                player.successChance += itm.successChance;
                player.xpBonus += itm.xpBonus;
                player.coinBonus += itm.coinBonus;
            }

        }

        public void ResetClassStats(List<CharClass> partyMembers)
        {
            foreach (var member in partyMembers)
            {
                switch (member.classType)
                {
                    case CharClass.WARRIOR:
                        {
                            Warrior freshClass = new Warrior();
                            member.itemFind = freshClass.itemFind;
                            member.xpBonus = freshClass.xpBonus;
                            member.coinBonus = freshClass.coinBonus;
                            member.preventDeathBonus = freshClass.preventDeathBonus;
                            member.successChance = freshClass.successChance;
                        }
                        break;

                    case CharClass.MAGE:
                        {
                            Mage freshClass = new Mage();
                            member.itemFind = freshClass.itemFind;
                            member.xpBonus = freshClass.xpBonus;
                            member.coinBonus = freshClass.coinBonus;
                            member.preventDeathBonus = freshClass.preventDeathBonus;
                            member.successChance = freshClass.successChance;
                        }
                        break;

                    case CharClass.ROGUE:
                        {
                            Rogue freshClass = new Rogue();
                            member.itemFind = freshClass.itemFind;
                            member.xpBonus = freshClass.xpBonus;
                            member.coinBonus = freshClass.coinBonus;
                            member.preventDeathBonus = freshClass.preventDeathBonus;
                            member.successChance = freshClass.successChance;
                        }
                        break;

                    case CharClass.RANGER:
                        {
                            Ranger freshClass = new Ranger();
                            member.itemFind = freshClass.itemFind;
                            member.xpBonus = freshClass.xpBonus;
                            member.coinBonus = freshClass.coinBonus;
                            member.preventDeathBonus = freshClass.preventDeathBonus;
                            member.successChance = freshClass.successChance;
                        }
                        break;

                    case CharClass.CLERIC:
                        {
                            Cleric freshClass = new Cleric();
                            member.itemFind = freshClass.itemFind;
                            member.xpBonus = freshClass.xpBonus;
                            member.coinBonus = freshClass.coinBonus;
                            member.preventDeathBonus = freshClass.preventDeathBonus;
                            member.successChance = freshClass.successChance;
                        }
                        break;

                    default: break;
                }
            }
        }

        public void RemoveItemBuffs(CharClass player, List<CharClass> partyMembers)
        {
            int playerIter = 0;
            foreach (var member in partyMembers)
            {
                if (member.name == player.name)
                    break;

                playerIter++;
            }

            foreach (var itm in player.myItems)
            {
                if (!itm.isActive)
                    continue;

                partyMembers.ElementAt(playerIter).itemFind -= itm.itemFind;
                partyMembers.ElementAt(playerIter).successChance -= itm.successChance;
                partyMembers.ElementAt(playerIter).xpBonus -= itm.xpBonus;
                partyMembers.ElementAt(playerIter).coinBonus -= itm.coinBonus;
            }

        }

        public int awardLoot(CharClass player)
        {
            // default at 0
            int chanceForLoot = 0;
            chanceForLoot += player.itemFind;
            float roll = (float)RNG.Next(0, 100);
            // if player wins loot, find an item they don't already have from this dungeon that fits their class
            // if duplicate, return -1, bc we don't want dupes.
            if (chanceForLoot > roll)
            {

                foreach (var myLoot in loot)
                {
                    if (itemDB[myLoot - 1].forClass != player.classType)
                    {
                        continue;
                    }

                    bool hasItem = false;
                    foreach (var item in player.myItems)
                    {
                        if (item.itemID == itemDB[myLoot - 1].itemID)
                        {
                            hasItem = true;
                        }
                    }

                    if (!hasItem)
                    {
                        if (chanceForLoot > (roll + (itemDB[myLoot - 1].itemRarity * Item.QUALITY_MOD)))
                            return myLoot;
                    }
                    continue;
                }
            }
            return -1;
        }

        public int awardPet(CharClass player)
        {
            // default at 0
            int chanceForLoot = 150;


            //QUALITY_COMMON = 1;   | 150 out of 2000
            //QUALITY_UNCOMMON = 2; | 50 out of 2000
            //QUALITY_RARE = 3;     | 25 out of 2000
            //QUALITY_EPIC = 4;     | 10 out of 2000
            //QUALITY_ARTIFACT = 5; | 1 out of 2000

            float roll = (float)RNG.Next(1, 2000);
            // if player wins, send the rarity of pet to award
            if (chanceForLoot > roll)
            {
                if (roll > 50)
                {
                    // award a Common pet
                    return 1;
                }
                else if (roll > 25)
                {
                    // award an Uncommon pet
                    return 2;
                }
                else if (roll > 10)
                {
                    // award a Rare pet
                    return 3;
                }
                else if (roll > 1)
                {
                    // award an Epic pet
                    return 4;
                }
                else if (roll == 1)
                {
                    // award a Legendary pet
                    return 5;
                }
            }
            return -1;
        }
        //returns true if party succeeds
        public bool CalculateEncounterOutcome(float successChance)
        {
            float roll = (float)RNG.Next(0, 100);
            if (successChance > roll)
                return true;
            else
                return false;
        }

        //returns true if player dies
        public bool CalculateDeath(CharClass player, float partyPreventBonus)
        {
            deathChance -= (player.preventDeathBonus + partyPreventBonus);
            float roll = (float)RNG.Next(0, 100);
            if (deathChance > roll)
                return true;
            else
                return false;
        }

        public int CalculateXPReward(CharClass player)
        {
            int myLevel = player.level;
            float xpReward = XPForLevel(myLevel);
            float multiplier = (1.0f / (float)numDungeonsPerLevel);
            xpReward *= multiplier;
            return (int)xpReward;
        }

        //calculates the total xp to go from the provided level to the next one.
        public int XPForLevel(int level)
        {
            double levelXP = (int)(4 * (Math.Pow(level, 3)) + 50);
            levelXP = Math.Round((levelXP / 100d), 0) * 100;
            double nextLevelXP = (int)(4 * (Math.Pow((level + 1), 3)) + 50);
            nextLevelXP = Math.Round((nextLevelXP / 100d), 0) * 100;
            return (int)(nextLevelXP - levelXP);
        }

        public void UpdateDungeonStats(float classAmount, CharClass player)
        {
            float updatedSuccess = (player.successChance * ((float)player.level / (float)maxLevel));
            if (updatedSuccess < (player.successChance * 0.75f))
                updatedSuccess = player.successChance * 0.75f;

            partySuccessRate += (updatedSuccess / classAmount);

            partyDeathAvoidance += (player.preventDeathBonus / classAmount);
            partyCoinBonus += (player.coinBonus / classAmount);
            partyItemFindBonus += (player.itemFind / classAmount);
            partyXpBonus += (player.xpBonus / classAmount);
        }
    }
}
