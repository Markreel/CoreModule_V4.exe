using Unity.Networking.Transport;

namespace Assets.Code
{
    public class DefendRequestMessage : MessageHeader
    {
        public override MessageType Type => MessageType.DefendRequest;

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);
        }
    }
}
