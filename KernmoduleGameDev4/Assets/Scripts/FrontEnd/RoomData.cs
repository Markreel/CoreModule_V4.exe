using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Code;

[Flags]
public enum DirectionEnum
{
    North = 1,
    East = 2,
    South = 4,
    West = 8
}

public class Directions
{
    public bool North;
    public bool East;
    public bool South;
    public bool West;
}

public class RoomData
{
    public Directions Directions = new Directions();
    public int TreasureAmount = 0;
    public bool ContainsMonster = false;
    public int MonsterHP = 10;
    public bool ContainsExit = false;
    public int NumberOfOtherPlayers = 0;
    public List<int> OtherPlayersIDs = new List<int>();

    public static Directions GetDirectionsFromByte(byte _dirs)
    {
        return new Directions
        {
            North = (_dirs & (byte)DirectionEnum.North) == (byte)DirectionEnum.North,
            East = (_dirs & (byte)DirectionEnum.East) == (byte)DirectionEnum.East,
            South = (_dirs & (byte)DirectionEnum.South) == (byte)DirectionEnum.South,
            West = (_dirs & (byte)DirectionEnum.West) == (byte)DirectionEnum.West
        };
    }

    public static RoomData GetRoomDataFromRoomInfoMessage(RoomInfoMessage _message)
    {
        RoomData _roomData = new RoomData
        {
            Directions = GetDirectionsFromByte(_message.MoveDirections),
            TreasureAmount = _message.TreasureInRoom,
            ContainsMonster = _message.ContainsMonster == 0 ? false : true,
            ContainsExit = _message.ContainsExit == 0 ? false : true,
            MonsterHP = 10, //heel lelijk om het zo te moeten doen maar we sturen geen monsterHP mee dus het kan niet anders
            NumberOfOtherPlayers = _message.NumberOfOtherPlayers,
            OtherPlayersIDs = _message.OtherPlayerIDs
        };

        return _roomData;
    }

    public byte GetDirsByte()
    {
        byte _byte = 0;
        if (Directions.North) _byte |= (byte)DirectionEnum.North;
        if (Directions.East) _byte |= (byte)DirectionEnum.East;
        if (Directions.South) _byte |= (byte)DirectionEnum.South;
        if (Directions.West) _byte |= (byte)DirectionEnum.West;

        return _byte;
    }

    public byte GetByteFromDirection(DirectionEnum _enum)
    {
        byte _byte = 0;
        _byte |= (byte)_enum;
        return _byte;
    }

    public void AttackMonster(int _damage)
    {
        MonsterHP = MonsterHP - _damage > 0 ? MonsterHP - _damage : 0;
        if(MonsterHP <= 0) { ContainsMonster = false; }
    }

    public void AddOtherPlayer(int _connectionID)
    {
        if (!OtherPlayersIDs.Contains(_connectionID)) { OtherPlayersIDs.Add(_connectionID); NumberOfOtherPlayers++; }
    }

    public void RemoveOtherPlayer(int _connectionID)
    {
        if (OtherPlayersIDs.Contains(_connectionID)) { OtherPlayersIDs.Remove(_connectionID); NumberOfOtherPlayers--; }
    }
}
