using System.Collections.Concurrent;

namespace NeuralNetworkLib.Agents
{
    public struct BehaviourActions
    {
        /// <summary>
        /// Adds a behaviour (action) to the main thread execution list at a specified execution order.
        /// </summary>
        /// <param name="executionOrder">The order in which the behaviour should be executed.</param>
        /// <param name="behaviour">The behaviour (action) to be added.</param>
        public void AddMainThreadBehaviours(int executionOrder, Action behaviour)
        {
            if (MainThreadBehaviour == null)
                MainThreadBehaviour = new Dictionary<int, List<Action>>();

            if (MainThreadBehaviour.ContainsKey(executionOrder))
                MainThreadBehaviour[executionOrder].Add(behaviour);
            else
                MainThreadBehaviour.Add(executionOrder, new List<Action> { behaviour });
        }

        /// <summary>
        /// Adds a behaviour (action) to the multi-threaded execution list at a specified execution order.
        /// </summary>
        /// <param name="executionOrder">The order in which the behaviour should be executed.</param>
        /// <param name="behaviour">The behaviour (action) to be added.</param>
        public void AddMultiThreadableBehaviours(int executionOrder, Action behaviour)
        {
            if (MultiThreadablesBehaviour == null)
                MultiThreadablesBehaviour = new ConcurrentDictionary<int, ConcurrentBag<Action>>();

            if (MultiThreadablesBehaviour.ContainsKey(executionOrder))
                MultiThreadablesBehaviour[executionOrder].Add(behaviour);
            else
                MultiThreadablesBehaviour.TryAdd(executionOrder, new ConcurrentBag<Action> { behaviour });
        }

        /// <summary>
        /// Sets the transition behaviour, which is a single action to be executed at a transition point.
        /// </summary>
        /// <param name="behaviour">The behaviour (action) to be set as the transition action.</param>
        public void SetTransitionBehaviour(Action behaviour)
        {
            TransitionBehaviour = behaviour;
        }

        /// <summary>
        /// Gets or sets the dictionary that holds behaviours to be executed on the main thread, keyed by execution order.
        /// </summary>
        public Dictionary<int, List<Action>> MainThreadBehaviour { get; private set; }

        /// <summary>
        /// Gets or sets the concurrent dictionary that holds behaviours to be executed on multiple threads, keyed by execution order.
        /// </summary>
        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadablesBehaviour { get; private set; }

        /// <summary>
        /// Gets or sets the transition behaviour, which is a single action to be executed at a transition point.
        /// </summary>
        public Action TransitionBehaviour { get; private set; }
    }

    public abstract class State
    {
        /// <summary>
        /// An event that is triggered when a flag is set, passing an enum as the parameter.
        /// </summary>
        public Action<Enum> OnFlag;

        /// <summary>
        /// Gets the behaviour to be executed on each tick, optionally taking parameters.
        /// </summary>
        /// <param name="parameters">Optional parameters to customize the behaviour.</param>
        /// <returns>The behaviour action to be executed on each tick.</returns>
        public abstract BehaviourActions GetTickBehaviour(params object[] parameters);

        /// <summary>
        /// Gets the behaviour to be executed when entering a state, optionally taking parameters.
        /// </summary>
        /// <param name="parameters">Optional parameters to customize the behaviour.</param>
        /// <returns>The behaviour action to be executed when entering the state.</returns>
        public abstract BehaviourActions GetOnEnterBehaviour(params object[] parameters);

        /// <summary>
        /// Gets the behaviour to be executed when exiting a state, optionally taking parameters.
        /// </summary>
        /// <param name="parameters">Optional parameters to customize the behaviour.</param>
        /// <returns>The behaviour action to be executed when exiting the state.</returns>
        public abstract BehaviourActions GetOnExitBehaviour(params object[] parameters);
    }
}