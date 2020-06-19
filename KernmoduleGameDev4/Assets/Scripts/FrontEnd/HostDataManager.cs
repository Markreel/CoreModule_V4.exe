using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostDataManager : Singleton<HostDataManager>
{
    public Room Room;

    public RoomData[,] Grid;
    public Vector2Int GridSize;

    private int playerTurnIndex = 0;
    public int PlayerTurnIndex
    {
        get
        {
            if(PlayerManager.Instance.Players.Count == 0) { return -1; }
            return PlayerManager.Instance.Players[playerTurnIndex].ConnectionID;
        }
    }

    public void IncrementPlayerTurnIndex()
    {
        playerTurnIndex = playerTurnIndex < PlayerManager.Instance.Players.Count-1 ? playerTurnIndex + 1 : 0;
    }

    private void Start()
    {
        PopulateGrid();
    }

    private void PopulateGrid()
    {
        Grid = new RoomData[GridSize.x, GridSize.y];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                Grid[x, y] = new RoomData();

                if (x == 0 && y == 0) { Grid[x, y].ContainsMonster = false; Grid[x, y].TreasureAmount = 0; }
                else if (x == GridSize.x-1 && y == GridSize.y-1) { Grid[x, y].ContainsExit = true; }
                else
                {
                    Grid[x, y].ContainsMonster = Random.Range(0, 2) == 0;
                    Grid[x, y].MonsterHP = 10;
                    Grid[x, y].TreasureAmount = Random.Range(0, 2) == 0 ? 0 : Random.Range(10,101);
                }

                Grid[x, y].Directions = new Directions
                {
                    North = y < GridSize.y - 1,
                    East = x < GridSize.x -1,
                    South = y > 0,
                    West = x > 0
                };
            }
        }
    }

    public void UpdateRoomData(Vector2Int _roomPos)
    {
        List<Player> _playersInRoom = PlayerManager.Instance.GetPlayersInRoom(_roomPos);

        Grid[_roomPos.x,_roomPos.y].NumberOfOtherPlayers = _playersInRoom.Count;
        Grid[_roomPos.x, _roomPos.y].OtherPlayersIDs.Clear();

        foreach (var _player in _playersInRoom)
        {
            Grid[_roomPos.x, _roomPos.y].OtherPlayersIDs.Add(_player.ConnectionID);
        }
    }

    public Vector2Int GetRoomFromDirectionInByte(Vector2Int _currentPos, byte _directionByte)
    {
        RoomData _currentRoom = Grid[_currentPos.x, _currentPos.y];
        Vector2Int _newRoom = -Vector2Int.one;
        DirectionEnum _direction = (DirectionEnum)_directionByte;

        switch (_direction)
        {
            case DirectionEnum.North:
                if (_currentRoom.Directions.North)
                {
                    _newRoom = _currentPos + new Vector2Int(0, 1);
                }
                break;
            case DirectionEnum.East:
                if (_currentRoom.Directions.East)
                {
                    _newRoom = _currentPos + new Vector2Int(1, 0);
                }
                break;
            case DirectionEnum.South:
                if (_currentRoom.Directions.South)
                {
                    _newRoom = _currentPos - new Vector2Int(0, 1);
                }
                break;
            case DirectionEnum.West:
                if (_currentRoom.Directions.West)
                {
                    _newRoom = _currentPos - new Vector2Int(1, 0);
                }
                break;
        }

        return _newRoom;
    }
}