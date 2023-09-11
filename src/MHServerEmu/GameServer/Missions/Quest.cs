﻿using System.Text;
using Google.ProtocolBuffers;

namespace MHServerEmu.GameServer.Missions
{
    public class Quest
    {
        public ulong PrototypeId { get; set; }
        public ulong[] Fields { get; set; }

        public Quest(CodedInputStream stream)
        {
            PrototypeId = stream.ReadRawVarint64();

            Fields = new ulong[stream.ReadRawVarint64()];
            for (int i = 0; i < Fields.Length; i++)
            {
                Fields[i] = stream.ReadRawVarint64();
            }
        }

        public Quest(ulong prototypeId, ulong[] fields)
        {
            PrototypeId = prototypeId;
            Fields = fields;
        }

        public byte[] Encode()
        {
            using (MemoryStream ms = new())
            {
                CodedOutputStream cos = CodedOutputStream.CreateInstance(ms);

                cos.WriteRawVarint64(PrototypeId);
                cos.WriteRawVarint64((ulong)Fields.Length);
                foreach (ulong field in Fields) cos.WriteRawVarint64(field);

                cos.Flush();
                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"PrototypeId: 0x{PrototypeId:X}");
            for (int i = 0; i < Fields.Length; i++) sb.AppendLine($"Field{i}: 0x{Fields[i]:X}");
            return sb.ToString();
        }
    }
}
