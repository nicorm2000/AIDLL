namespace NeuralNetworkLib.Utils
{
    public class Sim2Graph : SimGraph<SimNode<IVector>, SimCoordinate, IVector>
    {
        /// <summary>
        /// Defines parallel options for managing the degree of parallelism in the graph's operations.
        /// </summary>
        private readonly ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 32
        };

        public int MinX => 0;
        public int MaxX => CoordNodes.GetLength(0);
        public int MinY => 0;
        public int MaxY => CoordNodes.GetLength(1);

        /// <summary>
        /// Initializes a new instance of the Sim2Graph class, inheriting properties and behavior from its base class.
        /// </summary>
        /// <param name="x">The number of cells along the x-axis.</param>
        /// <param name="y">The number of cells along the y-axis.</param>
        /// <param name="cellSize">The size of each cell in the graph.</param>
        public Sim2Graph(int x, int y, float cellSize) : base(x, y, cellSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Sim2Graph class, inheriting properties and behavior from its base class.
        /// </summary>
        /// <param name="x">The number of cells along the x-axis.</param>
        /// <param name="y">The number of cells along the y-axis.</param>
        /// <param name="cellSize">The size of each cell in the graph.</param>
        public override void CreateGraph(int x, int y, float cellSize)
        {
            CoordNodes = new SimCoordinate[x, y];

            Parallel.For(0, x, parallelOptions, i =>
            {
                for (int j = 0; j < y; j++)
                {
                    SimCoordinate node = new SimCoordinate();
                    node.SetCoordinate(i * cellSize, j * cellSize);
                    CoordNodes[i, j] = node;

                    SimNode<IVector> nodeType = new SimNode<IVector>();
                    nodeType.SetCoordinate(new MyVector(i * cellSize, j * cellSize));
                    NodesType[i, j] = nodeType;
                }
            });
        }

        /// <summary>
        /// Determines if a given position is within the boundaries of the graph.
        /// </summary>
        /// <param name="position">The position to check, represented as an <see cref="IVector"/>.</param>
        /// <returns>
        /// True if the position is within the graph's borders; otherwise, false.
        /// </returns>
        public bool IsWithinGraphBorders(IVector position)
        {
            return position.X >= MinX && position.X <= MaxX - 1 &&
                   position.Y >= MinY && position.Y <= MaxY - 1;
        }
    }
}