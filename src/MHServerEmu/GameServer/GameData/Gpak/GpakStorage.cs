﻿using System.Text.Json;

namespace MHServerEmu.GameServer.GameData.Gpak
{
    public class GpakStorage
    {
        protected JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        public virtual bool Verify()
        {
            throw new("Verify is not implemented for this GpakStorage.");
        }

        public virtual void Export()
        {
            throw new("Export is not implemented for this GpakStorage.");
        }

        protected void SerializeDictAsJson<T>(Dictionary<string, T> dict)
        {
            foreach (var kvp in dict)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "GPAK", "Export", $"{kvp.Key}.json");
                string dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);

                File.WriteAllText(path, JsonSerializer.Serialize((object)kvp.Value, _jsonSerializerOptions));
            }
        }
    }
}
