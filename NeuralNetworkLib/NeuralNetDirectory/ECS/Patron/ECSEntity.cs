namespace NeuralNetworkLib.NeuralNetDirectory.ECS.Patron
{
    public class EcsEntity
    {
        private readonly List<Type> componentsType;
        private readonly List<Type> flagType;

        private readonly uint ID;

        /// <summary>
        /// Represents an entity in the ECS (Entity-Component-System) framework, managing its ID and associated component and flag types.
        /// </summary>
        public EcsEntity()
        {
            ID = EntityID.GetNew();
            componentsType = new List<Type>();
            flagType = new List<Type>();
        }

        /// <summary>
        /// Gets the unique identifier for this entity.
        /// </summary>
        /// <returns>The entity's ID as a uint.</returns>
        public uint GetID()
        {
            return ID;
        }

        /// <summary>
        /// Releases all resources used by this entity, including clearing component and flag types.
        /// </summary>
        public void Dispose()
        {
            componentsType.Clear();
            flagType.Clear();
        }

        /// <summary>
        /// Adds a component type to the entity.
        /// </summary>
        /// <typeparam name="ComponentType">The type of the component to be added, must inherit from <see cref="EcsComponent"/>.</typeparam>
        public void AddComponentType<ComponentType>() where ComponentType : EcsComponent
        {
            AddComponentType(typeof(ComponentType));
        }

        /// <summary>
        /// Adds a specific component type to the entity.
        /// </summary>
        /// <param name="ComponentType">The type of the component to be added.</param>
        public void AddComponentType(Type ComponentType)
        {
            componentsType.Add(ComponentType);
        }

        /// <summary>
        /// Checks if the entity contains a specific component type.
        /// </summary>
        /// <typeparam name="ComponentType">The type of the component to check.</typeparam>
        /// <returns>True if the entity contains the component, otherwise false.</returns>
        public bool ContainsComponentType<ComponentType>() where ComponentType : EcsComponent
        {
            return ContainsComponentType(typeof(ComponentType));
        }

        /// <summary>
        /// Checks if the entity contains a specific component type.
        /// </summary>
        /// <param name="ComponentType">The type of the component to check.</param>
        /// <returns>True if the entity contains the component, otherwise false.</returns>
        public bool ContainsComponentType(Type ComponentType)
        {
            return componentsType.Contains(ComponentType);
        }

        /// <summary>
        /// Removes a component type from the entity.
        /// </summary>
        /// <typeparam name="ComponentType">The type of the component to remove.</typeparam>
        public void RemoveComponentType<ComponentType>() where ComponentType : EcsComponent
        {
            componentsType.Remove(typeof(ComponentType));
        }

        /// <summary>
        /// Removes a specific component type from the entity.
        /// </summary>
        /// <param name="ComponentType">The type of the component to remove.</param>
        public void RemoveComponentType(Type ComponentType)
        {
            componentsType.Remove(ComponentType);
        }

        /// <summary>
        /// Adds a flag type to the entity.
        /// </summary>
        /// <typeparam name="FlagType">The type of the flag to be added, must inherit from <see cref="ECSFlag"/>.</typeparam>
        public void AddFlagType<FlagType>() where FlagType : ECSFlag
        {
            AddFlagType(typeof(FlagType));
        }

        /// <summary>
        /// Adds a flag type to the entity.
        /// </summary>
        /// <typeparam name="FlagType">The type of the flag to be added, must inherit from <see cref="ECSFlag"/>.</typeparam>
        public void AddFlagType(Type FlagType)
        {
            flagType.Add(FlagType);
        }

        /// <summary>
        /// Checks if the entity contains a specific flag type.
        /// </summary>
        /// <typeparam name="FlagType">The type of the flag to check.</typeparam>
        /// <returns>True if the entity contains the flag, otherwise false.</returns>
        public bool ContainsFlagType<FlagType>() where FlagType : ECSFlag
        {
            return ContainsFlagType(typeof(FlagType));
        }

        /// <summary>
        /// Checks if the entity contains a specific flag type.
        /// </summary>
        /// <param name="FlagType">The type of the flag to check.</param>
        /// <returns>True if the entity contains the flag, otherwise false.</returns>
        public bool ContainsFlagType(Type FlagType)
        {
            return flagType.Contains(FlagType);
        }

        /// <summary>
        /// Removes a flag type from the entity.
        /// </summary>
        /// <typeparam name="FlagType">The type of the flag to remove.</typeparam>
        public void RemoveFlagType<FlagType>() where FlagType : ECSFlag
        {
            flagType.Remove(typeof(FlagType));
        }

        /// <summary>
        /// Removes a specific flag type from the entity.
        /// </summary>
        /// <param name="FlagType">The type of the flag to remove.</param>
        public void RemoveFlagType(Type FlagType)
        {
            flagType.Remove(FlagType);
        }

        private class EntityID
        {
            private static uint LastEntityID;

            /// <summary>
            /// A static method to generate a new unique entity ID.
            /// </summary>
            /// <returns>A new unique entity ID.</returns>
            internal static uint GetNew()
            {
                return LastEntityID++;
            }
        }
    }
}