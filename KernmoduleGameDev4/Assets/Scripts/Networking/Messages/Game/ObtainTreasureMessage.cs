using Unity.Networking.Transport;

namespace Assets.Code
{
    public class ObtainTreasureMessage : MessageHeader
    {
        public override MessageType Type => MessageType.ObtainTreassure;

        public ushort Amount { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteUShort(Amount);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            Amount = _reader.ReadUShort();
        }
    }
}