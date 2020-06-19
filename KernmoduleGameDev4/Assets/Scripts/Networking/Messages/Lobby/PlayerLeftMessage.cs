using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class PlayerLeftMessage : MessageHeader
    {
        public override MessageType Type => MessageType.PlayerLeft;

        public int PlayerLeftID { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteInt(PlayerLeftID);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            PlayerLeftID = _reader.ReadInt();
        }
    }
}