using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Nonogram
{
    public static class JsonHelper
    {
        public static BoardValues ReadJsonFile(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            string json;

            using (var reader = new StreamReader(path))
            {
                json = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<BoardValues>(json);
        }

        public static void WriteJsonFile(bool[,] board, int errors, string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var outputResult = new JObject(new JProperty("Errors", errors), new JProperty("Board", board));

            using (var file = File.CreateText(path))
            using (var writer = new JsonTextWriter(file))
            {
                outputResult.WriteTo(writer);
            }
        }
    }
}
