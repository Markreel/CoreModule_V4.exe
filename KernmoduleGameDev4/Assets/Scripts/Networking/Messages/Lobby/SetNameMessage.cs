using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class SetNameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.SetName;

        public string Name { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteString(Name);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            Name = _reader.ReadString().ToString();
        }
    }
}
