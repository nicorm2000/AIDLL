using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tester.DataManagement
{
    public enum SimAgentTypes
    {
        Carnivorous,
        Herbivore,
        Scavenger
    }
    public enum BrainType
    {
        Movement,
        ScavengerMovement,
        Eat,
        Attack,
        Escape,
        Flocking
    }
    
    public static class NeuronDataSystem
    {
        /// <summary>
        /// Saves the provided neuron data to JSON files, organized by agent type and brain type.
        /// - Creates a directory structure based on agent types.
        /// - Groups data by agent type and brain type, and saves each group to a separate JSON file.
        /// - File names include the generation number for versioning.
        /// </summary>
        /// <param name="agentsData">The list of agent neuron data to save.</param>
        /// <param name="directoryPath">The base directory path for saving the data.</param>
        /// <param name="generation">The generation number for naming files.</param>
        public static void SaveNeurons(List<AgentNeuronData> agentsData, string directoryPath, int generation)
        {
            var groupedData = agentsData
                .GroupBy(agent => new { agent.AgentType, agent.BrainType })
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var group in groupedData)
            {
                string agentTypeDirectory = Path.Combine(directoryPath, group.Key.AgentType.ToString());
                Directory.CreateDirectory(agentTypeDirectory);

                string fileName = $"gen{generation}{group.Key.BrainType}.json";
                string filePath = Path.Combine(agentTypeDirectory, fileName);
                string json = JsonSerializer.Serialize(group.Value);
                File.WriteAllText(filePath, json);
                Console.WriteLine("Saving in path: " + filePath);

            }
        }

        /// <summary>
        /// Loads the most recent neuron data from JSON files, organized by agent type and brain type.
        /// - Searches for directories and files matching the naming conventions.
        /// - Identifies the latest generation file for each agent and brain type.
        /// - Deserializes the JSON content into structured data.
        /// </summary>
        /// <param name="directoryPath">The base directory path to load the data from.</param>
        /// <returns>
        /// A nested dictionary where the first key is the agent type, 
        /// the second key is the brain type, 
        /// and the value is a list of agent neuron data.
        /// </returns>
        public static Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> LoadLatestNeurons(string directoryPath)
        {
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> agentsData = new Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>>();
            string[] directories = Directory.GetDirectories(directoryPath);

            foreach (string agentTypeDirectory in directories)
            {
                SimAgentTypes agentType = Enum.Parse<SimAgentTypes>(Path.GetFileName(agentTypeDirectory));
                agentsData[agentType] = new Dictionary<BrainType, List<AgentNeuronData>>();

                string[] files = Directory.GetFiles(agentTypeDirectory, "gen*.json");
                if (files.Length == 0)
                    continue;

                string latestFile = files.OrderByDescending(f =>
                {
                    string fileName = Path.GetFileName(f);
                    Match match = Regex.Match(fileName, @"gen(\d+)");
                    if (match.Success)
                    {
                        return int.Parse(match.Groups[1].Value);
                    }
                    return 0;
                }).First();

                string json = File.ReadAllText(latestFile);
                List<AgentNeuronData>? agentDataList = JsonSerializer.Deserialize<List<AgentNeuronData>>(json);

                foreach (AgentNeuronData agentData in agentDataList)
                {
                    if (!agentsData[agentType].ContainsKey(agentData.BrainType))
                    {
                        agentsData[agentType][agentData.BrainType] = new List<AgentNeuronData>();
                    }
                    agentsData[agentType][agentData.BrainType].Add(agentData);
                }
            }

            return agentsData;
        }        
    }
}