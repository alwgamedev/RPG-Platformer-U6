using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Threading.Tasks;

namespace RPGPlatformer.Saving
{
    public static class SavingUtilities
    {
        public static JsonSerializerOptions DefaultOptions()
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            return options;
        }

        public static async Task SaveJObjectToFile(JsonObject jObject, string fileName)
        {
            await using var streamWriter = File.Create(fileName);
            await JsonSerializer.SerializeAsync(streamWriter, jObject, DefaultOptions());
        }

        public static async Task<JsonObject> LoadJObjectFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new JsonObject();
            }

            using var streamReader = File.OpenText(fileName);
            return await JsonSerializer.DeserializeAsync<JsonObject>(streamReader.BaseStream);
        }
    }
}