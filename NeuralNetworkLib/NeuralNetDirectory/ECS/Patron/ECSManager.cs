using System;
using System.Collections.Concurrent;

namespace NeuralNetworkLib.NeuralNetDirectory.ECS.Patron
{
    public static class ECSManager
    {
        private static readonly ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

        private static ConcurrentDictionary<uint, EcsEntity> entities;
        private static ConcurrentDictionary<Type, ConcurrentDictionary<uint, EcsComponent>> components;
        private static ConcurrentDictionary<Type, ConcurrentDictionary<uint, ECSFlag>> flags;
        private static ConcurrentDictionary<Type, ECSSystem> systems;

        /// <summary>
        /// Initializes the ECS system by setting up the dictionaries for entities, components, flags, and systems,
        /// and adds predefined systems such as <see cref="NeuralNetSystem"/>.
        /// </summary>
        public static void Init()
        {
            entities = new ConcurrentDictionary<uint, EcsEntity>();
            components = new ConcurrentDictionary<Type, ConcurrentDictionary<uint, EcsComponent>>();
            flags = new ConcurrentDictionary<Type, ConcurrentDictionary<uint, ECSFlag>>();
            systems = new ConcurrentDictionary<Type, ECSSystem>();

            //foreach (var classType in typeof(ECSSystem).Assembly.GetTypes())
            //    if (typeof(ECSSystem).IsAssignableFrom(classType) && !classType.IsAbstract)
            //        systems.TryAdd(classType, Activator.CreateInstance(classType) as ECSSystem);

            systems.TryAdd(typeof(NeuralNetSystem), Activator.CreateInstance(typeof(NeuralNetSystem)) as ECSSystem);
            
            foreach (KeyValuePair<Type, ECSSystem> system in systems) system.Value.Initialize();

            foreach (Type classType in typeof(EcsComponent).Assembly.GetTypes())
                if (typeof(EcsComponent).IsAssignableFrom(classType) && !classType.IsAbstract)
                    components.TryAdd(classType, new ConcurrentDictionary<uint, EcsComponent>());

            foreach (Type classType in typeof(ECSFlag).Assembly.GetTypes())
                if (typeof(ECSFlag).IsAssignableFrom(classType) && !classType.IsAbstract)
                    flags.TryAdd(classType, new ConcurrentDictionary<uint, ECSFlag>());
        }

        /// <summary>
        /// Updates all systems in parallel with the given delta time.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame, used for updating the systems.</param>
        public static void Tick(float deltaTime)
        {
            Parallel.ForEach(systems, parallelOptions, system => { system.Value.Run(deltaTime); });
        }

        /// <summary>
        /// Creates a new entity and returns its unique ID.
        /// </summary>
        /// <returns>The unique ID of the newly created entity.</returns>
        public static uint CreateEntity()
        {
            entities ??= new ConcurrentDictionary<uint, EcsEntity>();
            EcsEntity ecsEntity = new EcsEntity();
            entities.TryAdd(ecsEntity.GetID(), ecsEntity);
            return ecsEntity.GetID();
        }

        /// <summary>
        /// Adds a new system to the ECS system.
        /// </summary>
        /// <param name="system">The system to be added.</param>
        public static void AddSystem(ECSSystem system)
        {
            systems ??= new ConcurrentDictionary<Type, ECSSystem>();

            systems.TryAdd(system.GetType(), system);
        }

        /// <summary>
        /// Initializes all systems that have been added to the ECS system.
        /// </summary>
        public static void InitSystems()
        {
            foreach (KeyValuePair<Type, ECSSystem> system in systems) system.Value.Initialize();
        }

        /// <summary>
        /// Adds a new component type to the ECS system.
        /// </summary>
        /// <param name="component">The type of the component to add.</param>
        public static void AddComponentList(Type component)
        {
            components ??= new ConcurrentDictionary<Type, ConcurrentDictionary<uint, EcsComponent>>();
            components.TryAdd(component, new ConcurrentDictionary<uint, EcsComponent>());
        }

