using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using Assets.Code;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;

public class ServerBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    private Queue<MessageHeader> messagesQueue;

    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];

    public int MaxConnections = 4;

    private void Start()
    {
        networkJobHandle.Complete();

        networkDriver = NetworkDriver.Create();
        var _endpoint = NetworkEndPoint.AnyIpv4;
        _endpoint.Port = 9000;
        if (networkDriver.Bind(_endpoint) != 0)
        {
            Debug.Log("Failed to bind port");
        }
        else
        {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        messagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ServerCallbacks.Length; i++)
        {
            ServerCallbacks[i] = new MessageEvent();
        }
        //ServerCallbacks[(int)MessageHeader.MessageType.SetName].AddListener(HandleSetName);
    }

    private void Update()
    {
        networkJobHandle.Complete();

        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
        NetworkConnection _connection;

        //Check if the max amount of connections has been reached or if the game has already started
        while ((_connection = networkDriver.Accept()) != default && connections.Length < MaxConnections && !GameManager.Instance.GameHasStarted)
        {
            connections.Add(_connection);
            Debug.Log("Accepted connection");

            Color[] _colors = { Color.cyan, Color.magenta, Color.red, Color.green };

            var _color = (Color32)_colors[_connection.InternalId];

            WelcomeMessage _welcomeMessage = new WelcomeMessage
            {
                PlayerID = _connection.InternalId,
                Colour = ColorExtensions.ToUInt(_color)
            };

            PlayerManager.Instance.AddNewPlayer(_welcomeMessage.PlayerID, _welcomeMessage.Colour);

            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _welcomeMessage, _connection);
        }

        DataStreamReader _reader;
        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type _cmd;
            while ((_cmd = networkDriver.PopEventForConnection(connections[i], out _reader)) != NetworkEvent.Type.Empty)
            {
                if (_cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)_reader.ReadUShort();
                    switch (messageType)
                    {
                        default:
                        case MessageHeader.MessageType.None:
                            SendStayAliveMessage(connections[i]);
                            break;

                        //Lobby Messages
                        case MessageHeader.MessageType.SetName:
                            HandleSetNameMessage(ref _reader, i);
                            break;

                        //Game Messages
                        case MessageHeader.MessageType.MoveRequest:
                            HandleMoveRequestMessage(ref _reader, i);
                            break;
                        case MessageHeader.MessageType.AttackRequest:
                            HandleAttackRequestMessage(ref _reader, i);
                            break;
                        case MessageHeader.MessageType.DefendRequest:
                            HandleDefendRequestMessage(ref _reader, i);
                            break;
                        case MessageHeader.MessageType.ClaimTreasureRequest:
                            HandleClaimTreasureRequestMessage(ref _reader, i);
                            break;
                        case MessageHeader.MessageType.LeaveDungeonRequest:
                            HandleLeaveDungeonRequestMessage(ref _reader, i);
                            break;
                    }
                }
                else if (_cmd == NetworkEvent.Type.Disconnect)
                {
                    SendPlayerLeftMessages(i);
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        ProcessMessagesQueue();
        networkJobHandle = networkDriver.ScheduleUpdate();
    }

    private void ProcessMessagesQueue()
    {
        while (messagesQueue.Count > 0)
        {
            var _message = messagesQueue.Dequeue();
            if(ServerCallbacks.Length > (int)_message.Type) { ServerCallbacks[(int)_message.Type].Invoke(_message); }
        }
    }

    private void OnDestroy()
    {
        networkJobHandle.Complete();
        networkDriver.Dispose();
        connections.Dispose();
    }

    #region HandleIncomingMessages

    private void HandleSetNameMessage(ref DataStreamReader _reader, int _connectionIndex)
    {
        SetNameMessage _message = MessageHandler.ReadMessage<SetNameMessage>(_reader, messagesQueue) as SetNameMessage;
        PlayerManager.Instance.SetPlayerName(_connectionIndex, _message.Name);
        SendNewPlayerMessages(connections[_connectionIndex].InternalId);
    }

    private void HandleMoveRequestMessage(ref DataStreamReader _reader, int _connectionIndex)
    {
        MoveRequestMessage _message = MessageHandler.ReadMessage<MoveRequestMessage>(_reader, messagesQueue) as MoveRequestMessage;

        //Check of client aan de beurt is
        if (!ConnectionIsTurn(_connectionIndex)) { SendRequestDeniedMessage(_message, _connectionIndex); return; }

        Player _player = PlayerManager.Instance.GetPlayer(_connectionIndex);

        Vector2Int _oldRoomPos = _player.Position;
        Vector2Int _newRoomPos = HostDataManager.Instance.GetRoomFromDirectionInByte(_oldRoomPos, _message.Direction);

        RoomData _oldRoom = HostDataManager.Instance.Grid[_oldRoomPos.x, _oldRoomPos.y];

        //Check if new room is available and if there isn't a monster in the current room
        if (_newRoomPos == -Vector2Int.one || _oldRoom.ContainsMonster)
        {
            SendRequestDeniedMessage(_message, _connectionIndex);
            return;
        }

        _player.Position = _newRoomPos;

        HostDataManager.Instance.UpdateRoomData(_oldRoomPos);
        HostDataManager.Instance.UpdateRoomData(_newRoomPos);

        SendRoomInfoMessages(_newRoomPos, _connectionIndex);

        SendPlayerLeaveRoomMessages(_oldRoomPos, _player.ConnectionID);
        SendPlayerEnterRoomMessages(_newRoomPos, _player.ConnectionID);

        NextPlayerTurn();
    }

    private void HandleAttackRequestMessage(ref DataStreamReader _reader, int _connectionIndex)
    {
        AttackRequestMessage _message = MessageHandler.ReadMessage<AttackRequestMessage>(_reader, messagesQueue) as AttackRequestMessage;

        //Check of client aan de beurt is
        if (!ConnectionIsTurn(_connectionIndex)) { SendRequestDeniedMessage(_message, _connectionIndex); return; }

        Player _player = PlayerManager.Instance.GetPlayer(_connectionIndex);
        RoomData _roomData = HostDataManager.Instance.Grid[_player.Position.x, _player.Position.y];

        //Check if there is a monster in the room
        if (_roomData.ContainsMonster)
        {
            //Damage the monster
            _roomData.AttackMonster(10);
            SendHitMonsterMessages(_player.ConnectionID, 10);

            //Check if the monster died from the attack
            if (_roomData.MonsterHP <= 0)
            {
                //Damage the player
                _player.TakeDamage(2);
                SendHitByMonsterMessages(_connectionIndex, (ushort)_player.HP);

                //Check if the player is dead
                if (_player.HP <= 0)
                {
                    //Remove the player from the player list and send the "PlayerDies" message to all clients
                    PlayerManager.Instance.RemovePlayer(_player.ConnectionID);
                    SendPlayerDiesMessages(_player.ConnectionID);
                }
            }

            NextPlayerTurn();
        }
        else
        {
            SendRequestDeniedMessage(_message, _connectionIndex);
        }
    }

    private void HandleDefendRequestMessage(ref DataStreamReader _reader, int _connectionIndex)
    {
        DefendRequestMessage _message = MessageHandler.ReadMessage<DefendRequestMessage>(_reader, messagesQueue) as DefendRequestMessage;

        //Check of client aan de beurt is
        if (!ConnectionIsTurn(_connectionIndex)) { SendRequestDeniedMessage(_message, _connectionIndex); return; }

        Player _player = PlayerManager.Instance.GetPlayer(_connectionIndex);
        RoomData _roomData = HostDataManager.Instance.Grid[_player.Position.x, _player.Position.y];

        //Check if there is a monster in the room
        if (_roomData.ContainsMonster)
        {
            //Damage the player
            _player.TakeDamage(1);
            SendHitByMonsterMessages(_connectionIndex, (ushort)_player.HP);

            //Check if the player is dead
            if (_player.HP <= 0)
            {
                //Remove the player from the player list and send the "PlayerDies" message to all clients
                PlayerManager.Instance.RemovePlayer(_player.ConnectionID);
                SendPlayerDiesMessages(_player.ConnectionID);
            }

            else
            {
                //Heal the player
                _player.Heal(3);
                SendPlayerDefendsMessages(_connectionIndex, (ushort)_player.HP);
            }

            NextPlayerTurn();
        }
        else
        {
            SendRequestDeniedMessage(_message, _connectionIndex);
        }
    }

    private void HandleClaimTreasureRequestMessage(ref DataStreamReader _reader, int _connectionIndex)
    {
        ClaimTreasureRequestMessage _message = MessageHandler.ReadMessage<ClaimTreasureRequestMessage>(_reader, messagesQueue) as ClaimTreasureRequestMessage;

        //Check of client aan de beurt is
        if (!ConnectionIsTurn(_connectionIndex)) { SendRequestDeniedMessage(_message, _connectionIndex); return; }

        Player _player = PlayerManager.Instance.GetPlayer(_connectionIndex);
        RoomData _roomData = HostDataManager.Instance.Grid[_player.Position.x, _player.Position.y];

        //Check if there is a treasure and if there isn't a monster guarding it
        if (_roomData.TreasureAmount > 0 && !_roomData.ContainsMonster)
        {
            List<Player> _playersInRoom = PlayerManager.Instance.GetPlayersInRoom(_player.Position);

            //Divide the gold
            int _rest = _roomData.TreasureAmount % _playersInRoom.Count;
            int _dividedGold = (_roomData.TreasureAmount - _rest) / _playersInRoom.Count;

            foreach (var _playerInRoom in _playersInRoom)
            {
                //Give the player that claims the treasure the rest amount of the treasure
                int _finalAmount = _playerInRoom.ConnectionID == _player.ConnectionID ? _dividedGold + _rest : _dividedGold;

                _player.Gold += _finalAmount;
                SendObtainTreasureMessage((ushort)_finalAmount, _playerInRoom.ConnectionID);
            }

            //"Remove" the treasure from the room
            _roomData.TreasureAmount = 0;
            NextPlayerTurn();
        }
        else
        {
            SendRequestDeniedMessage(_message, _connectionIndex);
        }
    }

    private void HandleLeaveDungeonRequestMessage(ref DataStreamReader _reader, int _connectionIndex)
    {
        LeaveDungeonRequestMessage _message = MessageHandler.ReadMessage<LeaveDungeonRequestMessage>(_reader, messagesQueue) as LeaveDungeonRequestMessage;

        //Check of client aan de beurt is
        if (!ConnectionIsTurn(_connectionIndex)) { SendRequestDeniedMessage(_message, _connectionIndex); return; }

        PlayerManager.Instance.RemovePlayer(_connectionIndex);
        SendPlayerLeftDungeonMessage(_connectionIndex);

        NextPlayerTurn();
    }

    #endregion

    #region SendMessages
    private void SendPlayerLeaveRoomMessages(Vector2Int _roomPos, int _playerID)
    {
        PlayerLeaveRoomMessage _message = new PlayerLeaveRoomMessage
        {
            PlayerID = _playerID
        };

        foreach (var _player in PlayerManager.Instance.GetPlayersInRoom(_roomPos))
        {
            if(_player.ConnectionID == _playerID) { continue; }

            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[_player.ConnectionID]);
        }
    }

    private void SendPlayerEnterRoomMessages(Vector2Int _roomPos, int _playerID)
    {
        PlayerEnterRoomMessage _message = new PlayerEnterRoomMessage
        {
            PlayerID = _playerID
        };

        foreach (var _player in PlayerManager.Instance.GetPlayersInRoom(_roomPos))
        {
            if (_player.ConnectionID == _playerID) { continue; }

            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[_player.ConnectionID]);
        }
    }

    private void SendRequestDeniedMessage(MessageHeader _message, int _connectionIndex)
    {
        RequestDeniedMessage _deniedMessage = new RequestDeniedMessage()
        {
            DeniedMessageID = _message.ID
        };

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _deniedMessage, connections[_connectionIndex]);
    }

    /// <summary>
    /// Send a "RoomInfoMessage" to all players in the given room.
    /// </summary>
    /// <param name="_roomPosition"></param>
    private void SendRoomInfoMessages(Vector2Int _roomPosition, int _connectionIndex = -1)
    {
        HostDataManager.Instance.UpdateRoomData(_roomPosition);
        RoomData _roomData = HostDataManager.Instance.Grid[_roomPosition.x, _roomPosition.y];

        RoomInfoMessage _roomInfoMessage = new RoomInfoMessage
        {
            MoveDirections = _roomData.GetDirsByte(),
            TreasureInRoom = (ushort)_roomData.TreasureAmount,
            ContainsMonster = (byte)(_roomData.ContainsMonster ? 1 : 0),
            ContainsExit = (byte)(_roomData.ContainsExit ? 1 : 0),
            NumberOfOtherPlayers = (byte)_roomData.NumberOfOtherPlayers,
            OtherPlayerIDs = _roomData.OtherPlayersIDs,
        };

        foreach (var _player in PlayerManager.Instance.GetPlayersInRoom(_roomPosition))
        {
            if (_connectionIndex != -1 && _player.ConnectionID != _connectionIndex) { continue; }

            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _roomInfoMessage, connections[_player.ConnectionID]);
        }

    }

    private void SendNewPlayerMessages(int _newPlayerID)
    {
        NewPlayerMessage _newPlayerMessage = new NewPlayerMessage
        {
            PlayerID = _newPlayerID,
            PlayerName = PlayerManager.Instance.GetPlayer(_newPlayerID).Name,
            PlayerColour = ColorExtensions.ToUInt(PlayerManager.Instance.GetPlayer(_newPlayerID).Color)
        };

        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].InternalId != _newPlayerID)
            {
                //Send the new player to this client
                networkJobHandle.Complete();
                MessageHandler.SendMessage(networkDriver, _newPlayerMessage, connections[i]);

                //Send this client to the new player
                NewPlayerMessage _currentPlayerMessage = new NewPlayerMessage
                {
                    PlayerID = connections[i].InternalId,
                    PlayerName = PlayerManager.Instance.GetPlayer(connections[i].InternalId).Name,
                    PlayerColour = ColorExtensions.ToUInt(PlayerManager.Instance.GetPlayer(connections[i].InternalId).Color)
                };

                networkJobHandle.Complete();
                MessageHandler.SendMessage(networkDriver, _currentPlayerMessage, connections[_newPlayerID]);
            }
        }
    }

    private void SendPlayerLeftMessages(int _playerLeftID)
    {
        PlayerLeftMessage _playerLeftMessage = new PlayerLeftMessage
        {
            PlayerLeftID = _playerLeftID
        };

        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].InternalId != _playerLeftID)
            {
                networkJobHandle.Complete();
                MessageHandler.SendMessage(networkDriver, _playerLeftMessage, connections[i]);
            }
        }
    }

    private void SendStayAliveMessage(NetworkConnection _connection)
    {
        StayAliveMessage _stayAliveMessage = new StayAliveMessage();

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _stayAliveMessage, _connection);
    }

    private void SendStartGameMessages(ushort _startingHP)
    {
        StartGameMessage _startGameMessage = new StartGameMessage
        {
            StartHP = _startingHP
        };

        SendRoomInfoMessages(new Vector2Int(0, 0));

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _startGameMessage, connections[i]);
        }
    }

    private void SendPlayerTurnMessages()
    {
        PlayerTurnMessage _message = new PlayerTurnMessage
        {
            PlayerID = HostDataManager.Instance.PlayerTurnIndex
        };

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[i]);
        }
    }

    private void SendHitMonsterMessages(int _attackingPlayerID, ushort _damageDealt)
    {
        HitMonsterMessage _message = new HitMonsterMessage
        {
            PlayerID = _attackingPlayerID,
            DamageDealt = _damageDealt
        };

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[i]);
        }
    }

    private void SendHitByMonsterMessages(int _targetedPlayerID, ushort _newHP)
    {
        HitByMonsterMessage _message = new HitByMonsterMessage
        {
            PlayerID = _targetedPlayerID,
            newHP = _newHP
        };

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[i]);
        }
    }

    private void SendPlayerDefendsMessages(int _defendingPlayerID, ushort _newHP)
    {
        PlayerDefendsMessage _message = new PlayerDefendsMessage
        {
            PlayerID = _defendingPlayerID,
            newHP = _newHP
        };

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[i]);
        }
    }

    private void SendPlayerDiesMessages(int _deadPlayerID)
    {
        PlayerDiesMessage _message = new PlayerDiesMessage
        {
            PlayerID = _deadPlayerID
        };

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[i]);
        }

        //Check if the game should be ended
        if (!AreAnyPlayersLeft()) { SendEndGameMessages(); }
    }

    private void SendObtainTreasureMessage(ushort _amount, int _connectionID)
    {
        ObtainTreasureMessage _message = new ObtainTreasureMessage
        {
            Amount = _amount
        };

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connections[_connectionID]);
    }

    private void SendPlayerLeftDungeonMessage(int _connectionID)
    {
        PlayerLeftDungeonMessage _message = new PlayerLeftDungeonMessage
        {
            PlayerID = _connectionID
        };

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connections[_connectionID]);

        //Check if the game should be ended
        if (!AreAnyPlayersLeft()) { SendEndGameMessages(); }
    }

    private void SendEndGameMessages()
    {
        List<Player> _players = PlayerManager.Instance.RemovedPlayers;
        List<HighScorePair> _highScorePairs = new List<HighScorePair>();

        //Make highscore pairs
        foreach (var _player in PlayerManager.Instance.RemovedPlayers)
        {
            HighScorePair _pair = new HighScorePair(_player.ConnectionID, (ushort)_player.Gold);
            _highScorePairs.Add(_pair);
        }

        EndGameMessage _message = new EndGameMessage
        {
            NumberOfScores = (byte)_players.Count,
            PlayerIDHighscorePairs = _highScorePairs.ToArray()
        };

        for (int i = 0; i < connections.Length; i++)
        {
            networkJobHandle.Complete();
            MessageHandler.SendMessage(networkDriver, _message, connections[i]);
        }

    }
    #endregion

    /// <summary>
    /// Check if the client with this connectionID is at turn
    /// </summary>
    /// <param name="_connectionID"></param>
    /// <returns></returns>
    private bool ConnectionIsTurn(int _connectionID)
    {
        return _connectionID == HostDataManager.Instance.PlayerTurnIndex;
    }

    private void NextPlayerTurn()
    {
        HostDataManager.Instance.IncrementPlayerTurnIndex();
        SendPlayerTurnMessages();
    }

    private bool AreAnyPlayersLeft()
    {
        return PlayerManager.Instance.Players.Count > 0;
    }

    public void StartGame(ushort _startingHP)
    {
        networkJobHandle.Complete();
        SendStartGameMessages(_startingHP);
    }
}
