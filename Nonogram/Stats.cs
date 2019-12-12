using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nonogram
{
    public static class Stats
    {
        private static readonly int _repeatNumber = 25;

        public static void GenerateStats(string inputsFilename, string outputFilename, bool withoutBruteForce = true)
        {
            var methods = Type.GetType("Nonogram.Method").GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();
            var selectedMethods = methods.GetRange(Convert.ToInt32(withoutBruteForce), methods.Count - 1);
            var examples = JsonHelper.ReadInputListFile(inputsFilename);

            var csv = new StringBuilder();
            csv.AppendLine("METODA,ROZMIAR,CZAS,ERROR");

            for (var i = 0; i < _repeatNumber; i++)
            {
                foreach (var method in selectedMethods)
                {
                    var parameters = method.GetParameters();
                    var parametersLength = parameters.Length;

                    var obj = new object[parametersLength];

                    foreach (var example in examples)
                    {
                        obj[0] = example;
                        for (var j = 1; j < parametersLength; j++)
                            obj[j] = parameters[j].DefaultValue;
                        Result result;

                        var watch = Stopwatch.StartNew();
                        if (method.Name == "Genetic")
                        {
                            var gen = new GeneticAlgorithm();
                            result = gen.Run();
                        }else
                            result = (Result)method.Invoke(Type.GetType("Nonogram.Method"), obj);
                        watch.Stop();

                        result.Time = watch.ElapsedMilliseconds / 1000f;

                        csv.AppendLine($"{method.Name},{example.ColumnCount * example.RowCount},{result.Time},{result.Error}");
                    }
                }
            }

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), outputFilename), csv.ToString());

            var filenames = new List<string>();
            foreach (var method in selectedMethods)
                filenames.Add($"{method.Name}_mean.csv");

            Mean(outputFilename, filenames.ToArray());
        }

        public static void Mean(string inputFilename, string[] outputsFilename)
        {
            var dic = new Dictionary<string, (double meanTime, double meanError)>();

            using (var reader = new StreamReader(inputFilename))
            {
                reader.ReadLine(); // Skip first line

                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine();
                    var cells = row.Split(',');

                    var key = $"{cells[0]} {cells[1]}";
                    var value = (time: Convert.ToDouble(cells[2]), error: Convert.ToDouble(cells[3]));

                    if (dic.ContainsKey(key))
                    {
                        var v = dic[key];
                        v.meanTime += value.time;
                        v.meanError += value.error;
                        dic[key] = v;
                    }
                    else
                        dic.Add(key, value);
                }
            }

            var examplesCount = dic.Count / outputsFilename.Length;

            for (var i = 0; i < outputsFilename.Length; i++)
            {
                var outputFilename = outputsFilename[i];

                var csv = new StringBuilder();
                csv.AppendLine("METODA,ROZMIAR,CZAS_SR,ERROR_SR");

                foreach (var row in dic.ToList().GetRange(i * examplesCount, examplesCount))
                {
                    var keys = row.Key.Split(' ');
                    csv.AppendLine($"{keys[0]},{keys[1]},{row.Value.meanTime / _repeatNumber},{row.Value.meanError / _repeatNumber}");
                }

                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), outputFilename), csv.ToString());

                Plot(outputFilename);
            }
        }

        public static void Plot(string inputFilename)
        {
            var sizeData = new List<double>();
            var timeData = new List<double>();
            var errorData = new List<double>();

            using (var reader = new StreamReader(inputFilename))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine();
                    var cells = row.Split(',');
                    sizeData.Add(Convert.ToDouble(cells[1]));
                    timeData.Add(Convert.ToDouble(cells[2]));
                    errorData.Add(Convert.ToDouble(cells[3]));
                }
            }

            for (var i = 0; i < 2; i++)
            {
                var title = i == 0 ? "Time" : "Error";
                var data = i == 0 ? timeData : errorData;

                var model = new PlotModel { Title = $"{inputFilename.Replace('_', ' ').Replace(".csv", "")} {title.ToLower()} plot"};
                model.Axes.Add(new LinearAxis
                {
                    Title = "Size",
                    Position = AxisPosition.Bottom,
                    Minimum = 0,
                    Maximum = sizeData.Last() + 1
                });

                model.Axes.Add(new LinearAxis
                {
                    Title = title,
                    Position = AxisPosition.Left,
                    Minimum = 0,
                    Maximum = data.Max() + data.Max() / 10
                });

                var lineSeries = new LineSeries();

                for (var j = 0; j < sizeData.Count; j++)
                    lineSeries.Points.Add(new DataPoint(sizeData[j], data[j]));

                model.Series.Add(lineSeries);

                using (var stream = File.Create($"{inputFilename}_{title}Plot.pdf"))
                {
                    var pdfExporter = new PdfExporter { Width = 600, Height = 400 };
                    pdfExporter.Export(model, stream);
                }            }        }
    }
}
