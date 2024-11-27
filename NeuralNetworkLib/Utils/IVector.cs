namespace NeuralNetworkLib.Utils
{
    public interface IVector : IEquatable<IVector>
    {
        float X { get; set; }
        float Y { get; set; }

        IVector Normalized();
        float Distance(IVector other);

        /// <summary>
        /// Calculates the Euclidean distance between two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The distance between the two vectors as a float.</returns>
        public static float Distance(IVector a, IVector b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Multiplies a vector by a scalar value.
        /// </summary>
        /// <param name="vector">The vector to scale.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>A new vector scaled by the scalar value.</returns>
        public static MyVector operator *(IVector? vector, float scalar)
        {
            if (vector == null) return new MyVector();
            return new MyVector(vector.X * scalar, vector.Y * scalar);
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A new vector representing the sum of the two vectors.</returns>
        public static MyVector operator +(IVector a, IVector b)
        {
            float x1 = a?.X ?? 0;
            float y1 = a?.Y ?? 0;
            float x2 = b?.X ?? 0;
            float y2 = b?.Y ?? 0;

            return new MyVector(x1 + x2, y1 + y2);
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The vector to subtract from.</param>
        /// <param name="b">The vector to subtract.</param>
        /// <returns>A new vector representing the difference of the two vectors.</returns>
        public static MyVector operator -(IVector a, IVector b)
        {
            if (a == null || b == null)
            {
                return MyVector.zero();
            }

            return new MyVector(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Divides a vector by an integer scalar value.
        /// </summary>
        /// <param name="a">The vector to divide.</param>
        /// <param name="integer">The scalar divisor.</param>
        /// <returns>A new vector representing the division result.</returns
        public static MyVector operator /(IVector a, int integer)
        {
            if (a == null)
            {
                return MyVector.zero();
            }

            return new MyVector(a.X / integer, a.Y / integer);
        }

        /// <summary>
        /// Checks if one vector is less than another based on its coordinates.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>True if both coordinates of vector a are less than those of vector b; otherwise, false.</returns>
        public static bool operator <(IVector a, IVector b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            
            return (a.X < b.X && a.Y < b.Y);
        }

        /// <summary>
        /// Checks if one vector is greater than another based on its coordinates.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>True if both coordinates of vector a are greater than those of vector b; otherwise, false.</returns>
        public static bool operator >(IVector a, IVector b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            return (a.X > b.X && a.Y > b.Y);
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The dot product of the two vectors as a float.</returns>
        static float Dot(IVector a, IVector b)
        {
            if (a == null || b == null) return 0;
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        /// Calculates the magnitude (length) of the vector.
        /// </summary>
        /// <returns>The magnitude of the vector as a float.</returns>
        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The squared distance between the two vectors as a float.</returns>
        static float DistanceSquared(IVector a, IVector b)
        {
            float deltaX = a.X - b.X;
            float deltaY = a.Y - b.Y;
            return deltaX * deltaX + deltaY * deltaY;
        }
    }

    public class MyVector : IVector, IEquatable<MyVector>
    {
        public float X { get; set; }
        public float Y { get; set; }

        /// <summary>
        /// Constructs a new vector with specified X and Y values.
        /// </summary>
        /// <param name="x">The X-coordinate of the vector.</param>
        /// <param name="y">The Y-coordinate of the vector.</param>
        public MyVector(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructs a default vector with X and Y set to 0.
        /// </summary>
        public MyVector()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Constructs a default vector with X and Y set to 0.
        /// </summary>
        public IVector Normalized()
        {
            float magnitude = (float)Math.Sqrt(X * X + Y * Y);
            return magnitude == 0 ? new MyVector(0, 0) : new MyVector(X / magnitude, Y / magnitude);
        }

        /// <summary>
        /// Calculates the distance between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The distance between the two vectors as a float.</returns>
        public float Distance(IVector other)
        {
            return (float)Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
        }

        /// <summary>
        /// Calculates the distance between this vector and another vector.
        /// </summary>
        /// <param name="a">The a vector.</param>
        /// <param name="b">The b vector.</param>
        /// <returns>The distance between the two vectors as a float.</returns>
        public static float Distance(IVector a, IVector b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Adds two MyVector instances component-wise (X and Y) and returns a new MyVector with the result.
        /// </summary>
        /// <param name="a">The first MyVector.</param>
        /// <param name="b">The second MyVector.</param>
        /// <returns>A new MyVector representing the sum of the two vectors.</returns>
        public static MyVector operator +(MyVector a, MyVector b)
        {
            return new MyVector(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// Subtracts the components of one MyVector from another and returns a new MyVector with the result.
        /// </summary>
        /// <param name="a">The first MyVector.</param>
        /// <param name="b">The second MyVector.</param>
        /// <returns>A new MyVector representing the difference between the two vectors.</returns>
        public static MyVector operator -(MyVector a, MyVector b)
        {
            return new MyVector(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Divides the components of a MyVector by a scalar and returns a new MyVector with the result.
        /// </summary>
        /// <param name="a">The MyVector to be divided.</param>
        /// <param name="scalar">The scalar value by which the vector will be divided.</param>
        /// <returns>A new MyVector representing the division result.</returns>
        public static MyVector operator /(MyVector a, float scalar)
        {
            return new MyVector(a.X / scalar, a.Y / scalar);
        }

        // <summary>
        /// Multiplies the components of a MyVector by a scalar and returns a new MyVector with the result.
        /// Returns zero() if the vector is null.
        /// </summary>
        /// <param name="a">The MyVector to be multiplied.</param>
        /// <param name="scalar">The scalar value by which the vector will be multiplied.</param>
        /// <returns>A new MyVector representing the multiplication result, or zero() if the vector is null.</returns>
        public static MyVector operator *(MyVector? a, float scalar)
        {
            if (a == null) return zero();
            return new MyVector(a.X * scalar, a.Y * scalar);
        }

        /// <summary>
        /// Computes the dot product of two MyVector instances and returns the result as a float.
        /// </summary>
        /// <param name="a">The first MyVector.</param>
        /// <param name="b">The second MyVector.</param>
        /// <returns>The dot product of the two vectors as a float.</returns>
        public static float Dot(MyVector a, MyVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        /// Returns a vector representing (0, 0).
        /// </summary>
        /// <returns>A vector with both X and Y set to 0.</returns>
        public static MyVector zero()
        {
            return new MyVector(0, 0);
        }

        /// <summary>
        /// Returns a special vector representing (-1, -1), used to indicate no target.
        /// </summary>
        /// <returns>A vector with both X and Y set to -1.</returns>
        public static MyVector NoTarget()
        {
            return new MyVector(-1, -1);
        }

        /// <summary>
        /// Checks equality between this vector and another vector based on their coordinates.
        /// </summary>
        /// <param name="other">The other vector to compare.</param>
        /// <returns>True if both vectors are equal; otherwise, false.</returns>
        public bool Equals(IVector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <summary>
        /// Determines whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>True if the current object is equal to the specified object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MyVector)obj);
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// Determines whether the current vector is equal to another vector.
        /// </summary>
        /// <param name="other">The MyVector to compare with the current instance.</param>
        /// <returns>True if the current vector is equal to the specified vector; otherwise, false.</returns>
        public bool Equals(MyVector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <summary>
        /// Calculates the magnitude (length) of the vector.
        /// </summary>
        /// <returns>The magnitude of the vector as a float.</returns>
        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
    }
}