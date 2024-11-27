using NeuralNetworkLib.Agents.States;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.SimAgents
{
    public class Carnivore<TVector, TTransform> : SimAgent<TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
    {
        public Action OnAttack { get; set; }
        public bool HasAttacked { get; private set; }
        public bool HasKilled { get; private set; }

        public int DamageDealt { get; private set; } = 0;

        private SimAgent<IVector, ITransform<IVector>> target;

        /// <summary>
        /// Initializes the carnivore's state by setting up basic properties such as food target, movement speed, 
        /// and attack status. It also registers the attack event.
        /// </summary>
        public override void Init()
        {
            base.Init();
            foodTarget = SimNodeType.Corpse;
            FoodLimit = 1;
            movement = 2;
            HasAttacked = false;
            HasKilled = false;

            CalculateInputs();
            OnAttack += Attack;
        }

        /// <summary>
        /// Uninitializes the carnivore's state by unregistering the attack event.
        /// </summary>
        public override void Uninit()
        {
            base.Uninit();
            OnAttack -= Attack;
        }

        /// <summary>
        /// Resets the carnivore's state, including resetting the attack and kill flags, as well as damage dealt.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            HasAttacked = false;
            HasKilled = false;
            DamageDealt = 0;
        }

        /// <summary>
        /// Updates the carnivore's inputs, including target searching, food searching, movement inputs, 
        /// and any additional inputs. It calls <see cref="FindFoodInputs"/> and <see cref="MovementInputs"/> 
        /// for specific logic.
        /// </summary>
        public override void UpdateInputs()
        {
            target = DataContainer.GetNearestEntity(SimAgentTypes.Herbivore, Transform.position);
            FindFoodInputs();
            MovementInputs();
            ExtraInputs();
        }

        /// <summary>
        /// Defines the extra inputs for the carnivore's brain, including coordinates for the current position, target, 
        /// and food target. If no target is found, it sets placeholders for target coordinates.
        /// </summary>
        protected override void ExtraInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Attack);
            int inputCount = GetInputCount(BrainType.Attack);
            input[brain] = new float[inputCount];

            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
                return;
            }

            input[brain][2] = target.CurrentNode.GetCoordinate().X;
            input[brain][3] = target.CurrentNode.GetCoordinate().Y;
        }

        /// <summary>
        /// Defines movement-related inputs, including coordinates of the current node, target, and food node. 
        /// The inputs are set accordingly, with placeholders if targets are not found.
        /// </summary>
        protected override void MovementInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Movement);
            int inputCount = GetInputCount(BrainType.Movement);

            input[brain] = new float[inputCount];
            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            INode<IVector> nodeTarget = GetTarget(foodTarget);


            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }
            else
            {
                input[brain][2] = target.CurrentNode.GetCoordinate().X;
                input[brain][3] = target.CurrentNode.GetCoordinate().Y;
            }

            if (nodeTarget == null)
            {
                input[brain][4] = NoTarget;
                input[brain][5] = NoTarget;
            }
            else
            {
                input[brain][4] = nodeTarget.GetCoordinate().X;
                input[brain][5] = nodeTarget.GetCoordinate().Y;
            }

            input[brain][6] = Food;
        }

        /// <summary>
        /// Adds behaviors related to eating, walking, and attacking to the finite state machine (FSM).
        /// </summary>
        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatCarnState>(Behaviours.Eat, EatTickParameters);

            Fsm.AddBehaviour<SimWalkCarnState>(Behaviours.Walk, WalkTickParameters);

            Fsm.AddBehaviour<SimAttackState>(Behaviours.Attack, AttackTickParameters);
        }

        /// <summary>
        /// Returns the tick parameters for the attack behavior, including various details such as the current node, 
        /// food target, movement state, and outputs related to eating and attacking.
        /// </summary>
        private object[] AttackTickParameters()
        {
            if (output.Length < 3 || output[GetBrainTypeKeyByValue(BrainType.Movement)].Length < 2)
            {
                return Array.Empty<object>();
            }

            int extraBrain = GetBrainTypeKeyByValue(BrainType.Attack);
            object[] objects =
            {
                CurrentNode,
                foodTarget,
                OnMove,
                output[GetBrainTypeKeyByValue(BrainType.Eat)],
                output[extraBrain],
                OnAttack,
                output[GetBrainTypeKeyByValue(BrainType.Eat)],
                output[GetBrainTypeKeyByValue(BrainType.Attack)],
                output[GetBrainTypeKeyByValue(BrainType.Movement)][2]
            };
            return objects;
        }

        /// <summary>
        /// Returns the tick parameters for the walking behavior, including the current node, target coordinates, 
        /// food target, and movement output.
        /// </summary>
        protected override object[] WalkTickParameters()
        {
            object[] objects =
            {
                CurrentNode, target?.CurrentNode.GetCoordinate(), foodTarget, OnMove,
                output[GetBrainTypeKeyByValue(BrainType.Eat)], output[GetBrainTypeKeyByValue(BrainType.Attack)]
            };
            return objects;
        }

        /// <summary>
        /// Executes the attack logic on the target herbivore, reducing its health and setting flags for attack and kill.
        /// </summary>
        private void Attack()
        {
            if (target.agentType != SimAgentTypes.Herbivore ||
                !Approximatly(target.Transform.position, transform.position, 0.2f)) return;
            Herbivore<IVector, ITransform<IVector>> herbivore = (Herbivore<IVector, ITransform<IVector>>)target;
            herbivore.Hp--;
            HasAttacked = true;
            DamageDealt++;
            if (herbivore.Hp <= 0)
            {
                HasKilled = true;
            }
        }

        /// <summary>
        /// Executes the eating logic for the carnivore. It decreases the food count of the current node and increases 
        /// the carnivore's food amount. If the node's food count reaches zero, the node is changed to carrion.
        /// </summary>
        protected override void Eat()
        {
            INode<IVector> node = CurrentNode;

            lock (node)
            {
                if (node.Food <= 0) return;
                Food++;
                node.Food--;

                if (node.Food > 0) return;

                node.NodeType = SimNodeType.Carrion;
                node.Food = 30;
            }
        }

        /// <summary>
        /// Checks whether two coordinates are approximately equal within a given tolerance.
        /// </summary>
        private bool Approximatly(IVector coord1, IVector coord2, float tolerance)
        {
            return Math.Abs(coord1.X - coord2.X) <= tolerance && Math.Abs(coord1.Y - coord2.Y) <= tolerance;
        }

        /// <summary>
        /// Adds the extra behaviors to the FSM by calling <see cref="ExtraBehaviours"/>.
        /// </summary>
        protected override void FsmBehaviours()
        {
            ExtraBehaviours();
        }

        /// <summary>
        /// Defines the state transitions for the carnivore's eating behavior, including transitioning to walking or attacking.
        /// </summary>
        protected override void EatTransitions()
        {
            Fsm.SetTransition(Behaviours.Eat, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Eat, Flags.OnSearchFood, Behaviours.Walk);
            Fsm.SetTransition(Behaviours.Eat, Flags.OnAttack, Behaviours.Attack);
        }

        /// <summary>
        /// Defines the state transitions for the carnivore's walking behavior, including transitioning to eating or attacking.
        /// </summary>
        protected override void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Walk, Flags.OnAttack, Behaviours.Attack);
            Fsm.SetTransition(Behaviours.Walk, Flags.OnSearchFood, Behaviours.Walk);
        }

        /// <summary>
        /// Defines additional state transitions for the attack behavior, including transitions to eating or walking states.
        /// </summary>
        protected override void ExtraTransitions()
        {
            Fsm.SetTransition(Behaviours.Attack, Flags.OnAttack, Behaviours.Attack);
            Fsm.SetTransition(Behaviours.Attack, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Attack, Flags.OnSearchFood, Behaviours.Walk);
        }
    }
}