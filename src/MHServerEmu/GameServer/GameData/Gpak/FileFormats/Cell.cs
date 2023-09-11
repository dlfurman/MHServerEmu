﻿using MHServerEmu.Common;
using MHServerEmu.Common.Extensions;
using MHServerEmu.GameServer.Common;
using MHServerEmu.GameServer.GameData.Prototypes;
using MHServerEmu.GameServer.GameData.Prototypes.Markers;

namespace MHServerEmu.GameServer.GameData.Gpak.FileFormats
{
    public class CellPrototype
    {
        public uint Header { get; }
        public uint Version { get; }
        public uint ClassId { get; }
        public Aabb Boundbox { get; }
        public uint Type { get; }
        public uint Walls { get; }
        public uint FillerEdges { get; }
        public uint RoadConnections { get; }
        public string ClientMap { get; }
        public MarkerPrototype[] InitializeSet { get; }
        public MarkerPrototype[] MarkerSet { get; }
        public NaviPatchSourcePrototype NaviPatchSource { get; }
        public byte IsOffsetInMapFile { get; }
        public CellHeightMap HeightMap { get; }
        public ulong[] HotspotPrototypes { get; }

        public CellPrototype(byte[] data)
        {
            using (MemoryStream stream = new(data))
            using (BinaryReader reader = new(stream))
            {
                Header = reader.ReadUInt32();
                Version = reader.ReadUInt32();
                ClassId = reader.ReadUInt32();
                Vector3 max = reader.ReadVector3();
                Vector3 min = reader.ReadVector3();
                Boundbox = new(max, min);
                Type = reader.ReadUInt32();
                Walls = reader.ReadUInt32();
                FillerEdges = reader.ReadUInt32();
                RoadConnections = reader.ReadUInt32();
                ClientMap = reader.ReadFixedString32();

                InitializeSet = new MarkerPrototype[reader.ReadInt32()];
                for (int i = 0; i < InitializeSet.Length; i++)
                    InitializeSet[i] = ReadMarkerPrototype(reader);

                MarkerSet = new MarkerPrototype[reader.ReadInt32()];
                for (int i = 0; i < MarkerSet.Length; i++)
                    MarkerSet[i] = ReadMarkerPrototype(reader);

                NaviPatchSource = new(reader);
                IsOffsetInMapFile = reader.ReadByte();
                HeightMap = new(reader);

                HotspotPrototypes = new ulong[reader.ReadUInt32()];
                for (int i = 0; i < HotspotPrototypes.Length; i++)
                    HotspotPrototypes[i] = reader.ReadUInt64();
            }
        }

        private MarkerPrototype ReadMarkerPrototype(BinaryReader reader)
        {
            MarkerPrototype markerPrototype;
            ResourcePrototypeHash hash = (ResourcePrototypeHash)reader.ReadUInt32();

            switch (hash)
            {
                case ResourcePrototypeHash.CellConnectorMarkerPrototype:
                    markerPrototype = new CellConnectorMarkerPrototype(reader);
                    break;
                case ResourcePrototypeHash.DotCornerMarkerPrototype:
                    markerPrototype = new DotCornerMarkerPrototype(reader);
                    break;
                case ResourcePrototypeHash.EntityMarkerPrototype:
                    markerPrototype = new EntityMarkerPrototype(reader);
                    break;
                case ResourcePrototypeHash.RoadConnectionMarkerPrototype:
                    markerPrototype = new RoadConnectionMarkerPrototype(reader);
                    break;
                default:
                    throw new($"Unknown ResourcePrototypeHash {(uint)hash}");   // Throw an exception if there's a hash for a type we didn't expect
            }

            return markerPrototype;
        }
    }

    public class CellHeightMap
    {
        public Vector2 HeightMapSize { get; }
        public ushort[] HeightMapData { get; }
        public byte[] HotspotData { get; }

        public CellHeightMap(BinaryReader reader)
        {
            HeightMapSize = new(reader.ReadUInt32(), reader.ReadUInt32());

            HeightMapData = new ushort[reader.ReadUInt32()];
            for (int i = 0; i < HeightMapData.Length; i++)
                HeightMapData[i] = reader.ReadUInt16();

            HotspotData = new byte[reader.ReadUInt32()];
            for (int i = 0; i < HotspotData.Length; i++)
                HotspotData[i] = reader.ReadByte();
        }
    }
}
