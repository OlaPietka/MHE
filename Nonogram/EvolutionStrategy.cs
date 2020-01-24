using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonogram
{
    public class Chromosome
    {
        public List<double> Y;
        public List<double> S;

        public Chromosome(List<double> y, List<double> s)
        {
            Y = y;
            S = s;
        }

        public double F()
        {
            var x0 = Y[0];
            var y0 = Y[1];

            var ret = 100 / (Math.Pow((x0 * x0 + y0 - 11), 2) + Math.Pow((x0 + y0 * y0 - 7), 2));

            if (ret < 0.0)
                return 0.0;

            return ret;
        }

        public static Chromosome operator +(Chromosome a, double b)
        {
            var ret = a.Copy();
            for(var i = 0; i < a.Y.Count(); i++)
                ret.Y[i] += b;

            return ret;
        }

        public Chromosome Copy()
        {
            return new Chromosome(new List<double>(Y), new List<double>(S));
        }

        public override string ToString()
        {
            var s = "Y: | ";
            for (var i = 0; i < Y.Count(); i++)
                s += Y[i].ToString() + " | ";
            s += "F: " + F().ToString();
            return s;
        }

    }


    public class EvolutionStrategy
    {
        private Random _rnd = new Random();

        private List<double> InitList(int n, double v)
        {
            var list = new List<double>();
            for (var i = 0; i < n; i++)
                list.Add(Math.Abs(v * _rnd.NextDouble()));
            return list;
        }

        public Chromosome Run()
        {
            return Strategy(new Chromosome(InitList(2, 3.0), InitList(2, 1.0)));
        }

        public Chromosome Strategy(Chromosome initPop, int iterations = 100)
        {
            var t = 1;
            var sigma = _rnd.NextDouble();
            var x = initPop;
            var cd = 0.82;
            var cj = 1 / 0.82;
            var count = new List<int>();

            while (t <= iterations)
            {
                var y = (x + (sigma * NormalDistribution())).Copy();

                if (y.F() > x.F())
                {
                    x = y.Copy();
                    count.Add(1);
                }
                else
                    count.Add(0);

                Console.WriteLine("x: "+x.ToString());
                Console.WriteLine("y: "+y.ToString());
                Console.WriteLine(sigma);

                if (count.Count > 10)
                {
                    var sum = count.GetRange(count.Count - 10, 10).Sum();
                    if (sum > 10 / 5)
                        sigma *= cj;
                    else if (sum < 10 / 5)
                        sigma *= cd;
                }

                t++;
            }

            return x;
        }

        private double NormalDistribution()
        {
        random:
            var u1 = _rnd.NextDouble();
            var u2 = _rnd.NextDouble();
            if (u1 == 0 || u2 == 0)
                goto random;

            return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }
    }
}
