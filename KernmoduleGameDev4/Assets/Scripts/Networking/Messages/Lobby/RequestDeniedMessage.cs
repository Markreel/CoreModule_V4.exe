using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class RequestDeniedMessage : MessageHeader
    {
        public override MessageType Type => MessageType.RequestDenied;

        public uint DeniedMessageID { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteUInt(DeniedMessageID);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            DeniedMessageID = _reader.ReadUInt();
        }
    }
}