using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Nonogram
{
    public static class JsonHelper
    {
        private static string ReadJson(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            string json;

            using (var reader = new StreamReader(path))
            {
                json = reader.ReadToEnd();
            }

            return json;
        }

        public static BoardValues ReadInputFile(string fileName) 
            => JsonConvert.DeserializeObject<BoardValues>(ReadJson(fileName));

        public static List<BoardValues> ReadInputListFile(string fileName)
            => JsonConvert.DeserializeObject<List<BoardValues>>(ReadJson(fileName));

        public static void WriteOutputFile(Result result, string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented; 
                serializer.Serialize(file, result);
            }
        }
    }
}
