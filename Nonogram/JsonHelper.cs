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

        public static void WriteJsonFile(Result result, float time, string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            using (var file = File.CreateText(path))
            using (var writer = new JsonTextWriter(file))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("Errors");
                writer.WriteValue(result.Error);
                writer.WritePropertyName("Time");
                writer.WriteValue(time);
                writer.WritePropertyName("Board");
                writer.WriteStartArray();

                for (var i = 0; i < result.Board.GetLength(0); i++)
                {
                    writer.WriteStartArray();
                    for (var j = 0; j < result.Board.GetLength(0); j++)
                        writer.WriteValue(result.Board[i, j]);
                    writer.WriteEnd();
                }

                writer.WriteEnd();
                writer.WriteEndObject();
            }
        }
    }
}
