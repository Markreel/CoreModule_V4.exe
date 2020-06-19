using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public abstract class MessageHeader
    {
        private static uint nextID = 0;
        public static uint NextID => ++nextID;

        public enum MessageType
        {
            None = 0,
            NewPlayer,
            Welcome,
            SetName,
            RequestDenied,
            PlayerLeft,
            StartGame,
            PlayerTurn,
            RoomInfo,
            PlayerEnterRoom,
            PlayerLeaveRoom,
            ObtainTreassure,
            HitMonster,
            HitByMonster,
            PlayerDefends,
            PlayerLeftDungeon,
            PlayerDies,
            EndGame,
            MoveRequest,
            AttackRequest,
            DefendRequest,
            ClaimTreasureRequest,
            LeaveDungeonRequest,
            Count
        }

        public abstract MessageType Type { get; }
        public uint ID { get; private set; } = NextID;

        public virtual void SerializeObject(ref DataStreamWriter _writer)
        {
            _writer.WriteUShort((ushort)Type);
            _writer.WriteUInt(ID);
        }

        public virtual void DeserializeObject(ref DataStreamReader _reader)
        {
            ID = _reader.ReadUInt();
        }
    }
}
