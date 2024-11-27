using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.States
{
    public class SimEatState : State
    {
        /// <summary>
        /// Retrieves the behavior actions based on the current simulation parameters.
        /// This method decides whether the object should eat or search for food based on the brain output and proximity to food.
        /// </summary>
        /// <param name="parameters">
        /// An array of objects containing the simulation data:
        /// 1. <see cref="SimNode{IVector}"/> current node where the object is located.
        /// 2. <see cref="SimNodeType"/> the type of food to search for.
        /// 3. <see cref="Action"/> the action to trigger when eating.
        /// 4. <see cref="float[]"/> the brain output influencing the eating decision.
        /// 5. <see cref="float[]"/> additional brain output influencing other behaviors (e.g., escape).
        /// </param>
        /// <returns>
        /// A <see cref="BehaviourActions"/> object representing the calculated actions for the current tick.
        /// </returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 5)
            {
                return default;
            }

            BehaviourActions behaviours = new BehaviourActions();
            SimNode<IVector> currentNode = parameters[0] as SimNode<IVector>;
            SimNodeType foodTarget = (SimNodeType)parameters[1];
            Action onEat = parameters[2] as Action;
            float[] outputBrain1 = parameters[3] as float[];
            float[] outputBrain2 = parameters[4] as float[];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (foodTarget == null || currentNode == null || onEat == null) return;
                if (currentNode.Food <= 0 || foodTarget != currentNode.NodeType) return;

                onEat?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (currentNode == null || currentNode.Food <= 0 || foodTarget != currentNode.NodeType)
                {
                    OnFlag?.Invoke(Flags.OnSearchFood);
                    return;
                }

                if (outputBrain1 != null && outputBrain1[0] > 0.5f && currentNode != null &&
                    currentNode.NodeType == foodTarget)
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }

                SpecialAction(outputBrain2);
            });

            return behaviours;
        }

        /// <summary>
        /// A special action to be performed based on the brain output. This can be overridden by subclasses to provide custom behavior.
        /// </summary>
        /// <param name="outputBrain2">An array of float values representing brain outputs for further decision-making.</param>
        protected virtual void SpecialAction(float[] outputBrain2)
        {
        }

        /// <summary>
        /// Called when entering a particular state. Currently not implemented.
        /// </summary>
        /// <param name="parameters">The parameters for the entry action, if any.</param>
        /// <returns>A default <see cref="BehaviourActions"/> object.</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Called when exiting a particular state. Currently not implemented.
        /// </summary>
        /// <param name="parameters">The parameters for the exit action, if any.</param>
        /// <returns>A default <see cref="BehaviourActions"/> object.</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }

    public class SimEatScavState : SimEatState
    {
        /// <summary>
        /// Retrieves the behavior actions based on the current simulation parameters.
        /// This method checks the distance to the food and triggers either an eating action or movement action
        /// depending on the conditions set for food presence, type, and distance.
        /// </summary>
        /// <param name="parameters">
        /// An array of objects containing the following simulation data:
        /// 1. <see cref="IVector"/> current position of the entity.
        /// 2. <see cref="SimNode{IVector}"/> target food node where food is located.
        /// 3. <see cref="Action"/> action to perform when the entity eats the food.
        /// 4. <see cref="Action"/> action to perform when the entity moves.
        /// 5. <see cref="float[]"/> brain output influencing the decision to eat or search for food.
        /// </param>
        /// <returns>
        /// A <see cref="BehaviourActions"/> object representing the calculated actions for the current tick.
        /// If conditions are not met, the entity will perform a movement action.
        /// </returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            if (parameters == null || parameters.Length != 5)
            {
                return default;
            }

            BehaviourActions behaviours = new BehaviourActions();
            IVector currentPos = parameters[0] as IVector;
            SimNode<IVector> foodNode = parameters[1] as SimNode<IVector>;
            Action onEat = parameters[2] as Action;
            Action onMove = parameters[3] as Action;
            float[] outputBrain1 = parameters[4] as float[];

            IVector distanceToFood = new MyVector();
            IVector maxDistance = new MyVector(4, 4);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentPos == null || foodNode == null || onEat == null || outputBrain1 == null)
                {
                    onMove?.Invoke();
                    return;
                }

                distanceToFood = new MyVector(foodNode.GetCoordinate().X - currentPos.X,
                    foodNode.GetCoordinate().Y - currentPos.Y);

                if (foodNode.Food <= 0 || foodNode.NodeType != SimNodeType.Carrion ||
                    distanceToFood.Magnitude() > maxDistance.Magnitude())
                {
                    onMove?.Invoke();
                    return;
                }

                onEat?.Invoke();
                onMove?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (foodNode == null || foodNode.Food <= 0 || distanceToFood.Magnitude() > maxDistance.Magnitude())
                {
                    OnFlag?.Invoke(Flags.OnSearchFood);
                    return;
                }

                if (outputBrain1 != null && outputBrain1[0] > 0.5f && currentPos != null &&
                    distanceToFood.Magnitude() <= maxDistance.Magnitude())
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }
            });

            return behaviours;
        }
    }

    public class SimEatHerbState : SimEatState
    {
        /// <summary>
        /// Executes a special action based on the provided brain output values.
        /// This method checks if the first value in the output array is greater than 0.5f, 
        /// and if so, triggers an escape flag.
        /// </summary>
        /// <param name="outputs">
        /// An array of floats representing the output from the brain model.
        /// The first value in the array determines whether the escape action is triggered.
        /// </param>
        protected override void SpecialAction(float[] outputs)
        {
            if (outputs != null && outputs[0] > 0.5f)
            {
                OnFlag?.Invoke(Flags.OnEscape);
                return;
            }
        }
    }

    public class SimEatCarnState : State
    {
        /// <summary>
        /// Retrieves the behavior actions based on the current simulation parameters.
        /// This method checks the current state of the entity and triggers actions based on certain conditions, 
        /// including the presence of food, the type of food, and the brain outputs affecting behavior.
        /// </summary>
        /// <param name="parameters">
        /// An array of objects containing the following simulation data:
        /// 1. <see cref="SimNode{IVector}"/> currentNode - The current node containing food data.
        /// 2. <see cref="SimNodeType"/> foodTarget - The type of food to compare against the current node.
        /// 3. <see cref="Action"/> onEat - The action to perform when the entity eats the food.
        /// 4. <see cref="float[]"/> outputBrain1 - Brain output affecting decision-making related to eating behavior.
        /// 5. <see cref="float[]"/> outputBrain2 - Additional brain output for triggering special actions like attacking.
        /// </param>
        /// <returns>
        /// A <see cref="BehaviourActions"/> object representing the actions to be performed during the tick.
        /// If conditions are met (food present, type matches, and brain output criteria), the entity will eat; 
        /// otherwise, it will search for food or perform special actions based on the outputs.
        /// </returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 5)
            {
                return default;
            }

            BehaviourActions behaviours = new BehaviourActions();
            if (parameters[0] is not SimNode<IVector> currentNode) return default;
            SimNodeType foodTarget = (SimNodeType)parameters[1];
            Action onEat = parameters[2] as Action;
            float[] outputBrain1 = parameters[3] as float[];
            float[] outputBrain2 = parameters[4] as float[];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (foodTarget == null || currentNode == null || onEat == null) return;
                if (currentNode.Food <= 0 || foodTarget != currentNode.NodeType) return;

                onEat?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (currentNode == null || currentNode.Food <= 0 || foodTarget != currentNode.NodeType)
                {
                    OnFlag?.Invoke(Flags.OnSearchFood);
                    return;
                }

                if (outputBrain1 != null && outputBrain1[0] > 0.5f && currentNode != null &&
                    currentNode.NodeType == foodTarget)
                {
                    OnFlag?.Invoke(Flags.OnEat);
                    return;
                }

                SpecialAction(outputBrain2);
            });

            return behaviours;
        }

        /// <summary>
        /// Executes a special action based on the brain outputs, such as triggering an attack flag when a certain threshold is met.
        /// </summary>
        /// <param name="outputs">
        /// An array of brain output values used to determine the action. If the first value exceeds the threshold, 
        /// an attack flag is triggered.
        /// </param>
        protected void SpecialAction(float[] outputs)
        {
            if (outputs != null && outputs[0] > 0.5f)
            {
                OnFlag?.Invoke(Flags.OnAttack);
                return;
            }
        }

        /// <summary>
        /// Defines the behavior when entering a state. Currently, this method does not implement any specific logic.
        /// </summary>
        /// <param name="parameters">
        /// An array of parameters required for entering the state, but this version doesn't utilize them.
        /// </param>
        /// <returns>
        /// A <see cref="BehaviourActions"/> object, which by default returns no specific behavior.
        /// </returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Defines the behavior when exiting a state. This method currently does not implement any specific exit logic.
        /// </summary>
        /// <param name="parameters">
        /// An array of parameters required for exiting the state, but this version doesn't utilize them.
        /// </param>
        /// <returns>
        /// A <see cref="BehaviourActions"/> object, which by default returns no specific behavior.
        /// </returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}