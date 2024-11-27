namespace NeuralNetworkLib.NeuralNetDirectory.NeuralNet
{
    public class Neuron
    {
        public readonly float bias;
        public readonly float[] weights;

        /// <summary>
        /// Initializes a new instance of the Neuron class, setting up the weights and bias.
        /// The weights are initialized to random values between -1 and 1.
        /// </summary>
        /// <param name="weightsCount">The number of weights for the neuron.</param>
        /// <param name="bias">The bias value for the neuron.</param>
        public Neuron(float weightsCount, float bias)
        {
            weights = new float[(int)weightsCount];

            Random random = new Random();
            for (int i = 0; i < weights.Length; i++) weights[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            this.bias = bias;
        }
    }
}