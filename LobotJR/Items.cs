using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using TwitchBot;
using Classes;
using Wolfcoins;
using Adventures;

namespace Equipment
{
    class Item
    {
        public const int QUALITY_UNCOMMON = 1;
        public const int QUALITY_RARE = 2;
        public const int QUALITY_EPIC = 3;
        public const int QUALITY_ARTIFACT = 4;
        public const int QUALITY_MOD = 5;

        public const int TYPE_WEAPON = 1;
        public const int TYPE_ARMOR = 2;
        public const int TYPE_TRINKET = 3;
        public const int TYPE_OTHER = 4;

        public const int CLASS_WARRIOR = 1;
        public const int CLASS_MAGE = 2;
        public const int CLASS_ROGUE = 3;
        public const int CLASS_RANGER = 4;
        public const int CLASS_CLERIC = 5;

        public int itemRarity = -1;
        public int itemType = -1;
        public int forClass = -1;
        public string itemName = "Unnamed Item";
        public string description = "";
        public int itemID = -1;
        public int inventoryID = -1;

        public float successChance = 0;
        public int itemFind = 0;
        public int coinBonus = 0;
        public int xpBonus = 0;
        public float preventDeathBonus = 0.0f;

        public bool isActive = false;

        public Item()
        {
            
        }
    }
}
