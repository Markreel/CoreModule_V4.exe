using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class StayAliveMessage : MessageHeader
    {
        public override MessageType Type => MessageType.None;

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);
        }
    }
}
