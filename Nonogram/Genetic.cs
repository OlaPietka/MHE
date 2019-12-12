using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonogram
{
    public class Genetic
    {
        private BoardValues _boardValues;
        private int _populationSize;
        private int _iterationCount;
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


        public Genetic(BoardValues boardValues, int populationSize = 10, int iterationCount = 10, double crossoverPropability = 0.9, 
            double mutationPropability = 0.1, string crossoverMethod = "OnePoint", string selectionMethod = "Tournament",
            string termConditionMethod = "Iteration")
        {
            var rnd = new Random();

            _boardValues = boardValues;
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

                Console.WriteLine($"Before: {BoardHelper.CheckForErrors(_boardValues, a)} After: {BoardHelper.CheckForErrors(_boardValues, newA)}");

                return newA;
            };

            _iterationTermCondition = pop =>
            {
                foreach(var s in  pop)
                    Console.Write(BoardHelper.CheckForErrors(_boardValues, s) + " ");
                Console.WriteLine();

                _iterationCount--;

                return (_iterationCount > 0);
            };

            _meanTermCondition = pop =>
            {
                var mean = pop.Sum(x => _fitness(x)) / pop.Count;

                return mean < 999 ? true : false;
            };

            _deviationTermCondition = pop =>
            {
                var mean = pop.Sum(x => _fitness(x)) / pop.Count;

                var w = pop.Sum(x => Math.Pow(_fitness(x) - mean, 2)) / pop.Count;

                return Math.Sqrt(w) > 0 ? true : false;
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

                for (var i = fitneses.Count - 1; i >= 0 ; i--)
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

                for(var i = 0; i < fitneses.Count; i++)
                    fitnesses_with_index.Add((fitneses[i], i));

                fitnesses_with_index = fitnesses_with_index.OrderBy(x => x.fitness).ToList();

                var f = new List<double>(fitneses);
                for (var i = 0; i < fitneses.Count; i++)
                    f[i] = (i*2) + 1;

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

        private List<bool[,]> InitialPopulation()
        {
            var pop = new List<bool[,]>();

            for (var i = 0; i < _populationSize; i++)
                pop.Add(Generator.GenerateRandomBoard(_boardValues));

            return pop;
        }

        public Result Run()
        {
            var result = Methods.Genetic(_initialPopulation, _fitness, _selection, _crossover,
                _mutation, _termCondition, _crossoverPropability, _mutationPropability);

            return new Result(result, _boardValues);
        }
    }
}
