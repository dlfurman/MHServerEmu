﻿using System.Text.Json;
using System.Text.Json.Serialization;
using MHServerEmu.GameServer.GameData.Gpak.FileFormats;
using MHServerEmu.GameServer.GameData.Gpak.JsonOutput;
using MHServerEmu.GameServer.GameData.Prototypes.Markers;

namespace MHServerEmu.GameServer.GameData.Gpak
{
    // Contains converters needed to correctly serialize all fields to JSON in interface dictionaries and add string representations where appropriate

    public class BlueprintConverter : JsonConverter<Blueprint>
    {
        private DataDirectory _prototypeDir;
        private DataDirectory _curveDir;
        private DataDirectory _typeDir;

        public BlueprintConverter(DataDirectory prototypeDir, DataDirectory curveDir, DataDirectory typeDir)
        {
            _prototypeDir = prototypeDir;
            _curveDir = curveDir;
            _typeDir = typeDir;
        }

        public override Blueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Blueprint value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (Blueprint)null, options);
                    break;

                default:
                    JsonSerializer.Serialize(writer, new BlueprintJson(value, _prototypeDir, _curveDir, _typeDir), options);
                    break;
            }
        }
    }

    public class PrototypeConverter : JsonConverter<Prototype>
    {
        private DataDirectory _prototypeDir;
        private DataDirectory _curveDir;
        private DataDirectory _typeDir;
        private Dictionary<ulong, string> _prototypeFieldDict;
        private Dictionary<ulong, string> _assetDict;
        private Dictionary<ulong, string> _assetTypeDict;


        public PrototypeConverter(DataDirectory prototypeDir, DataDirectory curveDir, DataDirectory typeDir,
            Dictionary<ulong, string> prototypeFieldDict, Dictionary<ulong, string> assetDict, Dictionary<ulong, string> assetTypeDict)
        {
            _prototypeDir = prototypeDir;
            _curveDir = curveDir;
            _typeDir = typeDir;
            _prototypeFieldDict = prototypeFieldDict;
            _assetDict = assetDict;
            _assetTypeDict = assetTypeDict;
        }

        public override Prototype Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Prototype value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (Prototype)null, options);
                    break;

                default:
                    JsonSerializer.Serialize(writer, new PrototypeJson(value, _prototypeDir, _curveDir, _typeDir, _prototypeFieldDict, _assetDict, _assetTypeDict), options);
                    break;
            }
        }
    }

    public class MarkerPrototypeConverter : JsonConverter<MarkerPrototype>
    {
        public override MarkerPrototype Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, MarkerPrototype value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (MarkerPrototype)null, options);
                    break;

                default:
                    var type = value.GetType();
                    JsonSerializer.Serialize(writer, value, type, options);
                    break;
            }
        }
    }

    public class UIPanelPrototypeConverter : JsonConverter<UIPanelPrototype>
    {
        public override UIPanelPrototype Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, UIPanelPrototype value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (UIPanelPrototype)null, options);
                    break;

                default:
                    var type = value.GetType();
                    JsonSerializer.Serialize(writer, value, type, options);
                    break;
            }
        }
    }
}
