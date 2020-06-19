using System.Collections.Generic;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class RoomInfoMessage : MessageHeader
    {
        public override MessageType Type => MessageType.RoomInfo;

        public byte MoveDirections { get; set; }
        public ushort TreasureInRoom { get; set; }
        public byte ContainsMonster { get; set; }
        public byte ContainsExit { get; set; }
        public byte NumberOfOtherPlayers { get; set; }
        public List<int> OtherPlayerIDs { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);
            _writer.WriteByte(MoveDirections);
            _writer.WriteUShort(TreasureInRoom);
            _writer.WriteByte(ContainsMonster);
            _writer.WriteByte(ContainsExit);
            _writer.WriteByte(NumberOfOtherPlayers);
            for (int i = 0; i < NumberOfOtherPlayers; i++)
            {
                _writer.WriteInt(OtherPlayerIDs[i]);
            }
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            MoveDirections = _reader.ReadByte();
            TreasureInRoom = _reader.ReadUShort();
            ContainsMonster = _reader.ReadByte();
            ContainsExit = _reader.ReadByte();
            NumberOfOtherPlayers = _reader.ReadByte();

            OtherPlayerIDs = new List<int>();
            for (int i = 0; i < NumberOfOtherPlayers; i++)
            {
                OtherPlayerIDs.Add(_reader.ReadInt());
            }
        }
    }
}