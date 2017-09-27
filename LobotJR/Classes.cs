using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Equipment;
using Companions;

namespace Classes
{

    class CharClass
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
        public int groupID = -1;
        public int queuePriority = -1;

        public int prestige = 0;

        public int totalItemCount = -1;
        public int numInvitesSent = 0;
        public bool pendingInvite = false;
        public bool isPartyLeader = false;
        public string name = "NAMELESS ONE";
        public string className = "Deprived";
        public bool usedGroupFinder = false;

        public List<int> queueDungeons = new List<int>();

        public DateTime queueTime;

        public List<Item> myItems = new List<Item>();

        public List<Pet> myPets = new List<Pet>();

        public DateTime lastDailyGroupFinder = DateTime.MinValue;

        public CharClass()
        {

        }

        public Item GetItem(int inventoryID)
        {
            foreach(var itm in myItems)
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
            foreach(var iterItem in myItems)
            {
                if(itm.itemType == iterItem.itemType && iterItem != itm)
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
                if(itm == iterItem)
                {
                    int tempType = itm.itemType;
                    myItems.Remove(itm);
                    int listPos = 0;
                    foreach(var newActive in myItems)
                    {
                        if(tempType == newActive.itemType)
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

    class Warrior : CharClass
    {

        public Warrior()
        {
            successChance = 10;
            itemFind = 3;
            coinBonus = 5;
            xpBonus = 0;
            classType = WARRIOR;
            className = "Warrior";
        }
    }

    class Mage : CharClass
    {

        public Mage()
        {
            successChance = 3;
            itemFind = 10;
            coinBonus = 0;
            xpBonus = 5;
            classType = MAGE;
            className = "Mage";
        }
    }

    class Rogue : CharClass
    {

        public Rogue()
        {
            successChance = 0;
            itemFind = 5;
            coinBonus = 10;
            xpBonus = 3;
            classType = ROGUE;
            className = "Rogue";
        }
    }

    class Ranger : CharClass
    {

        public Ranger()
        {
            successChance = 5;
            itemFind = 0;
            coinBonus = 3;
            xpBonus = 10;
            classType = RANGER;
            className = "Ranger";
        }
    }

    class Cleric : CharClass
    {

        public Cleric()
        {
            preventDeathBonus = 10.0f;
            successChance = 3;
            itemFind = 3;
            coinBonus = 3;
            xpBonus = 3;
            classType = CLERIC;
            className = "Cleric";
        }
    }

}
