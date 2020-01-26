namespace LobotJR.Modules.Pets
{
    public class Pet
    {
        public const int QUALITY_COMMON = 1;
        public const int QUALITY_UNCOMMON = 2;
        public const int QUALITY_RARE = 3;
        public const int QUALITY_EPIC = 4;
        public const int QUALITY_ARTIFACT = 5;

        public const int LEVEL_MAX = 10;
        public const int FEEDING_COST = 5;
        public const int HUNGER_MAX = 100;
        public const int XP_TO_LEVEL = 150;
        public const int FEEDING_AFFECTION = 5;
        public const int DUNGEON_HUNGER = 5;
        public const int DUNGEON_AFFECTION = 1;

        public int petRarity = -1;
        public string type = "Missing No";
        public string name = "Jimbo";
        public string size = "Fat";
        public string description = "";
        public string emote = "Kappa";
        public int ID = -1;
        public int stableID = -1;
        public bool isActive = false;
        public bool isSparkly = false;

        public int affection = 0;
        public int hunger = 100;
        public int xp = 0;
        public int level = 1;

        public float successChance = 0;
        public int itemFind = 0;
        public int coinBonus = 0;
        public int xpBonus = 0;
        public float preventDeathBonus = 0.0f;


        public Pet()
        {

        }

        public Pet(Pet copyPet)
        {
            petRarity = copyPet.petRarity;
            type = copyPet.type;
            name = copyPet.name;
            size = copyPet.size;
            description = copyPet.description;
            emote = copyPet.emote;
            ID = copyPet.ID;
            stableID = copyPet.stableID;
            isActive = copyPet.isActive;
            isSparkly = copyPet.isSparkly;
            affection = copyPet.affection;
            hunger = copyPet.hunger;
            xp = copyPet.xp;
            level = copyPet.level;
            successChance = copyPet.successChance;
            itemFind = copyPet.itemFind;
            coinBonus = copyPet.coinBonus;
            xpBonus = copyPet.xpBonus;
            preventDeathBonus = copyPet.preventDeathBonus;

        }

    }
}
