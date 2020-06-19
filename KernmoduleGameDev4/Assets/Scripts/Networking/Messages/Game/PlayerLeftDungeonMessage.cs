using Unity.Networking.Transport;

namespace Assets.Code
{
    public class PlayerLeftDungeonMessage : MessageHeader
    {
        public override MessageType Type => MessageType.PlayerLeftDungeon;

        public int PlayerID { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteInt(PlayerID);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            PlayerID = _reader.ReadInt();
        }
    }
}
