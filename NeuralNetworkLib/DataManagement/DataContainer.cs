using NeuralNetworkLib.Agents.Flocking;
using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.Utils;
using Random = System.Random;

namespace NeuralNetworkLib.DataManagement;

public struct NeuronInputCount
{
    public SimAgentTypes agentType;
    public BrainType brainType;
    public int inputCount;
    public int outputCount;
    public int[] hiddenLayersInputs;
}

public class DataContainer
{
    public static Sim2Graph graph;
    public static Dictionary<uint, SimAgent<IVector, ITransform<IVector>>> Agents = new Dictionary<uint, SimAgent<IVector, ITransform<IVector>>>();
    public static Dictionary<uint, Scavenger<IVector, ITransform<IVector>>> Scavengers = new Dictionary<uint, Scavenger<IVector, ITransform<IVector>>>();
    public static FlockingManager flockingManager = new FlockingManager();
    public static Dictionary<(BrainType, SimAgentTypes), NeuronInputCount> InputCountCache;
    public static NeuronInputCount[] inputCounts;
    public static Dictionary<int, BrainType> herbBrainTypes = new  Dictionary<int, BrainType>();
    public static Dictionary<int, BrainType> scavBrainTypes = new  Dictionary<int, BrainType>();
    public static Dictionary<int, BrainType> carnBrainTypes = new  Dictionary<int, BrainType>();
    private static Random random = new();

