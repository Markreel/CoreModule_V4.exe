using Unity.Networking.Transport;

namespace Assets.Code
{
    public class HitMonsterMessage : MessageHeader
    {
        public override MessageType Type => MessageType.HitMonster;

        public int PlayerID { get; set; }
        public ushort DamageDealt { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteInt(PlayerID);
            _writer.WriteUShort(DamageDealt);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            PlayerID = _reader.ReadInt();
            DamageDealt = _reader.ReadUShort();
        }
    }
}