using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.States
{
    public class SimWalkState : State
    {
        /// <summary>
        /// Gets the tick behaviour for the state, performs actions based on parameters.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the tick behaviour.</param>
        /// <returns>Returns a BehaviourActions instance that represents the actions for the current tick.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            SimNode<IVector> currentNode = parameters[0] as SimNode<IVector>;
            SimNodeType foodTarget = (SimNodeType)parameters[1];
            Action onMove = parameters[2] as Action;
            float[] outputBrain1 = parameters[3] as float[];
            float[] outputBrain2 = parameters[4] as float[];

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove?.Invoke(); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(outputBrain1 == null || outputBrain2 == null) return;
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }
                SpecialAction(outputBrain2);
            });
            return behaviours;
        }

        /// <summary>
        /// Placeholder method for special actions, meant to be overridden in derived classes.
        /// </summary>
        /// <param name="outputs">The outputs to process for custom actions.</param>
        protected virtual void SpecialAction(float[] outputs)
        {
        }

        /// <summary>
        /// Returns the on-enter behaviour for the state.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the on-enter behaviour.</param>
        /// <returns>Returns the on-enter behaviour actions (currently default).</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Returns the on-exit behaviour for the state.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the on-exit behaviour.</param>
        /// <returns>Returns the on-exit behaviour actions (currently default).</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }

    public class SimWalkScavState : SimWalkState
    {
        /// Gets the tick behaviour for the state, determines actions based on position and food proximity.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the tick behaviour.</param>
        /// <returns>Returns a BehaviourActions instance representing the actions for the current tick.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            IVector position = parameters[0] as IVector;
            IVector nearestFood = parameters[1] as IVector;
            Action onMove = parameters[2] as Action;
            float[] outputBrain1 = parameters[3] as float[];
            IVector distanceToFood = new MyVector();
            IVector maxDistance = new MyVector(4, 4);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onMove?.Invoke();
                if (nearestFood == null || position == null) return;
                distanceToFood = new MyVector(nearestFood.X - position.X, nearestFood.Y - position.Y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(outputBrain1 == null) return;
                if (outputBrain1[0] > 0.5f && distanceToFood.Magnitude() < maxDistance.Magnitude())
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }
            });
            return behaviours;
        }
    }

    public class SimWalkHerbState : State
    {
        /// <summary>
        /// Gets the tick behaviour for the state, determines actions based on current node and food target.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the tick behaviour.</param>
        /// <returns>Returns a BehaviourActions instance representing the actions for the current tick.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            SimNode<IVector> currentNode = parameters[0] as SimNode<IVector>;
            SimNodeType foodTarget = (SimNodeType)parameters[1];
            Action onMove = parameters[2] as Action;
            float[] outputBrain1 = parameters[3] as float[];
            float[] outputBrain2 = parameters[4] as float[];

            behaviours.AddMultiThreadableBehaviours(0, () => { onMove?.Invoke(); });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(outputBrain1 == null || outputBrain2 == null) return;
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }

                if (outputBrain2[0] > 0.5f)
                {
                    OnFlag?.Invoke(Flags.OnEscape);
                    return;
                }
            });
            return behaviours;
        }

        /// <summary>
        /// Returns the on-enter behaviour for the state. Currently returns the default behaviour actions.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the on-enter behaviour.</param>
        /// <returns>Returns the on-enter behaviour actions (currently default).</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Returns the on-exit behaviour for the state. Currently returns the default behaviour actions.
        /// </summary>
        /// <param name="parameters">The parameters needed to determine the on-exit behaviour.</param>
        /// <returns>Returns the on-exit behaviour actions (currently default).</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}