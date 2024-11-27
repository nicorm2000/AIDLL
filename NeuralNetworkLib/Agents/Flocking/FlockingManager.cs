using NeuralNetworkLib.Utils;

namespace NeuralNetworkLib.Agents.Flocking
{
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class FlockingManager
    {
        /// <summary>
        /// Calculates the alignment vector for the given boid based on its nearby boids. 
        /// If there are nearby boids, the boid aligns with their average forward direction.
        /// </summary>
        /// <param name="boid">
        /// The <see cref="SimBoid"/> instance for which the alignment behavior is being calculated.
        /// </param>
        /// <returns>
        /// A <see cref="IVector"/> representing the normalized alignment direction of the boid, 
        /// or the boid's forward direction if there are no nearby boids.
        /// </returns>
        public IVector Alignment(SimBoid boid)
        {
            if (boid.NearBoids.Count == 0) return boid.transform.forward;

            IVector avg = MyVector.zero();
            foreach (ITransform<IVector> b in boid.NearBoids)
            {
                avg += b.forward;
            }

            avg /= boid.NearBoids.Count;
            return EnsureValidVector(avg.Normalized());
        }

        /// <summary>
        /// Calculates the cohesion vector for the given boid based on the positions of its nearby boids. 
        /// The boid moves towards the average position of nearby boids, helping it to stay in the group.
        /// </summary>
        /// <param name="boid">
        /// The <see cref="SimBoid"/> instance for which the cohesion behavior is being calculated.
        /// </param>
        /// <returns>
        /// A <see cref="IVector"/> representing the direction the boid should move to maintain group cohesion.
        /// </returns>
        public IVector Cohesion(SimBoid boid)
        {
            if (boid.NearBoids.Count == 0) return MyVector.zero();

            IVector avg = MyVector.zero();
            foreach (ITransform<IVector> b in boid.NearBoids)
            {
                avg += b.position;
            }

            avg /= boid.NearBoids.Count;
            MyVector average = avg - boid.transform.position;
            return EnsureValidVector(average.Normalized());
        }

        /// <summary>
        /// Calculates the cohesion vector for the given boid based on the positions of its nearby boids. 
        /// The boid moves towards the average position of nearby boids, helping it to stay in the group.
        /// </summary>
        /// <param name="boid">
        /// The <see cref="SimBoid"/> instance for which the cohesion behavior is being calculated.
        /// </param>
        /// <returns>
        /// A <see cref="IVector"/> representing the direction the boid should move to maintain group cohesion.
        /// </returns>
        public IVector Separation(SimBoid boid)
        {
            if (boid.NearBoids.Count == 0) return MyVector.zero();

            IVector avg = MyVector.zero();
            foreach (ITransform<IVector> b in boid.NearBoids)
            {
                avg += boid.transform.position - b.position;
            }

            avg /= boid.NearBoids.Count;
            return EnsureValidVector(avg.Normalized());
        }

        /// <summary>
        /// Retrieves the direction vector of the boid based on its current forward direction. 
        /// This is used as the base for behavior calculations that rely on directionality.
        /// </summary>
        /// <param name="boid">
        /// The <see cref="SimBoid"/> instance for which the direction behavior is being calculated.
        /// </param>
        /// <returns>
        /// A <see cref="IVector"/> representing the forward direction of the boid.
        /// </returns>
        public IVector Direction(SimBoid boid)
        {
            return EnsureValidVector(boid.transform.forward);
        }

        /// <summary>
        /// Ensures that the given vector is valid (i.e., not null or containing NaN values).
        /// If the vector is invalid, it returns a zero vector.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="IVector"/> to validate.
        /// </param>
        /// <returns>
        /// A valid <see cref="IVector"/>. If the input vector is invalid, a zero vector is returned.
        /// </returns>
        private IVector EnsureValidVector(IVector vector)
        {
            
            if (vector == null || float.IsNaN(vector.X) || float.IsNaN(vector.Y))
            {
                return MyVector.zero();
            }

            return vector;
        }
    }
}