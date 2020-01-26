namespace LobotJR.Modules.Dungeons
{
    public class Rewards
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
}