        /// <summary>
        /// Adds a component of the specified type to the specified entity.
        /// </summary>
        /// <typeparam name="TComponentType">The type of the component.</typeparam>
        /// <param name="entityID">The ID of the entity to which the component will be added.</param>
        /// <param name="component">The component to be added.</param>
        public static void AddComponent<TComponentType>(uint entityID, TComponentType component)
            where TComponentType : EcsComponent
        {
            component.EntityOwnerID = entityID;
            entities[entityID].AddComponentType(typeof(TComponentType));
            components[typeof(TComponentType)].TryAdd(entityID, component);
        }

        /// <summary>
        /// Checks if the specified entity has the specified component type.
        /// </summary>
        /// <typeparam name="TComponentType">The type of the component.</typeparam>
        /// <param name="entityID">The ID of the entity to check.</param>
        /// <returns>True if the entity contains the component, false otherwise.</returns>
        public static bool ContainsComponent<TComponentType>(uint entityID) where TComponentType : EcsComponent
        {
            return entities[entityID].ContainsComponentType<TComponentType>();
        }

        /// <summary>
        /// Retrieves a collection of entity IDs that contain all the specified component types.
        /// </summary>
        /// <param name="componentTypes">The component types to check for.</param>
        /// <returns>A collection of entity IDs that have all the specified components.</returns>
        public static IEnumerable<uint> GetEntitiesWithComponentTypes(params Type[] componentTypes)
        {
            ConcurrentBag<uint> matchs = new ConcurrentBag<uint>();
            Parallel.ForEach(entities, parallelOptions, entity =>
            {
                for (int i = 0; i < componentTypes.Length; i++)
                    if (!entity.Value.ContainsComponentType(componentTypes[i]))
                        return;

                matchs.Add(entity.Key);
            });
            return matchs;
        }

        /// <summary>
        /// Retrieves all components of the specified type.
        /// </summary>
        /// <typeparam name="TComponentType">The type of the components to retrieve.</typeparam>
        /// <returns>A dictionary of entity IDs and their corresponding components of the specified type.</returns>
        public static ConcurrentDictionary<uint, TComponentType> GetComponents<TComponentType>()
            where TComponentType : EcsComponent
        {
            if (!components.ContainsKey(typeof(TComponentType))) return null;

            ConcurrentDictionary<uint, TComponentType> comps = new ConcurrentDictionary<uint, TComponentType>();

            Parallel.ForEach(components[typeof(TComponentType)], parallelOptions,
                component => { comps.TryAdd(component.Key, component.Value as TComponentType); });

            return comps;
        }

        /// <summary>
        /// Retrieves a specific component of the specified type for the given entity.
        /// </summary>
        /// <typeparam name="TComponentType">The type of the component.</typeparam>
        /// <param name="entityID">The ID of the entity to retrieve the component for.</param>
        /// <returns>The component of the specified type for the given entity.</returns>
        public static TComponentType GetComponent<TComponentType>(uint entityID) where TComponentType : EcsComponent
        {
            return components[typeof(TComponentType)][entityID] as TComponentType;
        }

        /// <summary>
        /// Removes a component of the specified type from the given entity.
        /// </summary>
        /// <typeparam name="TComponentType">The type of the component.</typeparam>
        /// <param name="entityID">The ID of the entity to remove the component from.</param>
        public static void RemoveComponent<TComponentType>(uint entityID) where TComponentType : EcsComponent
        {
            components[typeof(TComponentType)].TryRemove(entityID, out _);
        }

        /// <summary>
        /// Retrieves a collection of entity IDs that contain all the specified flag types.
        /// </summary>
        /// <param name="flagTypes">The flag types to check for.</param>
        /// <returns>A collection of entity IDs that have all the specified flags.</returns>
        public static IEnumerable<uint> GetEntitiesWhitFlagTypes(params Type[] flagTypes)
        {
            ConcurrentBag<uint> matchs = new ConcurrentBag<uint>();
            Parallel.ForEach(entities, parallelOptions, entity =>
            {
                for (int i = 0; i < flagTypes.Length; i++)
                    if (!entity.Value.ContainsFlagType(flagTypes[i]))
                        return;

                matchs.Add(entity.Key);
            });
            return matchs;
        }

