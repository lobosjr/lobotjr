namespace LobotJR.Modules.Fishing.Model
{
    /// <summary>
    /// Data that describes an instance of a fish being caught.
    /// </summary>
    public class Catch
    {
        /// <summary>
        /// The user that caught the fish.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The fish that was caught.
        /// </summary>
        public Fish Fish { get; set; }
        /// <summary>
        /// The length of the fish.
        /// </summary>
        public float Length { get; set; }
        /// <summary>
        /// The weight of the fish.
        /// </summary>
        public float Weight { get; set; }
        /// <summary>
        /// The amount of points the fish is worth in a tournament.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Copies the record values from another catch object into this one.
        /// </summary>
        /// <param name="other">Another catch object.</param>
        public void CopyFrom(Catch other)
        {
            Length = other.Length;
            Weight = other.Weight;
            Points = other.Points;
        }
    }
}
