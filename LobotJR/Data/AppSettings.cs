namespace LobotJR.Data
{
    /// <summary>
    /// Settings that modify the behavior of the app.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The shortest time, in seconds, it can take to hook a fish.
        /// </summary>
        public int FishingCastMinimum { get; set; }
        /// <summary>
        /// The longest time, in seconds, it can take to hook a fish.
        /// </summary>        
        public int FishingCastMaximum { get; set; }
        /// <summary>
        /// How long, in seconds, a fish remains on the hook before it gets
        /// away.
        /// </summary>
        public int FishingHookLength { get; set; }
        /// <summary>
        /// Determines whether to use the weights associated with each fish
        /// rarity, or a standard normal distribution.
        /// </summary>
        public bool FishingUseWeights { get; set; }

        /// <summary>
        /// How long, in minutes, a fishing tournament should last.
        /// </summary>
        public int FishingTournamentDuration { get; set; }
        /// <summary>
        /// How long, in minutes, between the end of a tournament and the start
        /// of the next.
        /// </summary>
        public int FishingTournamentInterval { get; set; }
        /// <summary>
        /// The shortest time, in seconds, it can take to hook a fish during a
        /// tournament.
        /// </summary>
        public int FishingTournamentCastMinimum { get; set; }
        /// <summary>
        /// The longest time, in seconds, it can take to hook a fish during a
        /// tournament.
        /// </summary>        
        public int FishingTournamentCastMaximum { get; set; }
    }
}
