using LobotJR.Data;

namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Holds the data used to describe a fish.
    /// </summary>
    public class Fish : TableObject
    {
        /// <summary>
        /// The size category of this fish.
        /// </summary>
        public FishSize SizeCategory { get; set; }
        /// <summary>
        /// Rarity category of the fish.
        /// </summary>
        public FishRarity Rarity { get; set; }
        /// <summary>
        /// The display name of the fish.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The minimum length of this fish.
        /// </summary>
        public float MinimumLength { get; set; }
        /// <summary>
        /// The maximum length of this fish.
        /// </summary>
        public float MaximumLength { get; set; }
        /// <summary>
        /// The minimum weight of this fish.
        /// </summary>
        public float MinimumWeight { get; set; }
        /// <summary>
        /// The maximum weight of this fish.
        /// </summary>
        public float MaximumWeight { get; set; }
        /// <summary>
        /// The flavor text given to describe the fish.
        /// </summary>
        public string FlavorText { get; set; }
    }

    public class FishRarity : TableObject
    {
        /// <summary>
        /// The name that represents this level of rarity.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The relative weight of this rarity.
        /// </summary>
        public float Weight { get; set; }
    }

    public class FishSize : TableObject
    {
        /// <summary>
        /// The name that represents this size category.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Message given to user when a fish of this size is hooked.
        /// </summary>
        public string Message { get; set; }
    }
}

/**
* TODO: seed the database with these sizes
Tiny: "You feel a light tug at your line! Type !catch to reel it in!"
Small: "Something nibbles at your bait! Type !catch to reel it in!"
Medium: "A strong tug snags your bait! Type !catch to reel it in!"
Large: "Whoa! Something big grabs your line! Type !catch to reel it in!"
Huge: "You're almost pulled into the water! Something HUGE is hooked! Type !catch to reel it in!"
*/