using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonogram
{
    public class GeneticAlgorithm
    {
        private BoardValues _boardValues;
        private bool _parallel;
        private int _populationSize;
        private int _iterationCount;
        private int _islandCount;
        private List<bool[,]> _initialPopulation;
        private Func<bool[,], double> _fitness;
        private Func<List<double>, int> _selection;
        private Func<List<double>, int> _ruletSelection;
        private Func<List<double>, int> _rankSelection;
        private Func<List<double>, int> _tournamentSelection;
        private Func<bool[,], bool[,], (bool[,], bool[,])> _crossover;
        private Func<bool[,], bool[,], (bool[,], bool[,])> _crossoverOnePoint;
        private Func<bool[,], bool[,], (bool[,], bool[,])> _crossoverTwoPoints;
        private Func<bool[,], bool[,]> _mutation;
        private Func<List<bool[,]>, bool> _termCondition;
        private Func<List<bool[,]>, bool> _iterationTermCondition;
        private Func<List<bool[,]>, bool> _meanTermCondition;
        private Func<List<bool[,]>, bool> _deviationTermCondition;
        private double _crossoverPropability;
        private double _mutationPropability;

        private int _meanTermConditionCount = 100;
        private double _meanTermConditionPrevious = 0;
        private int _deviationTermConditionCount = 100;
        private double _deviationTermConditionPrevious = 0;


        public GeneticAlgorithm(BoardValues boardValues, int populationSize = 10, int iterationCount = 10, double crossoverPropability = 0.9,
            double mutationPropability = 0.1, string crossoverMethod = "OnePoint", string selectionMethod = "Tournament",
            string termConditionMethod = "Iteration", bool parallel = false)
        {
            var rnd = new Random();

            _boardValues = boardValues;
            _parallel = parallel;
            _populationSize = populationSize;
            _crossoverPropability = crossoverPropability;
            _mutationPropability = mutationPropability;
            _iterationCount = iterationCount;

            _initialPopulation = InitialPopulation();

            _fitness = specimen => 1000.0 / (1.0 + BoardHelper.CheckForErrors(_boardValues, specimen));

            _crossoverOnePoint = (a, b) =>
            {
                var crossPoint = rnd.Next(0, a.Length - 1);

                var newA = a.Clone() as bool[,];
                var newB = b.Clone() as bool[,];

                for (var i = crossPoint; i < a.Length; i++)
                {
                    var pointX = i % a.GetLength(0);
                    var pointY = i / a.GetLength(1);

                    newA[pointX, pointY] = b[pointX, pointY];
                    newB[pointX, pointY] = a[pointX, pointY];
                }

                return (newA, newB);
            };

            _crossoverTwoPoints = (a, b) => //two point
            {
                var crossPointA = rnd.Next(0, a.Length - 1);
                var crossPointB = rnd.Next(0, a.Length - 1);

                if (crossPointA < crossPointB)
                {
                    var temp = crossPointA;
                    crossPointA = crossPointB;
                    crossPointB = crossPointA;
                }

                var newA = a.Clone() as bool[,];
                var newB = b.Clone() as bool[,];


                for (var i = crossPointA; i < crossPointB; i++)
                {
                    var pointX = i % a.GetLength(0);
                    var pointY = i / a.GetLength(1);

                    newA[pointX, pointY] = b[pointX, pointY];
                    newB[pointX, pointY] = a[pointX, pointY];
                }

                return (newA, newB);
            };

            _mutation = a =>
            {
                var mutPoint = rnd.Next(0, a.Length - 2);
                var pointX = mutPoint % a.GetLength(0);
                var pointY = mutPoint / a.GetLength(1);

                var newA = a.Clone() as bool[,];

                newA[pointX, pointY] = !newA[pointX, pointY];

                return newA;
            };

            _iterationTermCondition = pop =>
            {
                _iterationCount--;

                return (_iterationCount > 0);
            };

            _meanTermCondition = pop =>
            {
                var mean = pop.Sum(x => _fitness(x)) / pop.Count;

                if (mean == _meanTermConditionPrevious)
                    _meanTermConditionCount--;
                else
                    _meanTermConditionPrevious = mean;

                return (mean < (1000 / pop.Count) - 10) && (_meanTermConditionCount > 0) ? true : false;
            };

            _deviationTermCondition = pop =>
            {
                var mean = pop.Sum(x => _fitness(x)) / pop.Count;

                var w = Math.Sqrt(pop.Sum(x => Math.Pow(_fitness(x) - mean, 2)) / (pop.Count - 1));

                if (w == _deviationTermConditionPrevious)
                    _deviationTermConditionCount--;
                else
                    _deviationTermConditionPrevious = w;

                return w > pop.Count && _deviationTermConditionCount > 0 ? true : false;
            };

            _tournamentSelection = fitnesses => {
                var first = rnd.Next(0, fitnesses.Count - 1);
                var second = rnd.Next(0, fitnesses.Count - 1);

                return fitnesses[first] > fitnesses[second] ? first : second;
            };

            _ruletSelection = fitneses =>
            {
                var sum_fit = fitneses.Sum();
                var u = rnd.NextDouble() * (sum_fit - 0.0) + 0.0;

                for (var i = fitneses.Count - 1; i >= 0; i--)
                {
                    sum_fit -= fitneses[i];

                    if (sum_fit <= u)
                        return i;
                }

                return 0;
            };

            _rankSelection = fitneses =>
            {
                List<(double fitness, int index)> fitnesses_with_index = new List<(double fitness, int index)>();

                for (var i = 0; i < fitneses.Count; i++)
                    fitnesses_with_index.Add((fitneses[i], i));

                fitnesses_with_index = fitnesses_with_index.OrderBy(x => x.fitness).ToList();

                var f = new List<double>(fitneses);
                for (var i = 0; i < fitneses.Count; i++)
                    f[i] = (i * 2) + 1;

                return fitnesses_with_index[_ruletSelection(f)].index;
            };

            if (crossoverMethod == "OnePoint")
                _crossover = _crossoverOnePoint;
            else if (crossoverMethod == "TwoPoints")
                _crossover = _crossoverTwoPoints;

            if (selectionMethod == "Rulet")
                _selection = _ruletSelection;
            else if (selectionMethod == "Rank")
                _selection = _rankSelection;
            else if (selectionMethod == "Tournament")
                _selection = _tournamentSelection;

            if (termConditionMethod == "Iteration")
                _termCondition = _iterationTermCondition;
            else if (termConditionMethod == "Mean")
                _termCondition = _meanTermCondition;
            else if (termConditionMethod == "Deviation")
                _termCondition = _deviationTermCondition;
        }


        public GeneticAlgorithm(BoardValues boardValues, int populationSize = 10, int iterationCount = 10, int islandCount = 5,
            double crossoverPropability = 0.9, double mutationPropability = 0.1, string crossoverMethod = "OnePoint", 
            string selectionMethod = "Tournament", bool parallel = false)
        {
            var rnd = new Random();

            _boardValues = boardValues;
            _parallel = parallel;
            _populationSize = populationSize;
            _crossoverPropability = crossoverPropability;
            _mutationPropability = mutationPropability;
            _iterationCount = iterationCount;
            _islandCount = islandCount;

            _initialPopulation = InitialPopulation();

            _fitness = specimen => 1000.0 / (1.0 + BoardHelper.CheckForErrors(_boardValues, specimen));

            _crossoverOnePoint = (a, b) =>
            {
                var crossPoint = rnd.Next(0, a.Length - 1);

                var newA = a.Clone() as bool[,];
                var newB = b.Clone() as bool[,];

                for (var i = crossPoint; i < a.Length; i++)
                {
                    var pointX = i % a.GetLength(0);
                    var pointY = i / a.GetLength(1);

                    newA[pointX, pointY] = b[pointX, pointY];
                    newB[pointX, pointY] = a[pointX, pointY];
                }

                return (newA, newB);
            };

            _crossoverTwoPoints = (a, b) => //two point
            {
                var crossPointA = rnd.Next(0, a.Length - 1);
                var crossPointB = rnd.Next(0, a.Length - 1);

                if (crossPointA < crossPointB)
                {
                    var temp = crossPointA;
                    crossPointA = crossPointB;
                    crossPointB = crossPointA;
                }

                var newA = a.Clone() as bool[,];
                var newB = b.Clone() as bool[,];


                for (var i = crossPointA; i < crossPointB; i++)
                {
                    var pointX = i % a.GetLength(0);
                    var pointY = i / a.GetLength(1);

                    newA[pointX, pointY] = b[pointX, pointY];
                    newB[pointX, pointY] = a[pointX, pointY];
                }

                return (newA, newB);
            };

            _mutation = a =>
            {
                var mutPoint = rnd.Next(0, a.Length - 2);
                var pointX = mutPoint % a.GetLength(0);
                var pointY = mutPoint / a.GetLength(1);

                var newA = a.Clone() as bool[,];

                newA[pointX, pointY] = !newA[pointX, pointY];

                return newA;
            };

            _tournamentSelection = fitnesses => {
                var first = rnd.Next(0, fitnesses.Count - 1);
                var second = rnd.Next(0, fitnesses.Count - 1);

                return fitnesses[first] > fitnesses[second] ? first : second;
            };

            _ruletSelection = fitneses =>
            {
                var sum_fit = fitneses.Sum();
                var u = rnd.NextDouble() * (sum_fit - 0.0) + 0.0;

                for (var i = fitneses.Count - 1; i >= 0; i--)
                {
                    sum_fit -= fitneses[i];

                    if (sum_fit <= u)
                        return i;
                }

                return 0;
            };

            _rankSelection = fitneses =>
            {
                List<(double fitness, int index)> fitnesses_with_index = new List<(double fitness, int index)>();

                for (var i = 0; i < fitneses.Count; i++)
                    fitnesses_with_index.Add((fitneses[i], i));

                fitnesses_with_index = fitnesses_with_index.OrderBy(x => x.fitness).ToList();

                var f = new List<double>(fitneses);
                for (var i = 0; i < fitneses.Count; i++)
                    f[i] = (i * 2) + 1;

                return fitnesses_with_index[_ruletSelection(f)].index;
            };

            if (crossoverMethod == "OnePoint")
                _crossover = _crossoverOnePoint;
            else if (crossoverMethod == "TwoPoints")
                _crossover = _crossoverTwoPoints;

            if (selectionMethod == "Rulet")
                _selection = _ruletSelection;
            else if (selectionMethod == "Rank")
                _selection = _rankSelection;
            else if (selectionMethod == "Tournament")
                _selection = _tournamentSelection;
        }

        private List<bool[,]> InitialPopulation()
        {
            var pop = new List<bool[,]>();

            for (var i = 0; i < _populationSize; i++)
                pop.Add(Generator.GenerateRandomBoard(_boardValues));

            return pop;
        }

        public Result Run(bool isIsland = false)
        {
            bool[,] result;
            if(isIsland)
                result = IslandAlgorithm(_initialPopulation, _fitness, _selection, _crossover,
                _mutation, _crossoverPropability, _mutationPropability, _iterationCount, _islandCount, _parallel);
            else
                result = Algorithm(_initialPopulation, _fitness, _selection, _crossover,
                    _mutation, _termCondition, _crossoverPropability, _mutationPropability, _parallel);

            return new Result(result, _boardValues);
        }

        private bool[,] Algorithm(List<bool[,]> initialPopulation, Func<bool[,], double> fitness,
           Func<List<double>, int> selection, Func<bool[,], bool[,], (bool[,], bool[,])> crossover,
           Func<bool[,], bool[,]> mutation, Func<List<bool[,]>, bool> termCondition, double crossoverPropability,
           double mutationPropability, bool parallel)
        {
            var population = initialPopulation;
            var fit = new List<double>();

            if (parallel)
                Parallel.ForEach(population, specimen =>
                {
                    var tmp = fitness(specimen);
                    lock (fit)
                    {
                        fit.Add(tmp);
                    }
                });
            else
                foreach (var specimen in population)
                    fit.Add(fitness(specimen));

            while (termCondition(population))
            {
                var parents = new List<bool[,]>();
                var children = new List<bool[,]>();
               
                for (var i = 0; i < initialPopulation.Count; i++)
                    parents.Add(population[selection(fit)]);
                
                for (var i = 0; i < initialPopulation.Count - 1; i += 2)
                {
                    var u = new Random().NextDouble();

                    if (crossoverPropability < u)
                    {
                        (var a, var b) = crossover(parents[i], parents[i + 1]);
                        children.Add(a);
                        children.Add(b);
                    }
                    else
                    {
                        children.Add(parents[i]);
                        children.Add(parents[i + 1]);
                    }
                }

                for (var i = 0; i < initialPopulation.Count - 1; i += 2)
                {
                    var u = new Random().NextDouble();

                    if (mutationPropability < u)
                        children[i] = mutation(children[i]);
                }

                population = children;

                if(parallel)
                    Parallel.For(0, population.Count, i =>
                    {
                        fit[i] = fitness(population[i]);
                    });
                else
                    for(var i = 0; i < population.Count; i++)
                        fit[i] = fitness(population[i]);
            }

            return population.Find(x => fitness(x) == population.Max(y => fitness(y)));
        }

        public static bool[,] IslandAlgorithm(List<bool[,]> initialPopulation, Func<bool[,], double> fitness,
           Func<List<double>, int> selection, Func<bool[,], bool[,], (bool[,], bool[,])> crossover,
           Func<bool[,], bool[,]> mutation, double crossoverPropability, double mutationPropability, int iterations, 
           int islandsCount, bool parallel)
        {
            var populations = new List<List<bool[,]>>();
            var N = initialPopulation.Count / islandsCount;

            for (var i = 0; i < islandsCount; i++)
            {
                populations.Add(new List<bool[,]>());
                for (var j = i * N; j < (i + 1) * N; j++)
                    populations[i].Add(initialPopulation[j]);
            }

            var fitnesses = new List<List<double>>();
            for(var island = 0; island < islandsCount; island++)
            {
                fitnesses.Add(new List<double>());
                var population = populations[island];
                for (var i = 0; i < N; i++)
                    fitnesses[island].Add(fitness(population[i]));
            }

            var iteration = 0;
            while (iteration < iterations)
            {
                if(parallel)
                    Parallel.For(0, islandsCount, island =>
                    {
                        var fit = fitnesses[island];

                        var population = populations[island];

                        var parents = new List<bool[,]>();
                        var children = new List<bool[,]>();

                        for (var i = 0; i < N; i++)
                            parents.Add(population[selection(fit)]);

                        for (var i = 0; i < N - 1; i += 2)
                        {
                            var u = new Random().NextDouble();

                            if (crossoverPropability < u)
                            {
                                (var a, var b) = crossover(parents[i], parents[i + 1]);
                                children.Add(a);
                                children.Add(b);
                            }
                            else
                            {
                                children.Add(parents[i]);
                                children.Add(parents[i + 1]);
                            }
                        }

                        for (var i = 0; i < N - 1; i += 2)
                        {
                            var u = new Random().NextDouble();

                            if (mutationPropability < u)
                                children[i] = mutation(children[i]);
                        }

                        population = children;
                        population.OrderBy(x => fitness(x));

                        for (var i = 0; i < fit.Count; i++)
                            fit[i] = fitness(population[i]);

                    });
                else
                    for (var island = 0; island < islandsCount; island++)
                    {
                        var fit = fitnesses[island];

                        var population = populations[island];

                        var parents = new List<bool[,]>();
                        var children = new List<bool[,]>();

                        for (var i = 0; i < N; i++)
                            parents.Add(population[selection(fit)]);

                        for (var i = 0; i < N - 1; i += 2)
                        {
                            var u = new Random().NextDouble();

                            if (crossoverPropability < u)
                            {
                                (var a, var b) = crossover(parents[i], parents[i + 1]);
                                children.Add(a);
                                children.Add(b);
                            }
                            else
                            {
                                children.Add(parents[i]);
                                children.Add(parents[i + 1]);
                            }
                        }

                        for (var i = 0; i < N - 1; i += 2)
                        {
                            var u = new Random().NextDouble();

                            if (mutationPropability < u)
                                children[i] = mutation(children[i]);
                        }

                        population = children;
                        population.OrderBy(x => fitness(x));

                        for (var i = 0; i < fit.Count; i++)
                            fit[i] = fitness(population[i]);

                    }

                for(var island = 0; island < islandsCount; island++)
                {
                    var fit = fitnesses[island];
                    var population = populations[island];

                   for(var i = 0; i < islandsCount; i++)
                    {
                        if (i != island) {
                            populations[i].Add(population.Last());
                            fitnesses[i].Add(fit.Last());
                        }
                    }
                }

                if (parallel)
                    Parallel.For(0, islandsCount, island =>
                    {
                        var fit = fitnesses[island];
                        var population = populations[island];

                        fit.RemoveRange(0, islandsCount - 1);
                        population.RemoveRange(0, islandsCount - 1);
                    });
                else
                    for (var island = 0; island < islandsCount; island++)
                    {
                        var fit = fitnesses[island];
                        var population = populations[island];

                        fit.RemoveRange(0, islandsCount - 1);
                        population.RemoveRange(0, islandsCount - 1);
                    }

                iteration++;
            }

            var lastPopulation = new List<bool[,]>();
            foreach (var p in populations)
                foreach (var s in p)
                    lastPopulation.Add(s);

            return lastPopulation.Find(x => fitness(x) == populations.Max(y => fitness(y[0])));
        }
    }
}
