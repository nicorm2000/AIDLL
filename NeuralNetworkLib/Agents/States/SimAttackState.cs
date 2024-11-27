using NeuralNetworkLib.Agents.SimAgents;

namespace NeuralNetworkLib.Agents.States
{
    public class SimAttackState : State
    {
        /// <summary>
        /// Returns the tick behavior for this state, executing actions based on the provided parameters.
        /// The method invokes specific actions depending on the values of the brain outputs and flags. 
        /// It also schedules an attack action to run on a separate thread if the conditions are met.
        /// </summary>
        /// <param name="parameters">An array of parameters required to configure the tick behavior.</param>
        /// <returns>The behavior actions that will be executed.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 9)
            {
                return default;
            }
            
            BehaviourActions behaviours = new BehaviourActions();

            Action onAttack = parameters[5] as Action;
            float[] outputBrain1 = (float[])parameters[6];
            float[] outputBrain2 = (float[])parameters[7];
            float outputBrain3 = (float)parameters[8];

            if (outputBrain1 == null || outputBrain2 == null)
            {
                return default;
            }
            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onAttack?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (outputBrain1[0] > 0.5f)
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }

                if (outputBrain2[0] > 0.5f)
                {
                    OnFlag?.Invoke(Flags.OnAttack);
                    return;
                }

                if (outputBrain3 > 0.5f)
                {
                    OnFlag?.Invoke(Flags.OnSearchFood);
                    return;
                }
            });
            return behaviours;
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