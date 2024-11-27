namespace NeuralNetworkLib.Utils
{
    public abstract class SimGraph<TNodeType, TCoordinateNode, TCoordinateType>
        where TNodeType : INode<TCoordinateType>
        where TCoordinateNode : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        public static TCoordinateNode MapDimensions;
        public static TCoordinateNode OriginPosition;
        public static float CellSize;
        public TCoordinateNode[,] CoordNodes;
        public readonly TNodeType[,] NodesType;

        /// <summary>
        /// Defines parallel options for managing the degree of parallelism in the graph's operations.
        /// </summary>
        private ParallelOptions parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 32
        };

        /// <summary>
        /// Initializes a new instance of the SimGraph class.
        /// Sets the map dimensions, cell size, and creates the graph structure based on the provided dimensions.
        /// </summary>
        /// <param name="x">The number of cells along the x-axis.</param>
        /// <param name="y">The number of cells along the y-axis.</param>
        /// <param name="cellSize">The size of each cell in the graph.</param>
        public SimGraph(int x, int y, float cellSize)
        {
            MapDimensions = new TCoordinateNode();
            MapDimensions.SetCoordinate(x, y);
            CellSize = cellSize;

            CoordNodes = new TCoordinateNode[x, y];
            NodesType = new TNodeType[x, y];

            CreateGraph(x, y, cellSize);

            //AddNeighbors(cellSize);
        }

        /// <summary>
        /// Abstract method to create the graph structure based on the specified dimensions and cell size.
        /// Implementations must define how nodes are initialized and configured.
        /// </summary>
        /// <param name="x">The number of cells along the x-axis.</param>
        /// <param name="y">The number of cells along the y-axis.</param>
        /// <param name="cellSize">The size of each cell in the graph.</param>
        public abstract void CreateGraph(int x, int y, float cellSize);

        /// <summary>
        /// Assigns neighbors to each node in the graph based on proximity.
        /// Uses parallel processing to efficiently determine neighboring nodes.
        /// </summary>
        /// <param name="cellSize">The size of each cell, used to calculate proximity.</param>
        private void AddNeighbors(float cellSize)
        {
            List<INode<TCoordinateType>> neighbors = new List<INode<TCoordinateType>>();

            Parallel.For(0, CoordNodes.GetLength(0), parallelOptions, i =>
            {
                for (int j = 0; j < CoordNodes.GetLength(1); j++)
                {
                    neighbors.Clear();
                    for (int k = 0; k < CoordNodes.GetLength(0); k++)
                    {
                        for (int l = 0; l < CoordNodes.GetLength(1); l++)
                        {
                            if (i == k && j == l) continue;

                            bool isNeighbor =
                                (Approximately(CoordNodes[i, j].GetX(), CoordNodes[k, l].GetX()) &&
                                 Approximately(Math.Abs(CoordNodes[i, j].GetY() - CoordNodes[k, l].GetY()),
                                     cellSize)) ||
                                (Approximately(CoordNodes[i, j].GetY(), CoordNodes[k, l].GetY()) &&
                                 Approximately(Math.Abs(CoordNodes[i, j].GetX() - CoordNodes[k, l].GetX()), cellSize));

                            if (isNeighbor) neighbors.Add(NodesType[k, l]);
                        }
                    }

                    NodesType[i, j].SetNeighbors(new List<INode<TCoordinateType>>(neighbors));
                }
            });
        }

        /// <summary>
        /// Compares two floating-point numbers to determine if they are approximately equal.
        /// </summary>
        /// <param name="a">The first floating-point number.</param>
        /// <param name="b">The second floating-point number.</param>
        /// <returns>True if the numbers are approximately equal; otherwise, false.</returns>
        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}