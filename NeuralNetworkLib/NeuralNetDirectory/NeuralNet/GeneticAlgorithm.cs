namespace NeuralNetworkLib.NeuralNetDirectory.NeuralNet
{
    public class Genome
    {
        private static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

        public float fitness;
        public float[] genome;

        /// <summary>
        /// Initializes a new Genome object with a predefined set of genes and a fitness value.
        /// </summary>
        /// <param name="genes">The array of gene values that make up the genome.</param>
        public Genome(float[] genes)
        {
            genome = genes;
            fitness = 0;
        }

        /// <summary>
        /// Initializes a new Genome object with a specified number of genes, where each gene is randomly initialized.
        /// The fitness value is set to 0.
        /// </summary>
        /// <param name="genesCount">The number of genes to initialize in the genome.</param>
        public Genome(int genesCount)
        {
            genome = new float[genesCount];

            for (int j = 0; j < genesCount; j++)
                genome[j] = (float)(random.Value.NextDouble() * 2.0 - 1.0);

            fitness = 0;
        }

        /// <summary>
        /// Initializes a new empty Genome object with no genes and a fitness value set to 0.
        /// </summary>
        public Genome()
        {
            fitness = 0;
        }
    }

    public class GeneticAlgorithm
    {
        private static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());
        private readonly List<Genome> newPopulation = new List<Genome>();
        private readonly List<Genome> population = new List<Genome>();

        private readonly int eliteCount;
        private readonly float mutationChance;
        private readonly float mutationRate;

        private float totalFitness;

        /// <summary>
        /// Initializes a new instance of the GeneticAlgorithm class with the specified parameters for elite count, mutation chance, and mutation rate.
        /// </summary>
        /// <param name="eliteCount">The number of top genomes to keep in the population.</param>
        /// <param name="mutationChance">The probability that a mutation will occur.</param>
        /// <param name="mutationRate">The rate of change during mutation.</param>
        public GeneticAlgorithm(int eliteCount, float mutationChance, float mutationRate)
        {
            this.eliteCount = eliteCount;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;
        }

        /// <summary>
        /// Generates a specified number of random genomes, each with a given number of genes.
        /// </summary>
        /// <param name="count">The number of genomes to generate.</param>
        /// <param name="genesCount">The number of genes each genome should have.</param>
        /// <returns>An array of randomly generated genomes.</returns>
        public Genome[] GetRandomGenomes(int count, int genesCount)
        {
            Genome[] genomes = new Genome[count];

            for (int i = 0; i < count; i++) genomes[i] = new Genome(genesCount);

            return genomes;
        }

        /// <summary>
        /// Runs a single epoch of the genetic algorithm, evolving the population based on the old genomes and creating a new population.
        /// </summary>
        /// <param name="oldGenomes">The current population of genomes.</param>
        /// <param name="totalCount">The total number of genomes desired in the new population.</param>
        /// <returns>A list of genomes representing the new population after evolution.</returns>
        public List<Genome> Epoch(Genome[] oldGenomes, int totalCount)
        {
            totalFitness = 0;

            population.Clear();
            newPopulation.Clear();

            population.AddRange(oldGenomes);
            population.Sort(HandleComparison);

            foreach (Genome g in population) totalFitness += g.fitness;

            SelectElite();

            while (newPopulation.Count < (population.Count > 0 ? totalCount : 0)) Crossover();

            return newPopulation;
        }

        /// <summary>
        /// Selects the elite genomes (top performers) and adds them to the new population.
        /// </summary>
        private void SelectElite()
        {
            for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
                newPopulation.Add(population[i]);
        }

        /// <summary>
        /// Performs crossover between two selected parent genomes to produce two child genomes and adds them to the new population.
        /// </summary>
        private void Crossover()
        {
            const int maxRetries = 10;

            Genome mom = RouletteSelection();
            Genome dad = RouletteSelection();

            for (int i = 0; i < maxRetries; i++)
            {
                mom ??= RouletteSelection();
                dad ??= RouletteSelection();

                if (mom != null && dad != null)
                    break;
            }

            if (mom == null || dad == null) return;

            UniformCrossover(mom, dad, out Genome child1, out Genome child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        /// <summary>
        /// Performs a single-pivot crossover between two parent genomes, with optional mutation, to generate two child genomes.
        /// </summary>
        /// <param name="mom">The first parent genome.</param>
        /// <param name="dad">The second parent genome.</param>
        /// <param name="child1">The first child genome after crossover.</param>
        /// <param name="child2">The second child genome after crossover.</param>
        private void SinglePivotCrossover(Genome mom, Genome dad, out Genome child1, out Genome child2)
        {
            child1 = new Genome();
            child2 = new Genome();

            child1.genome = new float[mom.genome.Length];
            child2.genome = new float[mom.genome.Length];

            int pivot = random.Value.Next(0, mom.genome.Length);

            for (int i = 0; i < pivot; i++)
            {
                child1.genome[i] = mom.genome[i];

                if (ShouldMutate())
                    child1.genome[i] += (float)(random.Value.NextDouble() * 2.0 - 1.0);

                child2.genome[i] = dad.genome[i];

                if (ShouldMutate())
                    child2.genome[i] += (float)(random.Value.NextDouble() * 2.0 - 1.0);
            }

            for (int i = pivot; i < mom.genome.Length; i++)
            {
                child2.genome[i] = mom.genome[i];

                if (ShouldMutate())
                    child2.genome[i] += (float)(random.Value.NextDouble() * 2.0 - 1.0);

                child1.genome[i] = dad.genome[i];

                if (ShouldMutate())
                    child1.genome[i] += (float)(random.Value.NextDouble() * 2.0 - 1.0);
            }
        }

        /// <summary>
        /// Performs a double-pivot crossover between two parent genomes, with optional mutation, to generate two child genomes.
        /// </summary>
        /// <param name="parent1">The first parent genome.</param>
        /// <param name="parent2">The second parent genome.</param>
        /// <param name="child1">The first child genome after crossover.</param>
        /// <param name="child2">The second child genome after crossover.</param>
        public void DoublePivotCrossover(Genome parent1, Genome parent2, out Genome child1, out Genome child2)
        {
            List<float> parent1Chromosome = parent1.genome.ToList();
            List<float> parent2Chromosome = parent2.genome.ToList();

            int chromosomeLength = parent1Chromosome.Count - 1;

            int locus = random.Value.Next(0, chromosomeLength);
            int length = random.Value.Next(0, (int)Math.Ceiling(chromosomeLength / 2.0));

            List<float> child1Chromosome = new List<float>();
            List<float> child2Chromosome = new List<float>();

            if (locus + length > chromosomeLength)
            {
                child1Chromosome.AddRange(parent2Chromosome.GetRange(0, (locus + length) % chromosomeLength));
                child1Chromosome.AddRange(parent1Chromosome.GetRange((locus + length) % chromosomeLength, locus));
                child1Chromosome.AddRange(parent2Chromosome.GetRange(locus, parent2Chromosome.Count - locus));

                child2Chromosome.AddRange(parent1Chromosome.GetRange(0, (locus + length) % chromosomeLength));
                child2Chromosome.AddRange(parent2Chromosome.GetRange((locus + length) % chromosomeLength, locus));
                child2Chromosome.AddRange(parent1Chromosome.GetRange(locus, parent1Chromosome.Count - locus));
            }
            else
            {
                child1Chromosome.AddRange(parent1Chromosome.GetRange(0, locus));
                child1Chromosome.AddRange(parent2Chromosome.GetRange(locus, length));
                child1Chromosome.AddRange(parent1Chromosome.GetRange(locus + length,
                    parent1Chromosome.Count - (locus + length)));

                child2Chromosome.AddRange(parent2Chromosome.GetRange(0, locus));
                child2Chromosome.AddRange(parent1Chromosome.GetRange(locus, length));
                child2Chromosome.AddRange(parent2Chromosome.GetRange(locus + length,
                    parent2Chromosome.Count - (locus + length)));
            }

            child1 = new Genome(child1Chromosome.GetRange(0, child1Chromosome.Count - 1).ToArray());
            child2 = new Genome(child2Chromosome.GetRange(0, child2Chromosome.Count - 1).ToArray());
        }

        /// <summary>
        /// Performs uniform crossover between two parent genomes, with optional mutation, to generate two child genomes.
        /// </summary>
        /// <param name="parent1">The first parent genome.</param>
        /// <param name="parent2">The second parent genome.</param>
        /// <param name="child1">The first child genome after crossover.</param>
        /// <param name="child2">The second child genome after crossover.</param>
        private void UniformCrossover(Genome parent1, Genome parent2, out Genome child1, out Genome child2)
        {
            child1 = new Genome();
            child2 = new Genome();
            child1.genome = new float[parent1.genome.Length];
            child2.genome = new float[parent1.genome.Length];

            float selectionChance = 0.5f;

            for (int i = 0; i < parent1.genome.Length; i++)
            {
                if (random.Value.NextDouble() < selectionChance)
                {
                    child1.genome[i] = parent1.genome[i];
                    child2.genome[i] = parent2.genome[i];
                }
                else
                {
                    child1.genome[i] = parent2.genome[i];
                    child2.genome[i] = parent1.genome[i];
                }
            }
        }

        /// <summary>
        /// Determines whether a mutation should occur based on the mutation chance.
        /// </summary>
        /// <returns>True if a mutation should occur; otherwise, false.</returns>
        private bool ShouldMutate()
        {
            return random.Value.NextDouble() < mutationChance;
        }

        /// <summary>
        /// Compares two genomes based on their fitness values for sorting purposes.
        /// </summary>
        /// <param name="x">The first genome to compare.</param>
        /// <param name="y">The second genome to compare.</param>
        /// <returns>A comparison value indicating the relative fitness of the genomes.</returns>
        private static int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }

        /// <summary>
        /// Performs roulette wheel selection, choosing a genome based on its fitness proportion relative to the total population's fitness.
        /// </summary>
        /// <returns>A randomly selected genome based on fitness probability.</returns>
        public Genome RouletteSelection()
        {
            float rnd = (float)(random.Value.NextDouble() * Math.Max(totalFitness, 0));

            float fitness = 0;

            for (int i = 0; i < population.Count; i++)
            {
                fitness += Math.Max(population[i].fitness, 0);
                if (fitness >= rnd)
                    return population[i];
            }

            return null;
        }
    }
}