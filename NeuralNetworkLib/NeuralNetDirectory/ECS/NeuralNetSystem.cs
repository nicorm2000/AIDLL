using NeuralNetworkLib.NeuralNetDirectory.ECS.Patron;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;

namespace NeuralNetworkLib.NeuralNetDirectory.ECS
{
    public sealed class NeuralNetSystem : ECSSystem
    {
        private ParallelOptions parallelOptions;
        private IDictionary<uint, NeuralNetComponent> neuralNetworkComponents;
        private IDictionary<uint, OutputComponent> outputComponents;
        private IDictionary<uint, InputComponent> inputComponents;
        private IEnumerable<uint> queriedEntities;

        /// <summary>
        /// Initializes the system, setting the maximum degree of parallelism for processing.
        /// </summary>
        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        /// <summary>
        /// Deinitializes the system, clearing references to neural network components, output components, input components, and queried entities.
        /// </summary>
        public override void Deinitialize()
        {
            neuralNetworkComponents = null;
            outputComponents = null;
            inputComponents = null;
            queriedEntities = null;
        }

        /// <summary>
        /// Prepares the system for execution by retrieving necessary components and entities from the ECS manager.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame or tick.</param>
        protected override void PreExecute(float deltaTime)
        {
            neuralNetworkComponents ??= ECSManager.GetComponents<NeuralNetComponent>();
            outputComponents ??= ECSManager.GetComponents<OutputComponent>();
            inputComponents ??= ECSManager.GetComponents<InputComponent>();
            queriedEntities ??= ECSManager.GetEntitiesWithComponentTypes(
                typeof(NeuralNetComponent), typeof(OutputComponent), typeof(InputComponent));
        }

        /// <summary>
        /// Executes the system's logic in parallel, processing each queried entity's neural network components, input components, and calculating the outputs.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame or tick.</param>
        protected override void Execute(float deltaTime)
        {
            const int MaxBrains = 3;
            Parallel.ForEach(queriedEntities, parallelOptions, entityId =>
            {
                Parallel.For(0, MaxBrains, i =>
                {
                    NeuralNetComponent neuralNetwork = neuralNetworkComponents[entityId];
                    float[][] inputs = inputComponents[entityId].inputs;
                    float[] outputs = new float[3];
                    for (int j = 0; j < neuralNetwork.Layers[i].Count; j++)
                    {
                        outputs = Synapsis(neuralNetwork.Layers[i][j], inputs[i]);
                        inputs[i] = outputs;
                    }

                    if ((int)neuralNetwork.Layers[i][^1].OutputsCount != outputs.Length) return;

                    outputComponents[entityId].Outputs[i] = outputs;
                });
            });
        }

        /// <summary>
        /// Post-execution method, called after the main execution logic. Currently does not perform any actions.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame or tick.</param>
        protected override void PostExecute(float deltaTime)
        {
        }

        /// <summary>
        /// Computes the outputs of a neural network layer by applying the synapse function (weighted sum and bias) to the inputs, followed by the Tanh activation function.
        /// </summary>
        /// <param name="layer">The neural network layer to compute outputs for.</param>
        /// <param name="inputs">The inputs to the layer.</param>
        /// <returns>The computed outputs for the layer.</returns>
        private float[] Synapsis(NeuronLayer layer, float[] inputs)
        {
            float[] outputs = new float[(int)layer.NeuronsCount];
            Parallel.For(0, (int)layer.NeuronsCount, parallelOptions, j =>
            {
                float a = 0;
                for (int i = 0; i < inputs.Length; i++)
                {
                    a += layer.neurons[j].weights[i] * inputs[i];
                }
                a += layer.neurons[j].bias;
                outputs[j] = (float)Math.Tanh(a);
            });
            return outputs;
        }
    }
}