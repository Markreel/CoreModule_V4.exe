using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class StartGameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.StartGame;

        public ushort StartHP { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteUShort(StartHP);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            StartHP = _reader.ReadUShort();
        }
    }
}