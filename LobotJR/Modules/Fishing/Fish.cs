namespace LobotJR.Modules.Fishing
{
    public class Fish
    {
        public const int SIZE_TINY = 1;
        public const int SIZE_SMALL = 2;
        public const int SIZE_MEDIUM = 3;
        public const int SIZE_LARGE = 4;
        public const int SIZE_HUGE = 5;

        public int sizeCategory = -1;
        public int ID = -1;
        public int rarity = -1;
        public string name = "";
        public float[] lengthRange = { -1, -1 };
        public float[] weightRange = { -1, -1 };
        public float length = -1;
        public float weight = -1;
        public string flavorText = "";
        public string caughtBy = "";

        public Fish()
        {

        }

        public Fish(Fish toCopy)
        {
            sizeCategory = toCopy.sizeCategory;
            ID = toCopy.ID;
            name = toCopy.name;
            lengthRange = toCopy.lengthRange;
            weightRange = toCopy.weightRange;
            length = toCopy.length;
            weight = toCopy.weight;
            flavorText = toCopy.flavorText;
            rarity = toCopy.rarity;
            caughtBy = toCopy.caughtBy;
        }
    }
}
