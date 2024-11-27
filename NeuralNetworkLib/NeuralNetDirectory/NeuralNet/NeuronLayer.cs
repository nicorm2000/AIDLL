using NeuralNetworkLib.DataManagement;

namespace NeuralNetworkLib.NeuralNetDirectory.NeuralNet
{
    
    public class NeuronLayer
    {
        public BrainType BrainType;
        public SimAgentTypes AgentType;
        public float Bias {  get; set; }= 1;
        private readonly float p = 0.5f;
        public Neuron[] neurons;
        private float[] outputs;
        public float InputsCount { get; }
        public float NeuronsCount => neurons.Length;
        public float OutputsCount => outputs.Length;

        /// <summary>
        /// Constructor to initialize a new instance of the NeuronLayer class, setting the number of inputs, 
        /// neurons, bias, and a parameter 'p' for the layer. It also calls the method to initialize the neurons 
        /// based on the number of neurons provided.
        /// </summary>
        /// <param name="inputsCount">The number of inputs each neuron in the layer will receive.</param>
        /// <param name="neuronsCount">The number of neurons in this layer.</param>
        /// <param name="bias">The bias value applied to each neuron in the layer.</param>
        /// <param name="p">A parameter used for additional layer configuration, purpose unclear in this context.</param>
        public NeuronLayer(float inputsCount, int neuronsCount, float bias, float p)
        {
            InputsCount = inputsCount;
            this.Bias = bias;
            this.p = p;

            SetNeuronsCount(neuronsCount);
        }

        /// <summary>
        /// Initializes the neurons in the layer by creating a new array of neurons with the specified count. 
        /// Each neuron is initialized with the number of inputs and bias. Additionally, an output array 
        /// is created to store the output values for each neuron in the layer.
        /// </summary>
        /// <param name="neuronsCount">The number of neurons to initialize in the layer.</param>
        private void SetNeuronsCount(int neuronsCount)
        {
            neurons = new Neuron[neuronsCount];

            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i] = new Neuron(InputsCount, Bias);
            }

            outputs = new float[neurons.Length];
        }
    }
}