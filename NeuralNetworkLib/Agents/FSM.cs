namespace NeuralNetworkLib.Agents
{
    public class FSM<EnumState, EnumFlag>
        where EnumState : Enum
        where EnumFlag : Enum
    {
        private const int UNNASIGNED_TRANSITION = -1;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnEnterParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnExitParameters;
        private readonly Dictionary<int, State> _behaviours;
        private readonly Dictionary<int, Func<object[]>> _behaviourTickParameters;
        private readonly (int destinationInState, Action onTransition)[,] _transitions;
        private int _currentState;

        /// <summary>
        /// Initializes the system, setting the maximum degree of parallelism for processing.
        /// </summary>
        private readonly ParallelOptions parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 32
        };

        /// <summary>
        /// Initializes a new instance of the FSM (Finite State Machine) class. Sets up dictionaries for behaviors, transitions, 
        /// and behavior parameters for onEnter, onExit, and tick actions. It also initializes the transition array with default values.
        /// </summary>
        public FSM()
        {
            int states = Enum.GetValues(typeof(EnumState)).Length;
            int flags = Enum.GetValues(typeof(EnumFlag)).Length;
            _behaviours = new Dictionary<int, State>();
            _transitions = new (int, Action)[states, flags];

            for (int i = 0; i < states; i++)
            for (int j = 0; j < flags; j++)
                _transitions[i, j] = (UNNASIGNED_TRANSITION, null);

            _behaviourTickParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
        }

        /// <summary>
        /// Retrieves the behaviors to be executed when entering the current state, based on the state and any parameters passed.
        /// </summary>
        private BehaviourActions GetCurrentStateOnEnterBehaviours => _behaviours[_currentState].GetOnEnterBehaviour(_behaviourOnEnterParameters[_currentState]?.Invoke());

        /// <summary>
        /// Retrieves the behaviors to be executed when exiting the current state, based on the state and any parameters passed.
        /// </summary>
        private BehaviourActions GetCurrentStateOnExitBehaviours => _behaviours[_currentState].GetOnExitBehaviour(_behaviourOnExitParameters[_currentState]?.Invoke());

        /// <summary>
        /// Retrieves the behaviors to be executed during the tick phase for the current state, based on the state and any parameters passed.
        /// </summary>
        private BehaviourActions GetCurrentStateTickBehaviours => _behaviours[_currentState].GetTickBehaviour(_behaviourTickParameters[_currentState]?.Invoke());

        /// <summary>
        /// Forces a state transition by executing the exit behaviors of the current state, updating the state to the new one, 
        /// and executing the enter behaviors of the new state.
        /// </summary>
        /// <param name="state">The target state to transition to.</param>
        public void ForceTransition(Enum state)
        {
            ExecuteBehaviour(GetCurrentStateOnExitBehaviours);
            _currentState = Convert.ToInt32(state);
            ExecuteBehaviour(GetCurrentStateOnEnterBehaviours);
        }

        /// <summary>
        /// Adds a new behavior to a state, which includes the onTick, onEnter, and onExit behaviors along with their parameters.
        /// The behavior is only added if the state index does not already exist in the FSM.
        /// </summary>
        /// <typeparam name="T">The type of the state behavior to be added.</typeparam>
        /// <param name="stateIndexEnum">The enum representing the state index to which the behavior is added.</param>
        /// <param name="onTickParameters">Optional parameters for the onTick behavior.</param>
        /// <param name="onEnterParameters">Optional parameters for the onEnter behavior.</param>
        /// <param name="onExitParameters">Optional parameters for the onExit behavior.</param>
        public void AddBehaviour<T>(EnumState stateIndexEnum, Func<object[]> onTickParameters = null,
            Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
        {
            int stateIndex = Convert.ToInt32(stateIndexEnum);

            if (_behaviours.ContainsKey(stateIndex)) return;

            State newBehaviour = new T();
            newBehaviour.OnFlag += Transition;
            _behaviours.Add(stateIndex, newBehaviour);
            _behaviourTickParameters.Add(stateIndex, onTickParameters);
            _behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
            _behaviourOnExitParameters.Add(stateIndex, onExitParameters);
        }

        /// <summary>
        /// Sets a transition from one state to another, triggered by a flag. Optionally, a transition action can be provided.
        /// </summary>
        /// <param name="originState">The state from which the transition originates.</param>
        /// <param name="flag">The flag that triggers the transition.</param>
        /// <param name="destinationState">The target state to transition to.</param>
        /// <param name="onTransition">An optional action to execute during the transition.</param>
        public void SetTransition(Enum originState, Enum flag, Enum destinationState, Action onTransition = null)
        {
            _transitions[Convert.ToInt32(originState), Convert.ToInt32(flag)] =
                (Convert.ToInt32(destinationState), onTransition);
        }

        /// <summary>
        /// Executes the transition based on the given flag. This method handles exiting the current state, invoking any transition 
        /// action, and entering the new state. If no transition is defined, the method does nothing.
        /// </summary>
        /// <param name="flag">The flag that triggers the transition.</param>
        private void Transition(Enum flag)
        {
            if (_transitions[_currentState, Convert.ToInt32(flag)].destinationInState == UNNASIGNED_TRANSITION) return;

            ExecuteBehaviour(GetCurrentStateOnExitBehaviours);

            _transitions[_currentState, Convert.ToInt32(flag)].onTransition?.Invoke();

            int currentState = _transitions[_currentState, Convert.ToInt32(flag)].destinationInState;
            if (currentState == UNNASIGNED_TRANSITION) return;
            _currentState = _transitions[_currentState, Convert.ToInt32(flag)].destinationInState;

            ExecuteBehaviour(GetCurrentStateOnEnterBehaviours);
        }

        /// <summary>
        /// Executes the tick behavior for the current state if behaviors for the state are defined.
        /// </summary>
        public void Tick()
        {
            if (!_behaviours.ContainsKey(_currentState)) return;

            ExecuteBehaviour(GetCurrentStateTickBehaviours);
        }

        /// <summary>
        /// Executes the behaviors associated with the provided <see cref="BehaviourActions"/> object, either on the main thread 
        /// or using multiple threads based on the <paramref name="multi"/> flag.
        /// </summary>
        /// <param name="behaviourActions">The actions to execute.</param>
        /// <param name="multi">Whether to execute the behaviors on multiple threads (default is false).</param>
        private void ExecuteBehaviour(BehaviourActions behaviourActions, bool multi = false)
        {
            if (behaviourActions.Equals(default(BehaviourActions))) return;

            int executionOrder = 0;

            while ((behaviourActions.MainThreadBehaviour != null && behaviourActions.MainThreadBehaviour.Count > 0) ||
                   (behaviourActions.MultiThreadablesBehaviour != null &&
                    behaviourActions.MultiThreadablesBehaviour.Count > 0))
            {
                if (multi)
                {
                    ExecuteMultiThreadBehaviours(behaviourActions, executionOrder);
                }
                else
                {
                    ExecuteMainThreadBehaviours(behaviourActions, executionOrder);
                }

                executionOrder++;
            }

            behaviourActions.TransitionBehaviour?.Invoke();
        }

        /// <summary>
        /// Executes the behaviors associated with the provided <see cref="BehaviourActions"/> object for a specific execution order,
        /// either on the main thread or using multiple threads based on the <paramref name="multi"/> flag.
        /// </summary>
        /// <param name="behaviourActions">The actions to execute.</param>
        /// <param name="executionOrder">The order in which to execute the behaviors.</param>
        /// <param name="multi">Whether to execute the behaviors on multiple threads (default is false).</param>
        public void ExecuteBehaviour(BehaviourActions behaviourActions, int executionOrder, bool multi = false)
        {
            if (multi)
            {
                ExecuteMultiThreadBehaviours(behaviourActions, executionOrder);
            }
            else
            {
                ExecuteMainThreadBehaviours(behaviourActions, executionOrder);
            }

            behaviourActions.TransitionBehaviour?.Invoke();
        }

        /// <summary>
        /// Returns the count of behaviors that need to be executed on the main thread for the current state.
        /// </summary>
        /// <returns>The number of main thread behaviors to execute.</returns>
        public int GetMainThreadCount()
        {
            BehaviourActions currentStateBehaviours = GetCurrentStateTickBehaviours;
            if (currentStateBehaviours.MainThreadBehaviour == null)
            {
                return 0;
            }

            return currentStateBehaviours.MainThreadBehaviour.Count;
        }

        /// <summary>
        /// Returns the count of behaviors that need to be executed on multiple threads for the current state.
        /// </summary>
        /// <returns>The number of multi-threaded behaviors to execute.</returns>
        public int GetMultiThreadCount()
        {
            BehaviourActions currentStateBehaviours = GetCurrentStateTickBehaviours;
            return currentStateBehaviours.MultiThreadablesBehaviour == null ? 0 : currentStateBehaviours.MultiThreadablesBehaviour.Count;
        }

        /// <summary>
        /// Returns the count of behaviors that need to be executed on multiple threads for the current state.
        /// </summary>
        /// <returns>The number of multi-threaded behaviors to execute.</returns>
        public void ExecuteMainThreadBehaviours(BehaviourActions behaviourActions, int executionOrder)
        {
            if (behaviourActions.MainThreadBehaviour != null)
            {
                if (behaviourActions.MainThreadBehaviour.ContainsKey(executionOrder))
                {
                    foreach (Action action in behaviourActions.MainThreadBehaviour[executionOrder])
                    {
                        action.Invoke();
                    }

                    behaviourActions.MainThreadBehaviour.Remove(executionOrder);
                }
            }
        }

        /// <summary>
        /// Executes the behaviors associated with the provided <see cref="BehaviourActions"/> object on multiple threads for a specific execution order.
        /// </summary>
        /// <param name="behaviourActions">The actions to execute.</param>
        /// <param name="executionOrder">The order in which to execute the behaviors.</param>
        public void ExecuteMultiThreadBehaviours(BehaviourActions behaviourActions, int executionOrder)
        {
            if (behaviourActions.MultiThreadablesBehaviour == null) return;
            if (!behaviourActions.MultiThreadablesBehaviour.ContainsKey(executionOrder)) return;

            Parallel.ForEach(behaviourActions.MultiThreadablesBehaviour, parallelOptions, behaviour =>
            {
                foreach (Action action in behaviour.Value)
                {
                    action.Invoke();
                }
            });

            behaviourActions.MultiThreadablesBehaviour.TryRemove(executionOrder, out _);
        }

        /// <summary>
        /// Executes the tick behavior for the current state using multiple threads for the specified execution order.
        /// </summary>
        /// <param name="executionOrder">The order in which to execute the tick behavior.</param>
        public void MultiThreadTick(int executionOrder)
        {
            if (!_behaviours.ContainsKey(_currentState)) return;

            ExecuteBehaviour(GetCurrentStateTickBehaviours, executionOrder, true);
        }

        /// <summary>
        /// Executes the tick behavior for the current state on the main thread for the specified execution order.
        /// </summary>
        /// <param name="executionOrder">The order in which to execute the tick behavior.</param>
        public void MainThreadTick(int executionOrder)
        {
            if (!_behaviours.ContainsKey(_currentState)) return;

            ExecuteBehaviour(GetCurrentStateTickBehaviours, executionOrder, false);
        }
    }
}