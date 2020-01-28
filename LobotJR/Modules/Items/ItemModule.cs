using LobotJR.Client;
using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Modules.Items
{
    public class ItemModule
    {
        public static int GrantItem(int id, Currency wolfcoins, string user, Dictionary<int, Item> itemDatabase)
        {
            string logPath = "dungeonlog.txt";

            if (id < 1)
                return -1;

            Item newItem = itemDatabase[id - 1];
            bool hasActiveItem = false;
            foreach (var item in wolfcoins.classList[user].myItems)
            {
                if (item.itemType == newItem.itemType && item.isActive)
                    hasActiveItem = true;
            }
            if (!hasActiveItem)
                newItem.isActive = true;

            wolfcoins.classList[user].totalItemCount++;
            newItem.inventoryID = wolfcoins.classList[user].totalItemCount;
            wolfcoins.classList[user].myItems.Add(newItem);
            wolfcoins.classList[user].itemEarned = -1;

            File.AppendAllText(logPath, user + " looted a " + newItem.itemName + "!" + Environment.NewLine);
            Console.WriteLine(user + " just looted a " + newItem.itemName + "!");

            return newItem.itemID;
        }

        public static void UpdateItems(string itemListPath, ref Dictionary<int, string> itemList, ref Dictionary<int, Item> itemDatabase)
        {
            IEnumerable<string> fileText;
            if (File.Exists(itemListPath))
            {
                fileText = File.ReadLines(itemListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load item list file, {itemListPath} not found.");
            }
            itemDatabase = new Dictionary<int, Item>();
            itemList = new Dictionary<int, string>();
            int itemIter = 1;
            // ALERT: Was there a reason you were loading this from the file twice?
            // fileText = System.IO.File.ReadLines(itemListPath, UTF8Encoding.Default);
            foreach (var line in fileText)
            {
                string[] temp = line.Split(',');
                int id = -1;
                int.TryParse(temp[0], out id);
                if (id != -1)
                    itemList.Add(id, "content/items/" + temp[1]);
                else
                    Console.WriteLine("Invalid item read on line " + itemIter);
                itemIter++;
            }

            itemIter = 0;
            foreach (var item in itemList)
            {
                Item myItem = new Item();
                int parsedInt = -1;
                int line = 0;
                string[] temp = { "" };
                fileText = System.IO.File.ReadLines(itemList.ElementAt(itemIter).Value, UTF8Encoding.Default);
                // item ID
                myItem.itemID = itemList.ElementAt(itemIter).Key;
                // item name
                temp = fileText.ElementAt(line).Split('=');
                myItem.itemName = temp[1];
                line++;
                // item type (1=weapon, 2=armor, 3=other)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.itemType = parsedInt;
                line++;
                // Class designation (1=Warrior,2=Mage,3=Rogue,4=Ranger,5=Cleric)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.forClass = parsedInt;
                line++;
                // Item rarity (1=Uncommon,2=Rare,3=Epic,4=Artifact)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.itemRarity = parsedInt;
                line++;
                // success boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.successChance = parsedInt;
                line++;
                // item find (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.itemFind = parsedInt;
                line++;
                // coin boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.coinBonus = parsedInt;
                line++;
                // xp boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.xpBonus = parsedInt;
                line++;
                // prevent death boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.preventDeathBonus = parsedInt;
                line++;
                // item description
                temp = fileText.ElementAt(line).Split('=');
                myItem.description = temp[1];

                itemDatabase.Add(itemIter, myItem);

                itemIter++;
            }
        }

        public static void WhisperItem(string user, Item itm, IrcClient whisperClient, Dictionary<int, Item> itemDatabase)
        {
            string name = itm.itemName;
            string type = "";
            int inventoryID = itm.inventoryID;
            switch (itm.itemType)
            {
                case (Item.TYPE_ARMOR):
                    {
                        type = "Armor";
                    }
                    break;

                case (Item.TYPE_WEAPON):
                    {
                        type = "Weapon";
                    }
                    break;

                case (Item.TYPE_OTHER):
                    {
                        type = "Misc. Item";
                    }
                    break;
                default:
                    {
                        type = "Broken";
                    }
                    break;
            }
            string rarity = "";
            switch (itm.itemRarity)
            {
                case Item.QUALITY_UNCOMMON:
                    {
                        rarity = "Uncommon";
                    }
                    break;

                case Item.QUALITY_RARE:
                    {
                        rarity = "Rare";
                    }
                    break;

                case Item.QUALITY_EPIC:
                    {
                        rarity = "Epic";
                    }
                    break;

                case Item.QUALITY_ARTIFACT:
                    {
                        rarity = "Artifact";
                    }
                    break;

                default:
                    {
                        rarity = "Broken";
                    }
                    break;
            }
            List<string> stats = new List<string>();

            if (itm.successChance > 0)
            {
                stats.Add("+" + itm.successChance + "% Success Chance");
            }

            if (itm.xpBonus > 0)
            {
                stats.Add("+" + itm.xpBonus + "% XP Bonus");
            }

            if (itm.coinBonus > 0)
            {
                stats.Add("+" + itm.coinBonus + "% Wolfcoin Bonus");
            }

            if (itm.itemFind > 0)
            {
                stats.Add("+" + itm.itemFind + "% Item Find");
            }

            if (itm.preventDeathBonus > 0)
            {
                stats.Add("+" + itm.preventDeathBonus + "% to Prevent Death");
            }

            bool active = itm.isActive;
            string status = "";
            if (active)
            {
                status = "(Equipped)";
            }
            else
            {
                status = "(Unequipped)";
            }

            whisperClient.Whisper(user, name + " (" + rarity + " " + type + ") " + status);
            whisperClient.Whisper(user, "Inventory ID: " + inventoryID);
            foreach (var stat in stats)
            {
                whisperClient.Whisper(user, stat);
            }
        }
    }
}
