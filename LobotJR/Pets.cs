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

namespace Companions
{
    class Pet
    {
        public const int QUALITY_COMMON = 1;
        public const int QUALITY_UNCOMMON = 2;
        public const int QUALITY_RARE = 3;
        public const int QUALITY_EPIC = 4;
        public const int QUALITY_ARTIFACT = 5;

        public const int LEVEL_MAX = 10;

        public int petRarity = -1;
        public string type = "Missing No";
        public string name = "Jimbo";
        public string size = "Fat";
        public string description = "";
        public string emote = "Kappa";
        public int ID = -1;
        public int stableID = -1;

        public int affection = 0;
        public int hunger = 100;
        public int level = 1;

        public float successChance = 0;
        public int itemFind = 0;
        public int coinBonus = 0;
        public int xpBonus = 0;
        public float preventDeathBonus = 0.0f;


        public Pet()
        {

        }
    }
}
