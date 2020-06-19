using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class NewPlayerMessage : MessageHeader
    {
        public override MessageType Type => MessageType.NewPlayer;

        public int PlayerID { get; set; }
        public uint PlayerColour { get; set; }
        public string PlayerName { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteInt(PlayerID);
            _writer.WriteUInt(PlayerColour);
            _writer.WriteString(PlayerName);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            PlayerID = _reader.ReadInt();
            PlayerColour = _reader.ReadUInt();
            PlayerName = _reader.ReadString().ToString();
        }
    }
}