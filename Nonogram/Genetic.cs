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
        private Func<bool[,], bool[,], (bool[,], bool[,])> _crossover;
        private Func<bool[,], bool[,]> _mutation;
        private Func<List<bool[,]>, bool> _termCondition;
        private double _crossoverPropability;
        private double _mutationPropability;


        public Genetic(BoardValues boardValues, int populationSize = 10, int iterationCount = 10, double crossoverPropability = 0.9, double mutationPropability = 0.1)
        {
            var rnd = new Random();

            _boardValues = boardValues;
            _populationSize = populationSize;
            _crossoverPropability = crossoverPropability;
            _mutationPropability = mutationPropability;
            _iterationCount = iterationCount;

            _initialPopulation = InitialPopulation();

            _fitness = specimen => 1000.0 / (1.0 + BoardHelper.CheckForErrors(_boardValues, specimen));

            _selection = fitnesses => {
                var first = rnd.Next(0, fitnesses.Count - 1);
                var second = rnd.Next(0, fitnesses.Count - 1);

                return fitnesses[first] > fitnesses[second] ? first : second;
            };

            _crossover = (a, b) =>
            {
                var crossPoint = rnd.Next(0, a.Length - 1);

                var newA = a.Clone() as bool[,];
                var newB = b.Clone() as bool[,];

                for (var i = crossPoint; i < a.Length; i++)
                {
                    var pointX = i % a.GetLength(1);
                    var pointY = i / a.GetLength(0);

                    newA[pointX, pointY] = b[pointX, pointY];
                    newB[pointX, pointY] = a[pointX, pointY];
                }

                return (newA, newB);
            }; // krzyzowo

            _mutation = a =>
            {
                var mutPoint = rnd.Next(0, a.Length - 2);
                var pointX = mutPoint % a.GetLength(1);
                var pointY = mutPoint / a.GetLength(0);

                var newA = a.Clone() as bool[,];

                newA[pointX, pointY] = !newA[pointX, pointY];

                Console.WriteLine($"Before: {BoardHelper.CheckForErrors(_boardValues, a)} After: {BoardHelper.CheckForErrors(_boardValues, newA)}");

                return newA;
            };

            _termCondition = pop =>
            {
                foreach(var s in  pop)
                {
                    Console.Write(BoardHelper.CheckForErrors(_boardValues, s) + " ");
                }
                Console.WriteLine();

                _iterationCount--;

                return (_iterationCount > 0);
            };
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
