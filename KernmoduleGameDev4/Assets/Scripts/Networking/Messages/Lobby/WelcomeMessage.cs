using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class WelcomeMessage : MessageHeader
    {
        public override MessageType Type => MessageType.Welcome;

        public int PlayerID { get; set; }
        public uint Colour { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteInt(PlayerID);
            _writer.WriteUInt(Colour);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            PlayerID = _reader.ReadInt();
            Colour = _reader.ReadUInt();
        }
    }
}
