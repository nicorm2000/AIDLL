using NeuralNetworkLib.Agents.States;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.SimAgents
{
    public enum Flags
    {
        OnEscape,
        OnEat,
        OnSearchFood,
        OnAttack
    }

    public class SimAgent<TVector, TTransform>
        where TVector : IVector, IEquatable<TVector>
        where TTransform : ITransform<IVector>, new()
    {
        public enum Behaviours
        {
            Walk,
            Escape,
            Eat,
            Attack
        }

        /// <summary>
        /// Gets or sets the transform of the agent.
        /// </summary>
        /// <value>
        /// The agent's transform.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the value to set the transform is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the position of the transform is null.
        /// </exception>
        public virtual TTransform Transform
        {
            get => transform;
            set
            {
                transform ??= new TTransform();
                transform.position ??= new MyVector(0, 0);

                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Transform value cannot be null");
                }

                if (transform.position == null || value.position == null)
                {
                    throw new InvalidOperationException("Transform positions cannot be null");
                }

                transform.forward = (transform.position - value.position).Normalized();
                transform = value;
            }
        }

        public virtual INode<IVector> CurrentNode =>
            DataContainer.graph.NodesType[(int)Transform.position.X, (int)Transform.position.Y];

        public static Action<SimAgent<TVector, TTransform>> OnDeath;
        protected TTransform transform = new TTransform();
        public virtual bool CanReproduce => Food >= FoodLimit;
        public SimAgentTypes agentType { get; set; }
        public FSM<Behaviours, Flags> Fsm;

        protected int movement = 3;
        protected SimNodeType foodTarget;
        public int FoodLimit { get; protected set; } = 5;
        public int Food { get; protected set; } = 0;
        protected Action OnMove;
        protected Action OnEat;
        protected float dt;
        protected const int NoTarget = -1;

        Genome[] genomes;
        public float[][] output;
        public float[][] input;
        public Dictionary<int, BrainType> brainTypes = new Dictionary<int, BrainType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimAgent"/> class.
        /// </summary>
        public SimAgent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimAgent"/> class with a specified agent type.
        /// </summary>
        /// <param name="agentType">The type of agent to initialize.</param>
        public SimAgent(SimAgentTypes agentType)
        {
            this.agentType = agentType;
        }

        /// <summary>
        /// Initializes the agent, setting up the FSM and brain outputs.
        /// </summary>
        public virtual void Init()
        {
            Food = 0;
            Fsm = new FSM<Behaviours, Flags>();
            output = new float[brainTypes.Count][];
            foreach (BrainType brain in brainTypes.Values)
            {
                NeuronInputCount inputsCount =
                    DataContainer.InputCountCache[(brain, agentType)];
                output[GetBrainTypeKeyByValue(brain)] = new float[inputsCount.outputCount];
            }

            OnMove += Move;
            OnEat += Eat;

            FsmBehaviours();

            FsmTransitions();
            Fsm.ForceTransition(Behaviours.Walk);
        }

        /// <summary>
        /// Resets the agent's state, setting food to zero and forcing the FSM to transition to the "Walk" state.
        /// </summary>
        public virtual void Reset()
        {
            Food = 0;
            Fsm.ForceTransition(Behaviours.Walk);
            CalculateInputs();
        }

        /// <summary>
        /// Calculates the inputs for the agent's brain based on the current agent type and brain types.
        /// </summary>
        protected void CalculateInputs()
        {
            int brainTypesCount = brainTypes.Count;
            input = new float[brainTypesCount][];
            output = new float[brainTypesCount][];

            for (int i = 0; i < brainTypesCount; i++)
            {
                BrainType brainType = brainTypes[i];
                input[i] = new float[GetInputCount(brainType)];
                int outputCount = DataContainer.InputCountCache[(brainType, agentType)].outputCount;
                output[i] = new float[outputCount];
            }
        }

        /// <summary>
        /// Uninitializes the agent by removing event handlers.
        /// </summary>
        public virtual void Uninit()
        {
            OnMove -= Move;
            OnEat -= Eat;
        }

        /// <summary>
        /// Ticks the FSM with the given delta time.
        /// </summary>
        /// <param name="deltaTime">The delta time to use for the tick.</param>
        public void Tick(float deltaTime)
        {
            dt = deltaTime;
            Fsm.Tick();
        }

        /// <summary>
        /// Updates the agent's inputs by finding food, updating movement, and adding extra inputs.
        /// </summary>
        public virtual void UpdateInputs()
        {
            FindFoodInputs();
            MovementInputs();
            ExtraInputs();
        }

        /// <summary>
        /// Finds food-related inputs for the agent's brain.
        /// </summary>
        protected void FindFoodInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Eat);
            int inputCount = GetInputCount(BrainType.Eat);
            input[brain] = new float[inputCount];

            input[brain][0] = Transform.position.X;
            input[brain][1] = Transform.position.Y;
            INode<IVector> target = GetTarget(foodTarget);

            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
                return;
            }

            input[brain][2] = target.GetCoordinate().X;
            input[brain][3] = target.GetCoordinate().Y;
        }

        /// <summary>
        /// Handles the movement-related inputs for the agent's brain.
        /// </summary>
        protected virtual void MovementInputs()
        {
        }

        /// <summary>
        /// Handles additional inputs for the agent's brain that may be needed.
        /// </summary>
        protected virtual void ExtraInputs()
        {
        }

        /// <summary>
        /// Sets up the transitions for the agent's FSM, such as walking and eating transitions.
        /// </summary>
        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            EatTransitions();
            ExtraTransitions();
        }

        /// <summary>
        /// Defines the transitions for the walking state in the FSM.
        /// </summary>
        protected virtual void WalkTransitions()
        {
        }

        /// <summary>
        /// Defines the transitions for the eating state in the FSM.
        /// </summary>
        protected virtual void EatTransitions()
        {
        }

        /// <summary>
        /// Defines additional transitions for other states in the FSM.
        /// </summary>
        protected virtual void ExtraTransitions()
        {
        }

        /// <summary>
        /// Sets up the behaviours for the agent's FSM, such as walking and extra behaviours.
        /// </summary>
        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkState>(Behaviours.Walk, WalkTickParameters);
            ExtraBehaviours();
        }

        /// <summary>
        /// Defines extra behaviours for the agent's FSM.
        /// </summary>
        protected virtual void ExtraBehaviours()
        {
        }

        /// <summary>
        /// Provides the parameters required for walking state in the FSM.
        /// </summary>
        /// <returns>Parameters for the walking state.</returns>
        protected virtual object[] WalkTickParameters()
        {
            int extraBrain = agentType == SimAgentTypes.Carnivore
                ? GetBrainTypeKeyByValue(BrainType.Attack)
                : GetBrainTypeKeyByValue(BrainType.Escape);
            object[] objects =
            {
                CurrentNode, foodTarget, OnMove, output[GetBrainTypeKeyByValue(BrainType.Eat)],
                output[extraBrain]
            };
            return objects;
        }

        /// <summary>
        /// Provides the parameters required for eating state in the FSM.
        /// </summary>
        /// <returns>Parameters for the eating state.</returns>
        protected virtual object[] EatTickParameters()
        {
            int extraBrain = agentType == SimAgentTypes.Carnivore
                ? GetBrainTypeKeyByValue(BrainType.Attack)
                : GetBrainTypeKeyByValue(BrainType.Escape);

            object[] objects =
                { CurrentNode, foodTarget, OnEat, output[GetBrainTypeKeyByValue(BrainType.Eat)], output[extraBrain] };
            return objects;
        }

        /// <summary>
        /// Handles the agent eating food, updating the food count and the current node type.
        /// </summary>
        protected virtual void Eat()
        {
            INode<IVector> currNode = CurrentNode;
            lock (currNode)
            {
                if (currNode.Food <= 0) return;
                Food++;
                currNode.Food--;
                if (currNode.Food <= 0) currNode.NodeType = SimNodeType.Empty;
            }
        }

        /// <summary>
        /// Moves the agent based on its brain output and updates its position.
        /// </summary>
        protected virtual void Move()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Movement);

            IVector currentPos = new MyVector(CurrentNode.GetCoordinate().X, CurrentNode.GetCoordinate().Y);
            currentPos = CalculateNewPosition(currentPos, output[brain]);

            if (!DataContainer.graph.IsWithinGraphBorders(currentPos))
            {
                if (currentPos.X <= DataContainer.graph.MinX)
                {
                    currentPos.X = DataContainer.graph.MaxX - 1;
                }

                if (currentPos.X >= DataContainer.graph.MaxX)
                {
                    currentPos.X = DataContainer.graph.MinX + 1;
                }

                if (currentPos.Y <= DataContainer.graph.MinY)
                {
                    currentPos.Y = DataContainer.graph.MaxY - 1;
                }

                if (currentPos.Y >= DataContainer.graph.MaxY)
                {
                    currentPos.Y = DataContainer.graph.MinY + 1;
                }
            }

            INode<IVector> newPos = DataContainer.CoordinateToNode(currentPos);
            if (newPos != null) SetPosition(newPos.GetCoordinate());
        }

        /// <summary>
        /// Calculates the new position of the agent based on the brain output and current position.
        /// </summary>
        /// <param name="targetPos">The current position of the agent.</param>
        /// <param name="brainOutput">The output of the agent's brain that influences movement.</param>
        /// <returns>The calculated new position.</returns>
        private IVector CalculateNewPosition(IVector targetPos, float[] brainOutput)
        {
            if (brainOutput.Length < 2) return default;

            targetPos.X += movement * brainOutput[0];
            targetPos.Y += movement * brainOutput[1];

            return targetPos;
        }

        /// <summary>
        /// Gets the nearest target node of a specified type.
        /// </summary>
        /// <param name="nodeType">The type of node to search for.</param>
        /// <returns>The nearest target node of the specified type.</returns>
        public virtual INode<IVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            return DataContainer.GetNearestNode(nodeType, transform.position);
        }

        /// <summary>
        /// Gets the input count for a specified brain type.
        /// </summary>
        /// <param name="brainType">The brain type to retrieve input count for.</param>
        /// <returns>The number of inputs for the specified brain type.</returns>
        protected int GetInputCount(BrainType brainType)
        {
            return InputCountCache.GetInputCount(agentType, brainType);
        }

        /// <summary>
        /// Sets the position of the agent to the specified position if it is within the graph's borders.
        /// </summary>
        /// <param name="position">The new position to set for the agent.</param>
        public virtual void SetPosition(IVector position)
        {
            if (!DataContainer.graph.IsWithinGraphBorders(position)) return;
            Transform = (TTransform)new ITransform<IVector>(position);
        }

        /// <summary>
        /// Retrieves the key associated with the specified brain type from the brain types dictionary.
        /// </summary>
        /// <param name="value">The brain type to find the key for.</param>
        /// <returns>
        /// The key corresponding to the specified brain type.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the brain type is not found in the dictionary.
        /// </exception>
        public int GetBrainTypeKeyByValue(BrainType value)
        {
            foreach (KeyValuePair<int, BrainType> kvp in brainTypes)
            {
                if (EqualityComparer<BrainType>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException("The value is not present in the brainTypes dictionary.");
        }
    }
}