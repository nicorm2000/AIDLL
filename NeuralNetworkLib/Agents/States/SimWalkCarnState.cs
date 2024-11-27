using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.States
{
    public class SimWalkCarnState : State
    {
        /// <summary>
        /// Returns the tick behavior for this state, performing actions based on the provided parameters.
        /// It checks the current state and executes behaviors like moving or attacking, depending on certain brain output conditions.
        /// </summary>
        /// <param name="parameters">An array of parameters used to determine the behavior, including node, target, actions, and brain outputs.</param>
        /// <returns>The behavior actions to be executed, or the default if conditions are not met.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            if (parameters.Length < 6) return default;

            if (parameters[0] is not SimNode<IVector> currentNode) return default;
            if (parameters[1] is not IVector target) return default;
            if (parameters[2] is not SimNodeType foodTarget) return default;
            if (parameters[3] is not Action onMove) return default;
            if (parameters[4] is not float[] outputBrain1) return default;
            if (parameters[5] is not float[] outputBrain2) return default;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onMove?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f && currentNode != null && currentNode.NodeType == foodTarget)
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }

                if (outputBrain2[0] > 0.5f && Approximatly(target, currentNode?.GetCoordinate(), 0.2f))
                {
                    OnFlag?.Invoke(Flags.OnAttack);
                    return;
                }
            });

            return behaviours;
        }

        /// <summary>
        /// Helper method that checks whether two vectors are approximately equal within a given tolerance.
        /// </summary>
        /// <param name="coord1">The first coordinate vector.</param>
        /// <param name="coord2">The second coordinate vector.</param>
        /// <param name="tolerance">The allowed tolerance for comparison.</param>
        /// <returns>True if the coordinates are within the tolerance range, false otherwise.</returns>
        private bool Approximatly(IVector coord1, IVector coord2, float tolerance)
        {
            if (coord1 == null || coord2 == null) return false;
            return Math.Abs(coord1.X - coord2.X) <= tolerance && Math.Abs(coord1.Y - coord2.Y) <= tolerance;
        }

        /// <summary>
        /// Returns the behavior actions to be executed when entering this state.
        /// Currently, this method returns the default behavior (no specific actions on enter).
        /// </summary>
        /// <param name="parameters">An array of parameters, though none are used in this case.</param>
        /// <returns>The default behavior actions.</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Returns the behavior actions to be executed when exiting this state.
        /// Currently, this method returns the default behavior (no specific actions on exit).
        /// </summary>
        /// <param name="parameters">An array of parameters, though none are used in this case.</param>
        /// <returns>The default behavior actions.</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}