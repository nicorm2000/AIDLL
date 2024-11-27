using System;

namespace NeuralNetworkLib.NeuralNetDirectory.ECS.Patron
{
    public abstract class EcsComponent
    {
        public uint EntityOwnerID { get; set; } = 0;

        /// <summary>
        /// Releases the resources used by the current object.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}