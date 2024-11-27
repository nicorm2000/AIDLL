namespace NeuralNetworkLib.NeuralNetDirectory.ECS.Patron
{
    [Flags]
    public enum FlagType
    {
        None = 0,
        Miner = 1 << 0,
        Caravan = 1 << 1,
        Target = 1 << 2,
        TypeD = 1 << 3,
        TypeE = 1 << 4
    }

    public class ECSFlag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ECSFlag"/> class with a specified flag type.
        /// </summary>
        /// <param name="flagType">The type of the flag to be assigned.</param>
        protected ECSFlag(FlagType flagType)
        {
            Flag = flagType;
        }

        public uint EntityOwnerID { get; set; } = 0;

        public FlagType Flag { get; set; }

        /// <summary>
        /// Releases the resources used by the current object.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}