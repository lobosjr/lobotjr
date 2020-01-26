namespace LobotJR.Modules.Classes
{
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