        /// <summary>
        /// Adds a flag of the specified type to the specified entity.
        /// </summary>
        /// <typeparam name="TFlagType">The type of the flag.</typeparam>
        /// <param name="entityID">The ID of the entity to add the flag to.</param>
        /// <param name="flag">The flag to be added.</param>
        public static void AddFlag<TFlagType>(uint entityID, TFlagType flag)
            where TFlagType : ECSFlag
        {
            flag.EntityOwnerID = entityID;
            entities[entityID].AddComponentType(typeof(TFlagType));
            flags[typeof(TFlagType)].TryAdd(entityID, flag);
        }

        /// <summary>
        /// Checks if the specified entity has the specified flag type.
        /// </summary>
        /// <typeparam name="TFlagType">The type of the flag.</typeparam>
        /// <param name="entityID">The ID of the entity to check.</param>
        /// <returns>True if the entity contains the flag, false otherwise.</returns>
        public static bool ContainsFlag<TFlagType>(uint entityID) where TFlagType : ECSFlag
        {
            return entities[entityID].ContainsFlagType<TFlagType>();
        }

        /// <summary>
        /// Retrieves all flags of the specified type.
        /// </summary>
        /// <typeparam name="TFlagType">The type of the flags to retrieve.</typeparam>
        /// <returns>A dictionary of entity IDs and their corresponding flags of the specified type.</returns>
        public static ConcurrentDictionary<uint, TFlagType> GetFlags<TFlagType>() where TFlagType : ECSFlag
        {
            if (!flags.ContainsKey(typeof(TFlagType))) return null;

            ConcurrentDictionary<uint, TFlagType> flgs = new ConcurrentDictionary<uint, TFlagType>();

            Parallel.ForEach(flags[typeof(TFlagType)], parallelOptions,
                flag => { flgs.TryAdd(flag.Key, flag.Value as TFlagType); });

            return flgs;
        }

        /// <summary>
        /// Retrieves a specific flag of the specified type for the given entity.
        /// </summary>
        /// <typeparam name="TFlagType">The type of the flag.</typeparam>
        /// <param name="entityID">The ID of the entity to retrieve the flag for.</param>
        /// <returns>The flag of the specified type for the given entity.</returns>
        public static TFlagType GetFlag<TFlagType>(uint entityID) where TFlagType : ECSFlag
        {
            return flags[typeof(TFlagType)][entityID] as TFlagType;
        }

        /// <summary>
        /// Removes a flag of the specified type from the given entity.
        /// </summary>
        /// <typeparam name="TFlagType">The type of the flag.</typeparam>
        /// <param name="entityID">The ID of the entity to remove the flag from.</param>
        public static void RemoveFlag<TFlagType>(uint entityID) where TFlagType : ECSFlag
        {
            flags[typeof(TFlagType)].TryRemove(entityID, out _);
        }


        /// <summary>
        /// Removes the specified entity and its associated components and flags from the ECS system.
        /// </summary>
        /// <param name="agentId">The ID of the entity to remove.</param>
        public static void RemoveEntity(uint agentId)
        {
            entities.TryRemove(agentId, out _);
            foreach (KeyValuePair<Type, ConcurrentDictionary<uint, EcsComponent>> component in components)
                component.Value.TryRemove(agentId, out _);
            foreach (KeyValuePair<Type, ConcurrentDictionary<uint, ECSFlag>> flag in flags)
                flag.Value.TryRemove(agentId, out _);
        }

        /// <summary>
        /// Retrieves a system of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the system to retrieve.</typeparam>
        /// <returns>The system of the specified type.</returns>
        public static ECSSystem GetSystem<T>()
        {
            return systems[typeof(T)];
        }
    }
}