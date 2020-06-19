using Unity.Networking.Transport;

namespace Assets.Code
{
    public class MoveRequestMessage : MessageHeader
    {
        public override MessageType Type => MessageType.MoveRequest;

        public byte Direction { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteByte(Direction);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            Direction = _reader.ReadByte();
        }
    }
}
