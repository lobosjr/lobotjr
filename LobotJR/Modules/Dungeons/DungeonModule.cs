using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LobotJR.Modules.Dungeons
{
    public class DungeonModule
    {
        public static int DetermineEligibility(string user, int dungeonID, Dictionary<int, string> dungeonList, int baseDungeonCost, Currency wolfcoins)
        {
            if (!dungeonList.ContainsKey(dungeonID))
                return -1;

            int playerLevel = wolfcoins.DetermineLevel(wolfcoins.xpList[user]);
            Dungeon tempDungeon = new Dungeon("content/dungeons/" + dungeonList[dungeonID]);

            if (wolfcoins.Exists(wolfcoins.coinList, user))
            {
                if (wolfcoins.coinList[user] < (baseDungeonCost + ((playerLevel - 3) * 10)))
                {
                    //not enough money
                    return -2;
                }
            }
            // no longer gate dungeons by level
            //if (tempDungeon.minLevel <= playerLevel)
            //    return 1;
            return 1;
        }

        public static string GetDungeonName(int dungeonID, Dictionary<int, string> dungeonList)
        {
            if (!dungeonList.ContainsKey(dungeonID))
                return "Invalid DungeonID";

            Dungeon tempDungeon = new Dungeon("content/dungeons/" + dungeonList[dungeonID]);
            return tempDungeon.dungeonName;
        }

        public static string GetEligibleDungeons(string user, Currency wolfcoins, Dictionary<int, string> dungeonList)
        {
            string eligibleDungeons = "";
            int playerLevel = wolfcoins.DetermineLevel(user);
            List<Dungeon> dungeons = new List<Dungeon>();
            Dungeon tempDungeon;
            foreach (var id in dungeonList)
            {
                tempDungeon = new Dungeon("content/dungeons/" + dungeonList[id.Key])
                {
                    dungeonID = id.Key
                };
                dungeons.Add(tempDungeon);
            }

            if (dungeons.Count == 0)
                return eligibleDungeons;

            bool firstAdded = false;
            foreach (var dungeon in dungeons)
            {
                //if(dungeon.minLevel <= playerLevel)
                //{
                if (!firstAdded)
                {
                    firstAdded = true;
                }
                else
                {
                    eligibleDungeons += ",";
                }
                eligibleDungeons += dungeon.dungeonID;


                //}
            }
            return eligibleDungeons;
        }

        public static void UpdateDungeons(string dungeonListPath, ref Dictionary<int, string> dungeonList)
        {
            IEnumerable<string> fileText;
            if (File.Exists(dungeonListPath))
            {
                fileText = File.ReadLines(dungeonListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load dungeon list file, {dungeonListPath} not found.");
            }

            dungeonList = new Dictionary<int, string>();
            int dungeonIter = 1;
            foreach (var line in fileText)
            {
                string[] temp = line.Split(',');
                int id = -1;
                int.TryParse(temp[0], out id);
                if (id != -1)
                    dungeonList.Add(id, temp[1]);
                else
                    Console.WriteLine("Invalid dungeon read on line " + dungeonIter);
                dungeonIter++;
            }
        }
    }
}
