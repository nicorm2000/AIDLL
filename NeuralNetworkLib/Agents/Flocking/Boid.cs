using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.Flocking
{
    public class Boid<TVector, TTransform> 
        where TVector : IVector, IEquatable<TVector>
        where TTransform : ITransform<TVector>, new()
    {
        public float detectionRadious = 6.0f;
        public float alignmentOffset;
        public float cohesionOffset;
        public float separationOffset;
        public float directionOffset;
        public IVector target = new MyVector();
        public TTransform transform = new TTransform();

        private Func<Boid<TVector, TTransform>, TVector> alignment;
        private Func<Boid<TVector, TTransform>, TVector> cohesion;
        private Func<Boid<TVector, TTransform>, TVector> separation;
        private Func<Boid<TVector, TTransform>, TVector> direction;
        public List<ITransform<IVector>> NearBoids { get; set; }

        /// <summary>
        /// Initializes the boid behavior functions (alignment, cohesion, separation, and direction) 
        /// by accepting four function delegates that define the respective behaviors.
        /// </summary>
        /// <param name="Alignment">
        /// A <see cref="Func{Boid{TVector, TTransform}, TVector}"/> that defines the alignment behavior for the boid.
        /// </param>
        /// <param name="Cohesion">
        /// A <see cref="Func{Boid{TVector, TTransform}, TVector}"/> that defines the cohesion behavior for the boid.
        /// </param>
        /// <param name="Separation">
        /// A <see cref="Func{Boid{TVector, TTransform}, TVector}"/> that defines the separation behavior for the boid.
        /// </param>
        /// <param name="Direction">
        /// A <see cref="Func{Boid{TVector, TTransform}, TVector}"/> that defines the direction behavior for the boid.
        /// </param>
        public void Init(Func<Boid<TVector, TTransform>, TVector> Alignment,
            Func<Boid<TVector, TTransform>, TVector> Cohesion,
            Func<Boid<TVector, TTransform>, TVector> Separation,
            Func<Boid<TVector, TTransform>, TVector> Direction)
        {
            this.alignment = Alignment;
            this.cohesion = Cohesion;
            this.separation = Separation;
            this.direction = Direction;
        }

        /// <summary>
        /// Calculates the acceleration (ACS) for the boid by combining the individual behaviors
        /// (alignment, cohesion, separation, and direction) with their respective offsets.
        /// The result is normalized to give the final acceleration vector.
        /// </summary>
        /// <returns>
        /// A normalized <see cref="IVector"/> representing the combined acceleration for the boid, 
        /// which influences its movement based on its surrounding environment and behavior functions.
        /// </returns>
        public IVector ACS()
        {
            IVector ACS = (alignment(this) * alignmentOffset) +
                          (cohesion(this) * cohesionOffset) +
                          (separation(this) * separationOffset) +
                          (direction(this) * directionOffset);
            return ACS.Normalized();
        }

        /// <summary>
        /// Retrieves the direction vector of the boid based on its direction behavior function.
        /// </summary>
        /// <returns>
        /// A <see cref="TVector"/> representing the direction the boid is currently moving towards.
        /// </returns>
        public TVector GetDirection()
        {
            return direction(this);
        }

        /// <summary>
        /// Retrieves the alignment vector of the boid based on its alignment behavior function.
        /// </summary>
        /// <returns>
        /// A <see cref="TVector"/> representing the alignment behavior of the boid, 
        /// which helps it align with other nearby boids.
        /// </returns>
        public TVector GetAlignment()
        {
            return alignment(this);
        }

        /// <summary>
        /// Retrieves the cohesion vector of the boid based on its cohesion behavior function.
        /// </summary>
        /// <returns>
        /// A <see cref="TVector"/> representing the cohesion behavior of the boid, 
        /// which causes the boid to move towards the center of the nearby boid cluster.
        /// </returns>
        public TVector GetCohesion()
        {
            return cohesion(this);
        }

        /// <summary>
        /// Retrieves the separation vector of the boid based on its separation behavior function.
        /// </summary>
        /// <returns>
        /// A <see cref="TVector"/> representing the separation behavior of the boid, 
        /// which helps prevent crowding by steering away from nearby boids.
        /// </returns>
        public TVector GetSeparation()
        {
            return separation(this);
        }
    }
}