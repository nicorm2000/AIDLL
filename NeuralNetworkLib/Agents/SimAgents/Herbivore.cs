using NeuralNetworkLib.Agents.States;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.SimAgents
{
    public class Herbivore<TVector, TTransform> : SimAgent<TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
    {
        public int Hp
        {
            get => hp;
            set
            {
                hp = value;
                if (hp <= 0) Die();
            }
        }

        private int hp;
        private const int FoodDropped = 1;
        private const int InitialHp = 2;

        /// <summary>
        /// Initializes the herbivore agent. Sets the initial food target to 'Bush' and calculates the initial inputs.
        /// The agent's health points (hp) are also initialized.
        /// </summary>
        public override void Init()
        {
            base.Init();
            foodTarget = SimNodeType.Bush;

            CalculateInputs();

            hp = InitialHp;
        }

        /// <summary>
        /// Resets the herbivore agent. Resets the health points (hp) to the initial value.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            hp = InitialHp;
        }

        /// <summary>
        /// Sets additional inputs for the herbivore's escape behavior. 
        /// This includes setting the coordinates of the current node and checking for nearby carnivores.
        /// </summary>
        protected override void ExtraInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Escape);
            int inputCount = GetInputCount(BrainType.Escape);

            input[brain] = new float[inputCount];
            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;
            SimAgent<IVector, ITransform<IVector>> target =
                DataContainer.GetNearestEntity(SimAgentTypes.Carnivore, Transform.position);
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
        }

        /// <summary>
        /// Sets the movement inputs for the herbivore. This includes the current coordinates, nearest carnivore coordinates,
        /// and food target coordinates.
        /// </summary>
        protected override void MovementInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Movement);
            int inputCount = GetInputCount(BrainType.Movement);

            input[brain] = new float[inputCount];
            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            SimAgent<IVector, ITransform<IVector>> target =
                DataContainer.GetNearestEntity(SimAgentTypes.Carnivore, Transform.position);
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

            INode<IVector> nodeTarget = GetTarget(foodTarget);
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
            input[brain][7] = Hp;
        }

        /// <summary>
        /// Handles the death of the herbivore agent. The current node is marked as a corpse, and the food is dropped.
        /// The death event is triggered.
        /// </summary>
        private void Die()
        {
            INode<IVector> node = CurrentNode;
            node.NodeType = SimNodeType.Corpse;
            node.Food = FoodDropped;
            OnDeath?.Invoke(this);
        }

        /// <summary>
        /// Sets the transitions for the herbivore's eating behavior. The transitions define the conditions under which
        /// the herbivore will start or stop eating, search for food, escape, or attack.
        /// </summary>
        protected override void EatTransitions()
        {
            Fsm.SetTransition(Behaviours.Eat, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Eat, Flags.OnSearchFood, Behaviours.Walk);
            Fsm.SetTransition(Behaviours.Eat, Flags.OnEscape, Behaviours.Walk);
            Fsm.SetTransition(Behaviours.Eat, Flags.OnAttack, Behaviours.Walk);
        }

        /// <summary>
        /// Defines the state transitions for the herbivore's walking behavior. The transitions determine 
        /// when the herbivore should switch between walking, eating, escaping, or searching for food.
        /// </summary>
        protected override void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Walk, Flags.OnEscape, Behaviours.Walk);
            Fsm.SetTransition(Behaviours.Walk, Flags.OnAttack, Behaviours.Walk);
            Fsm.SetTransition(Behaviours.Walk, Flags.OnSearchFood, Behaviours.Walk);
        }

        /// <summary>
        /// This method can be overridden to add any additional transitions beyond the standard ones. 
        /// Currently, it does nothing, but it can be extended as needed.
        /// </summary>
        protected override void ExtraTransitions()
        {
        }

        /// <summary>
        /// Adds the behaviors for the herbivore's finite state machine (FSM). This method calls the 
        /// <see cref="ExtraBehaviours"/> method to add any additional behaviors beyond the default ones.
        /// </summary>
        protected override void FsmBehaviours()
        {
            ExtraBehaviours();
        }

        /// <summary>
        /// Adds the specific behaviors for the herbivore, including the 'Eat' and 'Walk' states. 
        /// This method is called to define the actions during each state. The 'Eat' state is tied to the 
        /// <see cref="SimEatHerbState"/> and the 'Walk' state is tied to the <see cref="SimWalkHerbState"/>.
        /// </summary>
        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatHerbState>(Behaviours.Eat, EatTickParameters);
            Fsm.AddBehaviour<SimWalkHerbState>(Behaviours.Walk, WalkTickParameters);
        }
    }
}