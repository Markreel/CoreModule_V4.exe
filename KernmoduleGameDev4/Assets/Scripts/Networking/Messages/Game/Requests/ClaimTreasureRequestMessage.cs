using Unity.Networking.Transport;

namespace Assets.Code
{
    public class ClaimTreasureRequestMessage : MessageHeader
    {
        public override MessageType Type => MessageType.ClaimTreasureRequest;

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