namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Holds the data used to describe a fish.
    /// </summary>
    public class Fish
    {
        /// <summary>
        /// Size categories for fish.
        /// </summary>
        public enum Size
        {
            Unknown,
            Tiny,
            Small,
            Medium,
            Large,
            Huge
        }

        /// <summary>
        /// Database id.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The size category of this fish.
        /// </summary>
        public Size SizeCategory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rarity { get; set; } = -1;
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

        public override bool Equals(object obj)
        {
            var other = obj as Fish;
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
