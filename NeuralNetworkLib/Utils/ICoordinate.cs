namespace NeuralNetworkLib.Utils
{
    public interface ICoordinate<T> : IEquatable<T>
        where T : IEquatable<T>
    {
        void Add(T a);
        T Multiply(float b);
        float GetX();
        float GetY();
        void SetX(float x);
        void SetY(float y);
        float Distance(T b);
        float GetMagnitude();
        T GetCoordinate();
        void SetCoordinate(float x, float y);
        void SetCoordinate(T coordinate);
        void Zero();
        void Perpendicular();
    }

    public class SimCoordinate : ICoordinate<IVector>, IEquatable<SimCoordinate>
    {
        public IVector coordinate = new MyVector();

        /// <summary>
        /// Determines whether the current vector is equal to another vector, within a small epsilon tolerance.
        /// </summary>
        /// <param name="other">The vector to compare against.</param>
        /// <returns>True if the vectors are equal, false otherwise.</returns>
        public bool Equals(IVector other)
        {
            const float epsilon = 0.0001f;
            return other != null && Math.Abs(coordinate.X - other.X) < epsilon && Math.Abs(coordinate.Y - other.Y) < epsilon;
        }

        /// <summary>
        /// Adds another vector to the current vector's coordinates.
        /// </summary>
        /// <param name="a">The vector to add.</param>
        public void Add(IVector a)
        {
            coordinate += a;
        }

        /// <summary>
        /// Multiplies the current vector by a scalar value.
        /// </summary>
        /// <param name="b">The scalar value.</param>
        /// <returns>A new vector resulting from the multiplication.</returns>
        public IVector Multiply(float b)
        {
            return coordinate * b;
        }

        /// <summary>
        /// Gets the X component of the current vector's coordinates.
        /// </summary>
        /// <returns>The X component of the vector.</returns>
        public float GetX()
        {
            return coordinate.X;
        }

        /// <summary>
        /// Gets the Y component of the current vector's coordinates.
        /// </summary>
        /// <returns>The Y component of the vector.</returns>
        public float GetY()
        {
            return coordinate.Y;
        }

        /// <summary>
        /// Sets the X component of the current vector's coordinates.
        /// </summary>
        /// <param name="x">The new X component value.</param>

        public void SetX(float x)
        {
            coordinate.X = x;
        }

        /// <summary>
        /// Sets the X component of the current vector's coordinates.
        /// </summary>
        /// <param name="x">The new X component value.</param>

        public void SetY(float y)
        {
            coordinate.Y = y;
        }

        /// <summary>
        /// Calculates the distance between the current vector and another vector.
        /// </summary>
        /// <param name="b">The vector to measure the distance to.</param>
        /// <returns>The distance as a float.</returns>
        public float Distance(IVector b)
        {
            return MyVector.Distance(coordinate, b);
        }

        /// <summary>
        /// Gets the magnitude (length) of the current vector.
        /// </summary>
        /// <returns>The magnitude of the vector.</returns>
        public float GetMagnitude()
        {
            return (float)Math.Sqrt(coordinate.X * coordinate.X + coordinate.Y * coordinate.Y);
        }

        /// <summary>
        /// Gets the current vector's coordinates.
        /// </summary>
        /// <returns>The coordinate of the vector.</returns>
        public IVector GetCoordinate()
        {
            return coordinate;
        }

        /// <summary>
        /// Sets the current vector's coordinates to the specified values.
        /// </summary>
        /// <param name="x">The new X component value.</param>
        /// <param name="y">The new Y component value.</param>
        public void SetCoordinate(float x, float y)
        {
            coordinate = new MyVector(x, y);
        }

        /// <summary>
        /// Sets the current vector's coordinates to another vector's coordinates.
        /// </summary>
        /// <param name="coordinate">The new coordinate vector.</param>
        public void SetCoordinate(IVector coordinate)
        {
            this.coordinate = coordinate;
        }

        /// <summary>
        /// Sets the current vector's coordinates to zero (0, 0).
        /// </summary>
        public void Zero()
        {
            coordinate = MyVector.zero();
        }

        /// <summary>
        /// Rotates the current vector 90 degrees counterclockwise.
        /// </summary>
        public void Perpendicular()
        {
            coordinate = new MyVector(-coordinate.Y, coordinate.X);
        }

        /// <summary>
        /// Determines whether the current coordinate is equal to another coordinate, within a small epsilon tolerance.
        /// </summary>
        /// <param name="other">The coordinate to compare against.</param>
        /// <returns>True if the coordinates are equal, false otherwise.</returns>
        public bool Equals(SimCoordinate other)
        {
            const float epsilon = 0.0001f;
            return other != null && Math.Abs(coordinate.X - other.GetCoordinate().X) < epsilon && Math.Abs(coordinate.Y - other.GetCoordinate().Y) < epsilon;
        }
    }
}