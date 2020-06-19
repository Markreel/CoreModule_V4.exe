using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    private int maxAmountOfPlayers = 4;
    public int CurrentPlayerID;
    public int MyPlayerID;

    [SerializeField] public List<Player> Players{ get; private set; } = new List<Player>();
    [SerializeField] public List<Player> RemovedPlayers{ get; private set; } = new List<Player>();

    private void UpdateLobbyPlayerUI()
    {
        UIManager.Instance.UpdateLobbyPlayerUI(Players);
    }

    public void AddNewPlayer(int _connectionID, uint _color, string _name = "Koos Naamloos")
    {
        if(GetPlayer(_connectionID) != null) { return; }

        Player _newPlayer = new Player(_connectionID, _name, _color);

        if(Players.Count < maxAmountOfPlayers) { Players.Add(_newPlayer); }

        UpdateLobbyPlayerUI();
    }

    public void RemovePlayer(int _connectionID)
    {
        int _index = -1;
        foreach (var _player in Players)
        {
            if (_player.ConnectionID == _connectionID) { _index = Players.IndexOf(_player); break; }
        }

        if(_index != -1) { RemovedPlayers.Add(Players[_index]); Players.RemoveAt(_index); }

        UpdateLobbyPlayerUI();
    }

    public void SetPlayerName(int _connectionID, string _name)
    {
        foreach (var _player in Players)
        {
            if(_player.ConnectionID == _connectionID) { _player.Name = _name; }
        }

        UpdateLobbyPlayerUI();
    }

    public void SetPlayerHP(int _connectionID, int _hp)
    {
        foreach (var _player in Players)
        {
            if (_player.ConnectionID == _connectionID) { _player.HP = _hp; }
        }
    }

    public void GiveGoldToPlayer(int _connectionID, int _gold)
    {
        foreach (var _player in Players)
        {
            if (_player.ConnectionID == _connectionID) { _player.Gold += _gold; }
        }
    }

    public Player GetPlayer(int _connectionID)
    {
        foreach (var _player in Players)
        {
            if (_player.ConnectionID == _connectionID) { return _player; }
        }

        return null;
    }

    public Player GetRemovedPlayer(int _connectionID)
    {
        foreach (var _player in RemovedPlayers)
        {
            if (_player.ConnectionID == _connectionID) { return _player; }
        }

        return null;
    }

    public List<Player> GetPlayersInRoom(Vector2 _roomPosition)
    {
        List<Player> _playersInRoom = new List<Player>();

        foreach (var _player in Players)
        {
            if(_player.Position == _roomPosition) { _playersInRoom.Add(_player); }
        }

        return _playersInRoom;
    }

    public void PlayerEnterRoom(int _connectionID)
    {
        GetPlayer(_connectionID).Position = GetPlayer(MyPlayerID).Position;
    }

    public void PlayerLeaveRoom(int _connectionID)
    {
        GetPlayer(_connectionID).Position = -Vector2Int.one;
    }
}
