using Newtonsoft.Json;
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

        public static void WriteJsonFile(Result result, string fileName)
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
