using Newtonsoft.Json;

namespace NeuralNetworkLib.DataManagement
{
    public enum SimAgentTypes
    {
        Carnivore,
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
        public static Action<bool>? OnSpecificLoaded;

        /// <summary>
        /// Saves the neural network data of agents, grouped by their <see cref="AgentType"/> and <see cref="BrainType"/>, to a directory.
        /// The data is serialized into JSON files, named according to the generation number.
        /// </summary>
        /// <param name="agentsData">The list of agent neural network data to be saved.</param>
        /// <param name="directoryPath">The directory path where the data should be saved.</param>
        /// <param name="generation">The generation number used in the file name.</param>
        public static void SaveNeurons(List<AgentNeuronData> agentsData, string directoryPath, int generation)
        {
            if (agentsData == null)
            {
                throw new ArgumentNullException(nameof(agentsData), "Agents data cannot be null.");
            }

            var groupedData = agentsData
                .GroupBy(agent => new { agent.AgentType, agent.BrainType })
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var group in groupedData)
            {
                string agentTypeDirectory = Path.Combine(directoryPath, group.Key.AgentType.ToString());
                string brainTypeDirectory = Path.Combine(agentTypeDirectory, group.Key.BrainType.ToString());
                Directory.CreateDirectory(brainTypeDirectory);

                string fileName = $"gen{generation}.json";
                string filePath = Path.Combine(brainTypeDirectory, fileName);
                string json = JsonConvert.SerializeObject(group.Value);
                File.WriteAllText(filePath, json);
            }
        }

        /// <summary>
        /// Loads the most recent neural network data for each <see cref="SimAgentTypes"/> and <see cref="BrainType"/> from a directory.
        /// The method retrieves the latest JSON file based on the generation number.
        /// </summary>
        /// <param name="directoryPath">The directory path where the data is stored.</param>
        /// <returns>A dictionary of agent types and their corresponding brain types, with their associated neural network data.</returns>
        public static Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>?>> LoadLatestNeurons(
            string directoryPath)
        {
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>?>> agentsData = new();
            string[] agentDirectories = Directory.Exists(directoryPath)
                ? Directory.GetDirectories(directoryPath)
                : Array.Empty<string>();

            foreach (string agentTypeDirectory in agentDirectories)
            {
                SimAgentTypes agentType = Enum.Parse<SimAgentTypes>(Path.GetFileName(agentTypeDirectory));
                agentsData[agentType] = new Dictionary<BrainType, List<AgentNeuronData>?>();

                string[] brainDirectories = Directory.GetDirectories(agentTypeDirectory);
                foreach (string brainTypeDirectory in brainDirectories)
                {
                    BrainType brainType = Enum.Parse<BrainType>(Path.GetFileName(brainTypeDirectory));
                    string[] files = Directory.GetFiles(brainTypeDirectory, "gen*.json");
                    if (files.Length == 0)
                        continue;

                    string? latestFile = files
                        .OrderByDescending(f =>
                        {
                            string? fileName = Path.GetFileName(f);
                            string[]? parts = fileName.Split('n');
                            if (parts.Length > 1 && int.TryParse(parts[1].Split('.')[0], out int generation))
                            {
                                return generation;
                            }

                            return -1;
                        }).First();

                    string json = File.ReadAllText(latestFile);
                    List<AgentNeuronData>? agentData;
                    try
                    {
                        agentData = JsonConvert.DeserializeObject<List<AgentNeuronData>>(json);
                    }
                    catch (JsonException)
                    {
                        agentData = new List<AgentNeuronData>();
                    }

                    agentsData[agentType][brainType] = agentData;
                }
            }

            return agentsData;
        }

        /// <summary>
        /// Loads the neural network data for a specific generation of agents, grouped by <see cref="SimAgentTypes"/> and <see cref="BrainType"/>, from a directory.
        /// If the specific generation is not found, it loads the latest available generation data.
        /// </summary>
        /// <param name="directoryPath">The directory path where the data is stored.</param>
        /// <param name="specificGeneration">The specific generation number to load.</param>
        /// <returns>A dictionary of agent types and their corresponding brain types, with their associated neural network data for the specified generation.</returns>
        public static Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>?>> LoadSpecificNeurons(
            string directoryPath, int specificGeneration)
        {
            var agentsData = new Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>?>>();
            var agentDirectories = Directory.Exists(directoryPath)
                ? Directory.GetDirectories(directoryPath)
                : Array.Empty<string>();

            foreach (var agentTypeDirectory in agentDirectories)
            {
                var agentType = Enum.Parse<SimAgentTypes>(Path.GetFileName(agentTypeDirectory));
                agentsData[agentType] = new Dictionary<BrainType, List<AgentNeuronData>?>();

                var brainDirectories = Directory.GetDirectories(agentTypeDirectory);
                foreach (var brainTypeDirectory in brainDirectories)
                {
                    var brainType = Enum.Parse<BrainType>(Path.GetFileName(brainTypeDirectory));
                    var files = Directory.GetFiles(brainTypeDirectory, "gen*.json");
                    if (files.Length == 0)
                        continue;

                    string? targetFile = files
                        .FirstOrDefault(f =>
                        {
                            var fileName = Path.GetFileName(f);
                            var parts = fileName.Split('n');
                            return parts.Length > 1 && int.TryParse(parts[1].Split('.')[0], out int generation) && generation == specificGeneration;
                        });

                    if (targetFile == null)
                    {
                        targetFile = files
                            .OrderByDescending(f =>
                            {
                                var fileName = Path.GetFileName(f);
                                var parts = fileName.Split('n');
                                if (parts.Length > 1 && int.TryParse(parts[1].Split('.')[0], out int generation))
                                {
                                    return generation;
                                }

                                return -1;
                            }).First();
                        OnSpecificLoaded?.Invoke(false);
                    }
                    else
                    {
                        OnSpecificLoaded?.Invoke(true);
                    }

                    var json = File.ReadAllText(targetFile);
                    List<AgentNeuronData>? agentData;
                    try
                    {
                        agentData = JsonConvert.DeserializeObject<List<AgentNeuronData>>(json);
                    }
                    catch (JsonException)
                    {
                        agentData = new List<AgentNeuronData>();
                    }

                    agentsData[agentType][brainType] = agentData;
                }
            }

            return agentsData;
        }
    }
}