    /// <summary>
    /// Initializes brain type mappings and input counts for various simulation agent types.
    /// </summary>
    /// <remarks>
    /// This method sets up dictionaries to map agent types (e.g., Herbivore, Scavenger, Carnivore) 
    /// to their respective brain types and neural network configurations.
    /// </remarks>
    public static void Init()
    {
        herbBrainTypes = new Dictionary<int, BrainType>();
        scavBrainTypes = new Dictionary<int, BrainType>();
        carnBrainTypes = new Dictionary<int, BrainType>();
        herbBrainTypes[0] = BrainType.Eat;
        herbBrainTypes[1] = BrainType.Movement;
        herbBrainTypes[2] = BrainType.Escape;
        scavBrainTypes[0] = BrainType.Eat;
        scavBrainTypes[1] = BrainType.ScavengerMovement;
        scavBrainTypes[2] = BrainType.Flocking;
        carnBrainTypes[0] = BrainType.Eat;
        carnBrainTypes[1] = BrainType.Movement;
        carnBrainTypes[2] = BrainType.Attack;
        inputCounts = new[]
        {
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Carnivore, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                hiddenLayersInputs = new[] { 1 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Carnivore, brainType = BrainType.Movement, inputCount = 7,
                outputCount = 3, hiddenLayersInputs = new[] { 3 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Carnivore, brainType = BrainType.Attack, inputCount = 4,
                outputCount = 1, hiddenLayersInputs = new[] { 1 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Herbivore, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                hiddenLayersInputs = new[] { 1 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Herbivore, brainType = BrainType.Movement, inputCount = 8,
                outputCount = 2, hiddenLayersInputs = new[] { 3 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Herbivore, brainType = BrainType.Escape, inputCount = 4, outputCount = 1,
                hiddenLayersInputs = new[] { 1 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Scavenger, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                hiddenLayersInputs = new[] { 1 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Scavenger, brainType = BrainType.ScavengerMovement, inputCount = 7,
                outputCount = 2, hiddenLayersInputs = new[] { 3 }
            },
            new NeuronInputCount
            {
                agentType = SimAgentTypes.Scavenger, brainType = BrainType.Flocking, inputCount = 16,
                outputCount = 4,
                hiddenLayersInputs = new[] { 12, 8, 6, 4 }
            },
        };
        InputCountCache = inputCounts.ToDictionary(input => (input.brainType, input.agentType));
    }

    /// <summary>
    /// Converts a coordinate to a node in the graph, based on the X and Y values of the coordinate.
    /// Returns null if the coordinate is out of bounds.
    /// </summary>
    /// <param name="coordinate">The coordinate to convert into a node.</param>
    /// <returns>The node corresponding to the given coordinate, or null if the coordinate is out of bounds.</returns>
    public static INode<IVector> CoordinateToNode(IVector coordinate)
    {
        if (coordinate.X < 0 || coordinate.Y < 0 || coordinate.X >= graph.MaxX || coordinate.Y >= graph.MaxY)
        {
            return null;
        }

        return graph.NodesType[(int)coordinate.X, (int)coordinate.Y];
    }

    /// <summary>
    /// Finds the nearest node of the specified type to the given position.
    /// </summary>
    /// <param name="nodeType">The type of node to search for.</param>
    /// <param name="position">The position to find the nearest node to.</param>
    /// <returns>The nearest node of the specified type.</returns>
    public static INode<IVector> GetNearestNode(SimNodeType nodeType, IVector position)
    {
        INode<IVector> nearestNode = null;
        float minDistance = float.MaxValue;

        foreach (SimNode<IVector> node in graph.NodesType)
        {
            if (node.NodeType != nodeType) continue;

            float distance = IVector.Distance(position, node.GetCoordinate());

            if (minDistance < distance) continue;

            minDistance = distance;

            nearestNode = node;
        }

        return nearestNode;
    }

    /// <summary>
    /// Finds the nearest agent of the specified type to the given position.
    /// </summary>
    /// <param name="entityType">The type of entity (agent) to search for.</param>
    /// <param name="position">The position to find the nearest agent to.</param>
    /// <returns>The nearest agent of the specified type.</returns>
    public static SimAgent<IVector, ITransform<IVector>> GetNearestEntity(SimAgentTypes entityType, IVector position)
    {
        SimAgent<IVector, ITransform<IVector>> nearestAgent = null;
        float minDistance = float.MaxValue;

        foreach (SimAgent<IVector, ITransform<IVector>> agent in Agents.Values)
        {
            if (agent.agentType != entityType) continue;

            float distance = IVector.Distance(position, agent.CurrentNode.GetCoordinate());

            if (minDistance < distance) continue;

            minDistance = distance;
            nearestAgent = agent;
        }

        return nearestAgent;
    }

    /// <summary>
    /// Returns a list of boids within the detection radius of the given boid.
    /// </summary>
    /// <param name="boid">The boid for which to find nearby boids.</param>
    /// <returns>A list of boids within the detection radius of the given boid.</returns>
    public static List<ITransform<IVector>> GetBoidsInsideRadius(Boid<IVector, ITransform<IVector>> boid)
    {
        List<ITransform<IVector>> insideRadiusBoids = new List<ITransform<IVector>>();
        float detectionRadiusSquared = boid.detectionRadious * boid.detectionRadious;
        IVector boidPosition = boid.transform.position;

        Parallel.ForEach(Scavengers.Values, scavenger =>
        {
            if (scavenger?.Transform.position == null || boid == scavenger.boid)
            {
                return;
            }

            IVector scavengerPosition = scavenger.Transform.position;
            float distanceSquared = IVector.DistanceSquared(boidPosition, scavengerPosition);

            if (distanceSquared > detectionRadiusSquared) return;
            lock (insideRadiusBoids)
            {
                insideRadiusBoids.Add(scavenger.boid.transform);
            }
        });

        return insideRadiusBoids;
    }

    /// <summary>
    /// Gets the key of the brain type by its value for the specified agent type.
    /// </summary>
    /// <param name="value">The brain type value to search for.</param>
    /// <param name="agentType">The type of agent (e.g., carnivore, herbivore, scavenger) to which the brain type belongs.</param>
    /// <returns>The key of the brain type for the specified agent type.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid agent type is provided.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the brain type value is not found for the given agent type.</exception>
    public static int GetBrainTypeKeyByValue(BrainType value, SimAgentTypes agentType)
    {
        Dictionary<int, BrainType> brainTypes = agentType switch
        {
            SimAgentTypes.Carnivore => carnBrainTypes,
            SimAgentTypes.Herbivore => herbBrainTypes,
            SimAgentTypes.Scavenger => scavBrainTypes,
            _ => throw new ArgumentException("Invalid agent type")
        };

        foreach (KeyValuePair<int, BrainType> kvp in brainTypes)
        {
            if (kvp.Value == value)
            {
                return kvp.Key;
            }
        }

        throw new KeyNotFoundException(
            $"The value '{value}' is not present in the brainTypes dictionary for agent type '{agentType}'.");
    }

    /// <summary>
    /// Gets a random position within the lower quarter of the graph (y-coordinate between 0 and Height/4).
    /// </summary>
    /// <returns>A random node located within the lower quarter of the graph.</returns>
    public static INode<IVector> GetRandomPositionInLowerQuarter()
    {
        int x = random.Next(0, graph.MaxX);
        int y = random.Next(1, graph.MaxY / 4);
        return DataContainer.graph.NodesType[x, y];
    }

    /// <summary>
    /// Gets a random position within the upper quarter of the graph (y-coordinate between 3*Height/4 and Height-1).
    /// </summary>
    /// <returns>A random node located within the upper quarter of the graph.</returns>
    public static INode<IVector> GetRandomPositionInUpperQuarter()
    {
        int x = random.Next(0, graph.MaxX);
        int y = random.Next(3 * graph.MaxY / 4, graph.MaxY - 1);
        return DataContainer.graph.NodesType[x, y];
    }

    /// <summary>
    /// Gets a random position within the entire graph (y-coordinate between 0 and Height).
    /// </summary>
    /// <returns>A random node located anywhere in the graph.</returns>
    public static INode<IVector> GetRandomPosition()
    {
        int x = random.Next(0, graph.MaxX);
        int y = random.Next(0, graph.MaxY);
        return DataContainer.graph.NodesType[x, y];
    }
}