using Unity.Networking.Transport;

namespace Assets.Code
{
    public class PlayerDefendsMessage : MessageHeader
    {
        public override MessageType Type => MessageType.PlayerDefends;

        public int PlayerID { get; set; }
        public ushort newHP { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteInt(PlayerID);
            _writer.WriteUShort(newHP);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            PlayerID = _reader.ReadInt();
            newHP = _reader.ReadUShort();
        }
    }
}