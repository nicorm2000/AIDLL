using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.NeuralNetDirectory.ECS;
using NeuralNetworkLib.NeuralNetDirectory.ECS.Patron;
using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.NeuralNetDirectory
{
    public class FitnessManager<TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
    {
        private static Dictionary<uint, SimAgent<TVector, TTransform>> _agents;

        /// <summary>
        /// Initializes the FitnessManager with a dictionary of agents.
        /// </summary>
        /// <param name="agents">A dictionary of agents with their unique IDs.</param>
        public FitnessManager(Dictionary<uint, SimAgent<TVector, TTransform>> agents)
        {
            _agents = agents;
        }

        /// <summary>
        /// Executes the fitness calculation for each agent on every tick.
        /// </summary>
        public void Tick()
        {
            foreach (KeyValuePair<uint, SimAgent<TVector, TTransform>> agent in _agents)
            {
                CalculateFitness(agent.Value.agentType, agent.Key);
            }
        }

        /// <summary>
        /// Calculates the fitness of an agent based on its type and ID.
        /// </summary>
        /// <param name="agentType">The type of the agent (Carnivore, Herbivore, Scavenger).</param>
        /// <param name="agentId">The ID of the agent.</param>
        public void CalculateFitness(SimAgentTypes agentType, uint agentId)
        {
            switch (agentType)
            {
                case SimAgentTypes.Carnivore:
                    CarnivoreFitnessCalculator(agentId);
                    break;
                case SimAgentTypes.Herbivore:
                    HerbivoreFitnessCalculator(agentId);
                    break;
                case SimAgentTypes.Scavenger:
                    ScavengerFitnessCalculator(agentId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null);
            }
        }

        /// <summary>
        /// Calculates the fitness of a herbivore agent based on its current actions.
        /// </summary>
        /// <param name="agentId">The ID of the herbivore agent.</param>
        private void HerbivoreFitnessCalculator(uint agentId)
        {
            foreach (KeyValuePair<int, BrainType> brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.Movement:
                        HerbivoreMovementFC(agentId);
                        break;
                    case BrainType.Eat:
                        EatFitnessCalculator(agentId);
                        break;
                    case BrainType.Escape:
                        HerbivoreEscapeFC(agentId);
                        break;
                    case BrainType.ScavengerMovement:
                    case BrainType.Attack:
                    case BrainType.Flocking:
                    default:
                        throw new ArgumentException("Herbivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        /// <summary>
        /// Calculates the escape fitness for a herbivore when near a predator.
        /// </summary>
        /// <param name="agentId">The ID of the herbivore agent.</param>
        private void HerbivoreEscapeFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            Herbivore<IVector, ITransform<IVector>> agent = _agents[agentId] as Herbivore<IVector, ITransform<IVector>>;
            SimAgent<IVector, ITransform<IVector>> nearestPredatorNode =
                DataContainer.GetNearestEntity(SimAgentTypes.Carnivore, agent?.Transform.position);

            IVector targetPosition;

            if (nearestPredatorNode?.CurrentNode?.GetCoordinate() == null) return;
            targetPosition = nearestPredatorNode.CurrentNode.GetCoordinate();

            if (!IsMovingTowardsTarget(agentId, targetPosition))
            {
                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId), reward, BrainType.Escape);
            }

            if (agent?.Hp < 2)
            {
                Punish(ECSManager.GetComponent<NeuralNetComponent>(agentId), punishment, BrainType.Escape);
            }
        }

        /// <summary>
        /// Calculates the movement fitness for a herbivore agent when avoiding predators.
        /// </summary>
        /// <param name="agentId">The ID of the herbivore agent.</param>
        private void HerbivoreMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            SimAgent<TVector, TTransform> agent = _agents[agentId];
            SimAgent<IVector, ITransform<IVector>> nearestPredatorNode =
                DataContainer.GetNearestEntity(SimAgentTypes.Carnivore, agent.Transform.position);

            if (nearestPredatorNode?.CurrentNode?.GetCoordinate() == null) return;
            IVector targetPosition = nearestPredatorNode.CurrentNode.GetCoordinate();

            if (!IsMovingTowardsTarget(agentId, targetPosition))
            {
                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward, BrainType.Movement);
            }
            else
            {
                Punish(ECSManager.GetComponent<NeuralNetComponent>(agentId),punishment, BrainType.Movement);
            }
        }

        /// <summary>
        /// Calculates the fitness of a carnivore agent based on its current actions.
        /// </summary>
        /// <param name="agentId">The ID of the carnivore agent.</param>
        private void CarnivoreFitnessCalculator(uint agentId)
        {
            foreach (KeyValuePair<int, BrainType> brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.Attack:
                        CarnivoreAttackFC(agentId);
                        break;
                    case BrainType.Eat:
                        EatFitnessCalculator(agentId);
                        break;
                    case BrainType.Movement:
                        CarnivoreMovementFC(agentId);
                        break;
                    case BrainType.ScavengerMovement:
                    case BrainType.Escape:
                    case BrainType.Flocking:
                    default:
                        throw new ArgumentException("Carnivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        /// <summary>
        /// Calculates the attack fitness for a carnivore agent when hunting herbivores.
        /// </summary>
        /// <param name="agentId">The ID of the carnivore agent.</param>
        private void CarnivoreAttackFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            Carnivore<TVector, TTransform> agent = (Carnivore<TVector, TTransform>)_agents[agentId];
            SimAgent<IVector, ITransform<IVector>> nearestHerbivoreNode =
                DataContainer.GetNearestEntity(SimAgentTypes.Herbivore, agent.Transform.position);

            if (nearestHerbivoreNode?.CurrentNode?.GetCoordinate() == null) return;
            IVector targetPosition = nearestHerbivoreNode.CurrentNode.GetCoordinate();

            if (IsMovingTowardsTarget(agentId, targetPosition))
            {
                float killRewardMod = agent.HasKilled ? 2 : 1;
                float attackedRewardMod = agent.HasAttacked ? 1.5f : 0;
                float damageRewardMod = (float)agent.DamageDealt * 2 / 5;
                float rewardMod = killRewardMod * attackedRewardMod * damageRewardMod;

                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward * rewardMod, BrainType.Attack);
            }
            else
            {
                Punish(ECSManager.GetComponent<NeuralNetComponent>(agentId),punishment, BrainType.Attack);
            }
        }

        /// <summary>
        /// Calculates the movement fitness for a carnivore agent when pursuing prey or corpses.
        /// </summary>
        /// <param name="agentId">The ID of the carnivore agent.</param>
        private void CarnivoreMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            SimAgent<TVector, TTransform> agent = _agents[agentId];
            SimAgent<IVector, ITransform<IVector>> nearestHerbivoreNode =
                DataContainer.GetNearestEntity(SimAgentTypes.Herbivore, agent.Transform.position);
            INode<IVector> nearestCorpseNode = DataContainer.GetNearestNode(SimNodeType.Corpse, agent.Transform.position);

            if (nearestHerbivoreNode?.CurrentNode?.GetCoordinate() == null) return;

            IVector herbPosition = nearestHerbivoreNode.CurrentNode.GetCoordinate();
            IVector corpsePosition = nearestCorpseNode?.GetCoordinate();

            bool movingToHerb = IsMovingTowardsTarget(agentId, herbPosition);
            bool movingToCorpse = corpsePosition != null && IsMovingTowardsTarget(agentId, corpsePosition);
            
            if (movingToHerb || movingToCorpse)
            {
                float rewardMod = movingToHerb ? 1.15f : 0.9f;
                
                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward * rewardMod, BrainType.Movement);
            }
            else
            {
                Punish(ECSManager.GetComponent<NeuralNetComponent>(agentId),punishment, BrainType.Movement);
            }
        }

        /// <summary>
        /// Calculates the fitness of an agent based on its brain type. For each agent, it iterates through the brain types 
        /// and calls the appropriate fitness calculator method, such as <see cref="ScavengerMovementFC"/>, <see cref="EatFitnessCalculator"/>, 
        /// or <see cref="ScavengerFlockingFC"/>. If an unrecognized brain type is found, it throws an exception.
        /// </summary>
        /// <param name="agentId">The ID of the agent whose fitness is being calculated.</param>
        private void ScavengerFitnessCalculator(uint agentId)
        {
            foreach (KeyValuePair<int, BrainType> brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.ScavengerMovement:
                        ScavengerMovementFC(agentId);
                        break;
                    case BrainType.Eat:
                        EatFitnessCalculator(agentId);
                        break;
                    case BrainType.Flocking:
                        ScavengerFlockingFC(agentId);
                        break;
                    case BrainType.Movement:
                    case BrainType.Attack:
                    case BrainType.Escape:
                    default:
                        throw new ArgumentException("Scavenger doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        /// <summary>
        /// Calculates the fitness of a scavenger agent based on its flocking behavior. The method checks if the agent is 
        /// maintaining a safe distance from other agents, aligning with the flock, and moving towards its target. 
        /// Rewards or punishes the agent based on its performance in these areas.
        /// </summary>
        /// <param name="agentId">The ID of the agent whose flocking fitness is being calculated.</param>
        private void ScavengerFlockingFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;
            const float safeDistance = 0.7f;

            Scavenger<TVector, TTransform> agent = (Scavenger<TVector, TTransform>)_agents[agentId];
            IVector targetPosition = agent.target;

            bool isMaintainingDistance = true;
            bool isAligningWithFlock = true;
            bool isColliding = false;

            IVector averageDirection = null;
            int neighborCount = 0;

            foreach (ITransform<IVector> neighbor in agent.boid.NearBoids)
            {
                IVector neighborPosition = neighbor.position;
                float distance = agent.Transform.position.Distance(neighborPosition);

                if (distance < safeDistance)
                {
                    isColliding = true;
                    isMaintainingDistance = false;
                }

                averageDirection += neighbor.forward;
                neighborCount++;
            }

            if (neighborCount > 0)
            {
                averageDirection /= neighborCount;
                IVector agentDirection = agent.boid.transform.forward;
                float alignmentDotProduct = IVector.Dot(agentDirection, averageDirection.Normalized());

                if (alignmentDotProduct < 0.9f)
                {
                    isAligningWithFlock = false;
                }
            }

            if (isMaintainingDistance || isAligningWithFlock || IsMovingTowardsTarget(agentId, targetPosition))
            {
                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward, BrainType.Flocking);
            }
            
            if (isColliding || !IsMovingTowardsTarget(agentId, targetPosition))
            {
                Punish(ECSManager.GetComponent<NeuralNetComponent>(agentId),punishment, BrainType.Flocking);
            }
        }

        /// <summary>
        /// Calculates the fitness of a scavenger agent based on its movement behavior. The agent's fitness is determined by 
        /// the number of neighboring agents and its ability to move towards specific targets, such as carrion, corpses, or carnivores. 
        /// Rewards and punishments are applied based on the agent's proximity to these targets and its movement towards them.
        /// </summary>
        /// <param name="agentId">The ID of the agent whose movement fitness is being calculated.</param>
        private void ScavengerMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            int brainId = (int)BrainType.ScavengerMovement;
            Scavenger<TVector, TTransform> agent = (Scavenger<TVector, TTransform>)_agents[agentId];
            int neighbors = DataContainer.GetBoidsInsideRadius(agent.boid).Count;
            INode<IVector> nearestCarrionNode = DataContainer.GetNearestNode(SimNodeType.Carrion, agent.Transform.position);
            INode<IVector> nearestCorpseNode = DataContainer.GetNearestNode(SimNodeType.Corpse, agent.Transform.position);
            SimAgent<IVector, ITransform<IVector>> nearestCarNode = DataContainer.GetNearestEntity(SimAgentTypes.Carnivore, agent.Transform.position);

            IVector targetPosition;

            if (nearestCarrionNode != null)
            {
                targetPosition = nearestCarrionNode.GetCoordinate();
            }
            else if (nearestCorpseNode != null)
            {
                targetPosition = nearestCorpseNode.GetCoordinate();
            }
            else
            {
                if (nearestCarNode?.CurrentNode?.GetCoordinate() == null) return;
                targetPosition = nearestCarNode.CurrentNode.GetCoordinate();
            }

            if (targetPosition == null) return;

            if(neighbors > 0)
            {
                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward/5+neighbors, BrainType.ScavengerMovement);
            }
            
            if (IsMovingTowardsTarget(agentId, targetPosition))
            {
                Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward, BrainType.ScavengerMovement);
            }
            else
            {
                Punish(ECSManager.GetComponent<NeuralNetComponent>(agentId),punishment, BrainType.ScavengerMovement);
            }
        }

        /// <summary>
        /// Calculates the fitness of a scavenger agent based on its food consumption. If the agent has food, the method 
        /// rewards the agent based on the amount of food it has relative to its food limit. The reward is proportional to 
        /// the agent's food intake.
        /// </summary>
        /// <param name="agentId">The ID of the agent whose eating fitness is being calculated.</param>
        private void EatFitnessCalculator(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            SimAgent<TVector, TTransform> agent = _agents[agentId];

            if (agent.Food <= 0) return;

            float rewardMod = (float)agent.Food * 2 / agent.FoodLimit;
            Reward(ECSManager.GetComponent<NeuralNetComponent>(agentId),reward * rewardMod, BrainType.Eat);
        }

        /// <summary>
        /// Checks if the agent is moving towards its target. It compares the agent's current direction with the direction 
        /// towards the target using the dot product. If the agent's direction is sufficiently aligned with the target, it 
        /// returns <c>true</c>, indicating the agent is moving towards the target.
        /// </summary>
        /// <param name="agentId">The ID of the agent to check.</param>
        /// <param name="targetPosition">The target position the agent is attempting to reach.</param>
        /// <returns><c>true</c> if the agent is moving towards the target, otherwise <c>false</c>.</returns>
        private bool IsMovingTowardsTarget(uint agentId, IVector targetPosition)
        {
            SimAgent<TVector, TTransform> agent = _agents[agentId];
            IVector currentPosition = agent.Transform.position;
            IVector agentDirection = agent.Transform.forward;

            if (targetPosition == null || currentPosition == null) return false;
            IVector directionToTarget = (targetPosition - currentPosition).Normalized();
            if(directionToTarget == null || agentDirection == null) return false;
            float dotProduct = IVector.Dot(directionToTarget, agentDirection);

            return dotProduct > 0.9f;
        }

        /// <summary>
        /// Applies a reward to the agent's fitness score. The method increases the agent's fitness based on the specified 
        /// reward and updates its fitness modifier.
        /// </summary>
        /// <param name="neuralNetComponent">The neural network component of the agent.</param>
        /// <param name="reward">The reward to be applied.</param>
        /// <param name="brainType">The brain type of the agent being rewarded.</param>
        private void Reward(NeuralNetComponent neuralNetComponent, float reward, BrainType brainType)
        {
            int id = DataContainer.GetBrainTypeKeyByValue(brainType, neuralNetComponent.Layers[0][0].AgentType);
            neuralNetComponent.FitnessMod[id] = IncreaseFitnessMod(neuralNetComponent.FitnessMod[id]);
            neuralNetComponent.Fitness[id] += reward * neuralNetComponent.FitnessMod[id];
        }

        /// <summary>
        /// Applies a punishment to the agent's fitness score. The method decreases the agent's fitness based on the specified 
        /// punishment and adjusts the fitness modifier accordingly.
        /// </summary>
        /// <param name="neuralNetComponent">The neural network component of the agent.</param>
        /// <param name="punishment">The punishment to be applied.</param>
        /// <param name="brainType">The brain type of the agent being punished.</param>
        private void Punish(NeuralNetComponent neuralNetComponent, float punishment, BrainType brainType)
        {
            const float mod = 0.9f;
            int id = DataContainer.GetBrainTypeKeyByValue(brainType, neuralNetComponent.Layers[0][0].AgentType);

            neuralNetComponent.FitnessMod[id] *= mod;
            neuralNetComponent.Fitness[id] /= punishment + 0.05f * neuralNetComponent.FitnessMod[id];
        }

        /// <summary>
        /// Increases the fitness modifier of the agent, ensuring it does not exceed a maximum limit. The method applies a 
        /// multiplicative factor to the current modifier and clamps it to a maximum value to prevent excessive growth.
        /// </summary>
        /// <param name="fitnessMod">The current fitness modifier of the agent.</param>
        /// <returns>The new fitness modifier after applying the increase.</returns>
        private float IncreaseFitnessMod(float fitnessMod)
        {
            const float maxFitness = 2;
            const float mod = 1.1f;
            fitnessMod *= mod;
            if (fitnessMod > maxFitness) fitnessMod = maxFitness;
            return fitnessMod;
        }
    }
}