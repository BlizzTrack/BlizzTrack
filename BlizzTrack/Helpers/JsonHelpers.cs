using System.Text.Json;

namespace BlizzTrack.Helpers
{
    public static class JsonHelpers
    {
        public static string PrettyJson(string unPrettyJson)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

            return JsonSerializer.Serialize(jsonElement, options);
        }
    }
}
