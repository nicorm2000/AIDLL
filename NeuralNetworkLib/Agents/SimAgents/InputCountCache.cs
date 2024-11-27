using System.Collections.Concurrent;
using System.Linq;
using NeuralNetworkLib.DataManagement;

namespace NeuralNetworkLib.Agents.SimAgents
{
    public static class InputCountCache
    {
        private static readonly ConcurrentDictionary<(SimAgentTypes, BrainType), int> cache = new ConcurrentDictionary<(SimAgentTypes, BrainType), int>();

        /// <summary>
        /// Retrieves the input count based on the specified agent type and brain type. 
        /// If the input count is already cached for the given combination of agent and brain types, 
        /// it is returned directly. Otherwise, the input count is looked up from the data container and cached for future use.
        /// </summary>
        /// <param name="agentType">
        /// The <see cref="SimAgentTypes"/> representing the type of the agent whose input count is being retrieved.
        /// </param>
        /// <param name="brainType">
        /// The <see cref="BrainType"/> representing the type of the brain for the agent.
        /// </param>
        /// <returns>
        /// An integer representing the input count for the given agent type and brain type. 
        /// If the agent and brain type combination is not found, the default value is returned.
        /// </returns>
        public static int GetInputCount(SimAgentTypes agentType, BrainType brainType)
        {
            (SimAgentTypes agentType, BrainType brainType) key = (agentType, brainType);
            if (cache.TryGetValue(key, out int inputCount)) return inputCount;
            
            inputCount = DataContainer.inputCounts
                .FirstOrDefault(input => input.agentType == agentType && input.brainType == brainType).inputCount;
            cache[key] = inputCount;

            return inputCount;
        }
    }
}