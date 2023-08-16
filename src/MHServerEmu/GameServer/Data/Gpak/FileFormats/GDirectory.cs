﻿using System.Text;
using MHServerEmu.Common;

namespace MHServerEmu.GameServer.Data.Gpak.FileFormats
{
    // It's actually called Directory, but we're calling it GDirectory to avoid confusion with C# stuff
    public enum GDirectoryHeader
    {
        Blueprint = 0xB524442,      // BDR
        Curve = 0xB524443,          // CDR
        Type = 0xB524454,           // TDR
        Replacement = 0xB524452,    // RDR
        Prototype = 0xB524450       // PDR
    }

    public class GDirectory
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public GDirectoryHeader Header { get; }
        public IGDirectoryEntry[] Entries { get; }

        public GDirectory(byte[] data)
        {
            using (MemoryStream stream = new(data))
            using (BinaryReader reader = new(stream))
            {
                Header = (GDirectoryHeader)reader.ReadUInt32();
                Entries = new IGDirectoryEntry[reader.ReadUInt32()];

                switch (Header)
                {
                    case GDirectoryHeader.Blueprint:
                    case GDirectoryHeader.Curve:
                    case GDirectoryHeader.Type:
                        for (int i = 0; i < Entries.Length; i++)
                            Entries[i] = new GDirectoryGenericEntry(reader);
                        break;
                    case GDirectoryHeader.Replacement:
                        for (int i = 0; i < Entries.Length; i++)
                            Entries[i] = new GDirectoryReplacementEntry(reader);
                        break;
                    case GDirectoryHeader.Prototype:
                        for (int i = 0; i < Entries.Length; i++)
                            Entries[i] = new GDirectoryPrototypeEntry(reader);
                        break;
                }

                //Logger.Trace($"Parsed {Entries.Length} entries from {Header}.directory");
            }
        }
    }

    public interface IGDirectoryEntry
    {
        public ulong Id1 { get; }
        public ulong Id2 { get; }
        public string Name { get; }
    }

    public class GDirectoryGenericEntry : IGDirectoryEntry      // BDR, CDR, and TDR share the same structure
    {
        public ulong Id1 { get; }
        public ulong Id2 { get; }
        public byte Field2 { get; }
        public string Name { get; }

        public GDirectoryGenericEntry(BinaryReader reader)
        {
            Id1 = reader.ReadUInt64();
            Id2 = reader.ReadUInt64();
            Field2 = reader.ReadByte();
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadUInt16()));
        }
    }

    public class GDirectoryReplacementEntry : IGDirectoryEntry  // RDR
    {
        public ulong Id1 { get; }
        public ulong Id2 { get; }
        public string Name { get; }

        public GDirectoryReplacementEntry(BinaryReader reader)
        {
            Id1 = reader.ReadUInt64();
            Id2 = reader.ReadUInt64();
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadUInt16()));
        }
    }

    public class GDirectoryPrototypeEntry : IGDirectoryEntry    // PDR
    {
        public ulong Id1 { get; }
        public ulong Id2 { get; }
        public ulong ParentId { get; }
        public byte Field3 { get; }
        public string Name { get; }

        public GDirectoryPrototypeEntry(BinaryReader reader)
        {
            Id1 = reader.ReadUInt64();
            Id2 = reader.ReadUInt64();
            ParentId = reader.ReadUInt64();
            Field3 = reader.ReadByte();
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadUInt16()));
        }
    }
}