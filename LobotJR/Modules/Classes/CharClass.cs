using LobotJR.Modules.Pets;
using LobotJR.Modules.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Classes
{
    public class CharClass
    {
        public const int WARRIOR = 1;
        public const int MAGE = 2;
        public const int ROGUE = 3;
        public const int RANGER = 4;
        public const int CLERIC = 5;

        public float successChance = 0;
        public int itemFind = 0;
        public int coinBonus = 0;
        public int xpBonus = 0;
        public int classType = 0;
        public float preventDeathBonus = 0.0f;

        public int level = 0;
        public int xpEarned = 0;
        public int coinsEarned = 0;
        public int itemEarned = -1;
        public int petEarned = -1;
        public int groupID = -1;
        public int queuePriority = -1;

        public int prestige = 0;

        //public int toRelease = -1;
        public int totalItemCount = -1;
        public int totalPetCount = 0;
        public int numInvitesSent = 0;
        public bool pendingInvite = false;
        public bool isPartyLeader = false;
        public string name = "NAMELESS ONE";
        public string className = "Deprived";
        public bool usedGroupFinder = false;
        public int groupFinderDungeon = -1;

        public bool pendingPetRelease = false;
        public Pet toRelease = new Pet();

        public List<int> queueDungeons = new List<int>();

        public DateTime queueTime;

        public List<Item> myItems = new List<Item>();

        public List<Pet> myPets = new List<Pet>();

        public DateTime lastDailyGroupFinder = DateTime.MinValue;

        public string PrintItems()
        {
            string itemList = "";
            foreach (var itm in myItems)
            {
                string isActive = "No";
                if (itm.isActive)
                    isActive = "Yes";

                itemList += itm.inventoryID + ": " + itm.itemName + " Active: " + isActive + "\n";
            }
            return itemList;
        }

        public bool ReleasePet(int stableID)
        {

            if (myPets.Remove(myPets.ElementAt(stableID - 1)))
            {

                if ((stableID) > myPets.Count())
                    return true;

                int tempStableID = 1;
                foreach (var pet in myPets)
                {
                    pet.stableID = tempStableID;
                    tempStableID++;
                }
                //for (int i = (stableID - 1); i < myPets.Count(); i++)
                //{
                //    myPets.ElementAt(i).stableID--;

                //}
                return true;
            }
            else
                return false;
        }

        public string FixItems()
        {
            int numErrors = 0;
            int i = 0;
            // Make sure IDs are correct
            foreach (var itm in myItems)
            {

                if (itm.inventoryID != i)
                {
                    itm.inventoryID = i;
                    numErrors++;
                }
                i++;
            }

            RemoveDupes();

            FixActives();

            //Set Total Item Count
            totalItemCount = myItems.Count();

            return "Fixed " + numErrors + " inventory errors.";
        }

        public void FixActives()
        {

            int numFixes = 0;
            //TYPE_WEAPON = 1;
            //TYPE_ARMOR = 2;
            //TYPE_TRINKET = 3;
            //TYPE_OTHER = 4;

            bool weaponActive = false;
            bool armorActive = false;

            foreach (var itm in myItems)
            {
                if (itm.isActive)
                {
                    // if it's a weapon
                    if (itm.itemType == 1)
                    {
                        if (weaponActive)
                        {
                            itm.isActive = false;
                            numFixes++;
                        }
                        else
                        {
                            weaponActive = true;
                        }
                    }

                    // if it's an armor
                    if (itm.itemType == 2)
                    {
                        if (armorActive)
                        {
                            itm.isActive = false;
                            numFixes++;
                        }
                        else
                        {
                            armorActive = true;
                        }
                    }

                }
            }

            Console.WriteLine("Fixed " + numFixes + " Active errors.");
        }

        public void RemoveDupes()
        {
            // go through every item in the inventory, comparing to all other items
            int id = 0;
            int dupesRemoved = 0;
            while (id < myItems.Count())
            {
                foreach (var itm in myItems)
                {
                    if (itm.inventoryID == id)
                        continue;

                    // if two items w/ matching ids exist
                    if (itm.itemID == myItems[id].itemID)
                    {
                        // duplicate found
                        RemoveItem(itm);
                        dupesRemoved++;
                    }
                }
                id++;
            }

            Console.WriteLine("Removed " + dupesRemoved + " duplicates from " + name + "'s inventory.");
        }

        public float GetTotalSuccessChance()
        {
            float chance = 0;
            foreach (var itm in myItems)
            {
                if (itm.isActive)
                    chance += itm.successChance;
            }

            return successChance + chance;
        }

        public Item GetItem(int inventoryID)
        {
            foreach (var itm in myItems)
            {
                if (itm.inventoryID == inventoryID)
                    return itm;
            }

            return new Item();
        }

        public Pet GetPet(int petID)
        {
            foreach (var pet in myPets)
            {
                if (pet.ID == petID)
                    return pet;
            }

            return new Pet();
        }

        public void ClearQueue()
        {
            usedGroupFinder = false;
            queueDungeons = new List<int>();
        }

        public int GetItemPos(int inventoryID)
        {
            int itemIter = 0;
            foreach (var itm in myItems)
            {
                if (itm.inventoryID == inventoryID)
                    return itemIter;
                itemIter++;
            }

            return -1;
        }

        public bool Equals(CharClass player1, CharClass player2)
        {
            if (player1.name == player2.name)
                return true;

            return false;
        }

        public void AddItem(Item itm)
        {
            myItems.Add(itm);
        }

        public void ActivateItem(Item itm)
        {
            foreach (var iterItem in myItems)
            {
                if (itm.itemType == iterItem.itemType && iterItem != itm)
                {
                    iterItem.isActive = false;
                }
            }
            itm.isActive = true;
        }

        public void RemoveItem(Item itm)
        {
            foreach (var iterItem in myItems)
            {
                if (itm == iterItem)
                {
                    int tempType = itm.itemType;
                    myItems.Remove(itm);
                    int listPos = 0;
                    foreach (var newActive in myItems)
                    {
                        if (tempType == newActive.itemType)
                        {
                            myItems.ElementAt(listPos).isActive = true;
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }
}
