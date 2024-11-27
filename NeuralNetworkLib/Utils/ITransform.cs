namespace NeuralNetworkLib.Utils
{
    public class ITransform<TVector>
        where TVector : IVector, IEquatable<TVector>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ITransform"/> class with the specified position.
        /// </summary>
        /// <param name="position">The position of the transform.</param>
        public ITransform (TVector position)
        {
            this.position = position;
        }
        public TVector position { get; set; }
        public TVector forward;

        /// <summary>
        /// Initializes a new instance of the <see cref="ITransform"/> class with default values for position and forward vectors.
        /// </summary>
        public ITransform()
        {
            position = default(TVector);
            forward = default(TVector);
        }
    }
}