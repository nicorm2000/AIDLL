using NeuralNetworkLib.Utils;

public class SimNode<Coordinate> : INode, INode<Coordinate>, IEquatable<INode<Coordinate>>
    where Coordinate : IEquatable<Coordinate>
{
    private Coordinate coordinate;
    private int cost;
    public int Food { get; set; }

    private ICollection<INode<Coordinate>> neighbors;

    /// <summary>
    /// Default constructor for SimNode.
    /// </summary>
    public SimNode()
    {
    }

    /// <summary>
    /// Constructor that initializes the SimNode with a specific coordinate.
    /// </summary>
    /// <param name="coord">The coordinate to assign to the node.</param>
    public SimNode(Coordinate coord)
    {
        coordinate = coord;
    }

    /// <summary>
    /// Gets or sets the type of the node (e.g., block type, passable type).
    /// </summary>
    public SimNodeType NodeType { get; set; }

    /// <summary>
    /// Checks equality with another node based on coordinates.
    /// </summary>
    /// <param name="other">The node to compare with.</param>
    /// <returns>True if coordinates are equal, otherwise false.</returns>
    public bool Equals(INode<Coordinate> other)
    {
        return other != null && coordinate.Equals(other.GetCoordinate());
    }

    /// <summary>
    /// Determines if the node is blocked (default implementation returns false).
    /// </summary>
    /// <returns>False by default; can be overridden.</returns>
    public bool IsBlocked()
    {
        return false;
    }

    /// <summary>
    /// Sets the coordinate of the node.
    /// </summary>
    /// <param name="coordinate">The new coordinate to assign.</param>
    public void SetCoordinate(Coordinate coordinate)
    {
        this.coordinate = coordinate;
    }

    /// <summary>
    /// Retrieves the coordinate of the node.
    /// </summary>
    /// <returns>The coordinate of the node.</returns>
    public Coordinate GetCoordinate()
    {
        return coordinate;
    }

    /// <summary>
    /// Sets the neighboring nodes for this node.
    /// </summary>
    /// <param name="neighbors">The collection of neighboring nodes.</param>
    public void SetNeighbors(ICollection<INode<Coordinate>> neighbors)
    {
        this.neighbors = neighbors;
    }

    /// <summary>
    /// Retrieves the neighboring nodes.
    /// </summary>
    /// <returns>A collection of neighboring nodes.</returns>
    public ICollection<INode<Coordinate>> GetNeighbors()
    {
        return neighbors;
    }

    /// <summary>
    /// Gets the cost associated with this node.
    /// </summary>
    /// <returns>The cost of the node.</returns>
    public int GetCost()
    {
        return cost;
    }

    /// <summary>
    /// Sets a new cost for this node.
    /// </summary>
    /// <param name="newCost">The cost to set.</param>
    public void SetCost(int newCost)
    {
        cost = newCost;
    }

    /// <summary>
    /// Checks equality with a coordinate.
    /// </summary>
    /// <param name="other">The coordinate to compare with.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public bool Equals(Coordinate other)
    {
        return coordinate.Equals(other);
    }

    /// <summary>
    /// Checks equality with another node based on coordinates.
    /// </summary>
    /// <param name="other">The node to compare with.</param>
    /// <returns>True if coordinates are equal, otherwise false.</returns>
    public bool EqualsTo(INode<Coordinate> other)
    {
        return coordinate.Equals(other.GetCoordinate());
    }

    /// <summary>
    /// Protected equality comparison with another SimNode.
    /// </summary>
    /// <param name="other">The other SimNode to compare.</param>
    /// <returns>True if equal, otherwise false.</returns>
    protected bool Equals(SimNode<Coordinate> other)
    {
        return coordinate.Equals(other.coordinate);
    }

    /// <summary>
    /// Overrides the default object equality check.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SimNode<Coordinate>)obj);
    }

    /// <summary>
    /// Generates a hash code for the node based on its coordinate.
    /// </summary>
    /// <returns>The hash code of the node.</returns>
    public override int GetHashCode()
    {
        return EqualityComparer<Coordinate>.Default.GetHashCode(coordinate);
    }
}