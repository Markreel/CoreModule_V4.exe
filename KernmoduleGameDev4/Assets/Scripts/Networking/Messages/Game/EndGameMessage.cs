using Unity.Networking.Transport;

namespace Assets.Code
{
    public class EndGameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.EndGame;

        public byte NumberOfScores { get; set; }
        public HighScorePair[] PlayerIDHighscorePairs { get; set; }

        public override void SerializeObject(ref DataStreamWriter _writer)
        {
            base.SerializeObject(ref _writer);

            _writer.WriteByte(NumberOfScores);
            for (int i = 0; i < NumberOfScores; i++)
            {
                _writer.WriteInt(PlayerIDHighscorePairs[i].playerID);
                _writer.WriteUShort(PlayerIDHighscorePairs[i].score);
            }
        }

        public override void DeserializeObject(ref DataStreamReader _reader)
        {
            base.DeserializeObject(ref _reader);

            NumberOfScores = _reader.ReadByte();
            PlayerIDHighscorePairs = new HighScorePair[NumberOfScores];
            for (int i = 0; i < NumberOfScores; i++)
            {
                PlayerIDHighscorePairs[i].playerID = _reader.ReadInt();
                PlayerIDHighscorePairs[i].score = _reader.ReadUShort();
            }
        }
    }
}

public struct HighScorePair
{
    public int playerID;
    public ushort score;

    public HighScorePair(int _playerID, ushort _score)
    {
        playerID = _playerID;
        score = _score;
    }
}
