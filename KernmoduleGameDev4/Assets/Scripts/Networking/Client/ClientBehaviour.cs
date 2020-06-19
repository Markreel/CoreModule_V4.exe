using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using System.IO;
using Assets.Code;
using Unity.Jobs;

public class ClientBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NetworkConnection connection;

    private JobHandle networkJobHandle;

    private Queue<MessageHeader> messagesQueue;

    private string name;
    private float stayAliveInterval = 10f;

    public MessageEvent[] ClientCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];

    public NetworkEndPoint NetworkEndPoint;

    public ClientBehaviour(string _ipAdress = null)
    {
        if (_ipAdress != null) { NetworkEndPoint = NetworkEndPoint.Parse(_ipAdress, 9000); }
        else { NetworkEndPoint = NetworkEndPoint.LoopbackIpv4; NetworkEndPoint.Port = 9000; }
    }

    private void Update()
    {
        networkJobHandle.Complete();    

        if (!connection.IsCreated)
        {
            return;
        }

        DataStreamReader _reader;
        NetworkEvent.Type _cmd;
        while ((_cmd = connection.PopEvent(networkDriver, out _reader)) != NetworkEvent.Type.Empty)
        {
            if (_cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to server");
                StartStayAliveCheck();
            }
            else if (_cmd == NetworkEvent.Type.Data)
            {
                var _messageType = (MessageHeader.MessageType)_reader.ReadUShort();
                switch (_messageType)
                {
                    default:
                    case MessageHeader.MessageType.None:
                        break;

                    //Lobby Messages
                    case MessageHeader.MessageType.NewPlayer:
                        HandleNewPlayerMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.Welcome:
                        HandleWelcomeMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.RequestDenied:
                        HandleRequestDeniedMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.PlayerLeft:
                        HandlePlayerLeftMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.StartGame:
                        HandleStartGameMessage(ref _reader);
                        break;


                    //Game Messages
                    case MessageHeader.MessageType.PlayerTurn:
                        HandlePlayerTurnMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.RoomInfo:
                        HandleRoomInfoMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.PlayerEnterRoom:
                        HandlePlayerEnterRoomMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.PlayerLeaveRoom:
                        HandlePlayerLeaveRoomMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.ObtainTreassure:
                        HandleObtainTreasureMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.HitMonster:
                        HandleHitMonsterMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.HitByMonster:
                        HandleHitByMonsterMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.PlayerDefends:
                        HandlePlayerDefendsMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.PlayerLeftDungeon:
                        HandlePlayerLeftDungeonMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.PlayerDies:
                        HandlePlayerDiesMessage(ref _reader);
                        break;
                    case MessageHeader.MessageType.EndGame:
                        HandleEndGameMessage(ref _reader);
                        break;
                }

            }
            else if (_cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }

        ProcessMessagesQueue();
        networkJobHandle = networkDriver.ScheduleUpdate();
    }

    #region HandleIncomingMessages
    private void HandleNewPlayerMessage(ref DataStreamReader _reader)
    {
        NewPlayerMessage _message = MessageHandler.ReadMessage<NewPlayerMessage>(_reader, messagesQueue) as NewPlayerMessage;
        PlayerManager.Instance.AddNewPlayer(_message.PlayerID, _message.PlayerColour, _message.PlayerName);

        GameManager.Instance.WriteLine($"{_message.PlayerName} has joined");
    }

    private void HandleWelcomeMessage(ref DataStreamReader _reader)
    {
        WelcomeMessage _message = MessageHandler.ReadMessage<WelcomeMessage>(_reader, messagesQueue) as WelcomeMessage;
        PlayerManager.Instance.AddNewPlayer(_message.PlayerID, _message.Colour, name);
        PlayerManager.Instance.MyPlayerID = _message.PlayerID;
        SetNameMessage _setNameMessage = new SetNameMessage
        {
            Name = name
        };

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _setNameMessage, connection);

        GameManager.Instance.WriteLine($"Welcome {name}!");
    }

    private void HandleRequestDeniedMessage(ref DataStreamReader _reader)
    {
        RequestDeniedMessage _message = MessageHandler.ReadMessage<RequestDeniedMessage>(_reader, messagesQueue) as RequestDeniedMessage;
        GameManager.Instance.WriteLine("Request denied of ID: " + _message.DeniedMessageID);
    }

    private void HandlePlayerLeftMessage(ref DataStreamReader _reader)
    {
        PlayerLeftMessage _message = MessageHandler.ReadMessage<PlayerLeftMessage>(_reader, messagesQueue) as PlayerLeftMessage;
        PlayerManager.Instance.RemovePlayer(_message.PlayerLeftID);

        GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerLeftID).Name} has left");
    }

    private void HandleStartGameMessage(ref DataStreamReader _reader)
    {
        StartGameMessage _message = MessageHandler.ReadMessage<StartGameMessage>(_reader, messagesQueue) as StartGameMessage;
        GameManager.Instance.ClientStartGame(_message.StartHP);
        PlayerManager.Instance.SetPlayerHP(connection.InternalId, _message.StartHP);

        GameManager.Instance.WriteLine("The game is on!");
    }

    private void HandlePlayerTurnMessage(ref DataStreamReader _reader)
    {
        PlayerTurnMessage _message = MessageHandler.ReadMessage<PlayerTurnMessage>(_reader, messagesQueue) as PlayerTurnMessage;
        PlayerManager.Instance.CurrentPlayerID = _message.PlayerID;

        GameManager.Instance.UpdateRoom();

        //Check if there are any players left
        if (PlayerManager.Instance.Players.Count == 0) { return; }

        GameManager.Instance.WriteLine($"It's {PlayerManager.Instance.GetPlayer(_message.PlayerID).Name}'s turn");
    }

    private void HandleRoomInfoMessage(ref DataStreamReader _reader)
    {
        RoomInfoMessage _message = MessageHandler.ReadMessage<RoomInfoMessage>(_reader, messagesQueue) as RoomInfoMessage;

        GameManager.Instance.MyRoomData = RoomData.GetRoomDataFromRoomInfoMessage(_message);
        GameManager.Instance.UpdateRoom();
    }

    private void HandlePlayerEnterRoomMessage(ref DataStreamReader _reader)
    {
        PlayerEnterRoomMessage _message = MessageHandler.ReadMessage<PlayerEnterRoomMessage>(_reader, messagesQueue) as PlayerEnterRoomMessage;

        PlayerManager.Instance.PlayerEnterRoom(_message.PlayerID);
        GameManager.Instance.MyRoomData.AddOtherPlayer(_message.PlayerID);
        GameManager.Instance.UpdateRoom();

        GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID)} has entered the room");
    }

    private void HandlePlayerLeaveRoomMessage(ref DataStreamReader _reader)
    {
        PlayerLeaveRoomMessage _message = MessageHandler.ReadMessage<PlayerLeaveRoomMessage>(_reader, messagesQueue) as PlayerLeaveRoomMessage;

        PlayerManager.Instance.PlayerEnterRoom(_message.PlayerID);
        GameManager.Instance.MyRoomData.RemoveOtherPlayer(_message.PlayerID);
        GameManager.Instance.UpdateRoom();

        GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID)} has left the room");
    }

    private void HandleObtainTreasureMessage(ref DataStreamReader _reader)
    {
        ObtainTreasureMessage _message = MessageHandler.ReadMessage<ObtainTreasureMessage>(_reader, messagesQueue) as ObtainTreasureMessage;

        PlayerManager.Instance.GiveGoldToPlayer(PlayerManager.Instance.MyPlayerID, _message.Amount);

        //Remove the treasure from the room
        GameManager.Instance.MyRoomData.TreasureAmount = 0;
        GameManager.Instance.UpdateRoom();

        GameManager.Instance.WriteLine($"You obtained {_message.Amount} secret file(s)!");
    }

    private void HandleHitMonsterMessage(ref DataStreamReader _reader)
    {
        HitMonsterMessage _message = MessageHandler.ReadMessage<HitMonsterMessage>(_reader, messagesQueue) as HitMonsterMessage;

        Vector2Int _myPos = PlayerManager.Instance.GetPlayer(PlayerManager.Instance.MyPlayerID).Position;
        List<Player> _playersInRoom = PlayerManager.Instance.GetPlayersInRoom(_myPos);

        //Check if the monster that has been hit is in the same room as I am.
        if (_playersInRoom.Find(x => x.ConnectionID == _message.PlayerID) != null)
        {
            GameManager.Instance.MyRoomData.AttackMonster(_message.DamageDealt);
            GameManager.Instance.UpdateRoom();
            GameManager.Instance.WriteLine($"The monster in your room has been attacked by {PlayerManager.Instance.GetPlayer(_message.PlayerID).Name}");
        }
        else
        {
            GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID).Name} has attacked a monster in a different room!");
        }
    }

    private void HandleHitByMonsterMessage(ref DataStreamReader _reader)
    {
        HitByMonsterMessage _message = MessageHandler.ReadMessage<HitByMonsterMessage>(_reader, messagesQueue) as HitByMonsterMessage;

        //Check if the player we need exists
        if (PlayerManager.Instance.GetPlayer(_message.PlayerID) != null)
        {
            PlayerManager.Instance.GetPlayer(_message.PlayerID).HP = _message.newHP;
            GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID).Name} has been attacked by a monster");
        }
    }

    private void HandlePlayerDefendsMessage(ref DataStreamReader _reader)
    {
        PlayerDefendsMessage _message = MessageHandler.ReadMessage<PlayerDefendsMessage>(_reader, messagesQueue) as PlayerDefendsMessage;
        PlayerManager.Instance.GetPlayer(_message.PlayerID).HP = _message.newHP;

        GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID).Name} has defended themselves");
    }

    private void HandlePlayerLeftDungeonMessage(ref DataStreamReader _reader)
    {
        PlayerLeftDungeonMessage _message = MessageHandler.ReadMessage<PlayerLeftDungeonMessage>(_reader, messagesQueue) as PlayerLeftDungeonMessage;

        //Check if the player we need exists
        if (PlayerManager.Instance.GetPlayer(_message.PlayerID) != null)
        {
            GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID).Name} has gone through the exit!");
        }

        PlayerManager.Instance.RemovePlayer(_message.PlayerID);
    }

    private void HandlePlayerDiesMessage(ref DataStreamReader _reader)
    {
        PlayerDiesMessage _message = MessageHandler.ReadMessage<PlayerDiesMessage>(_reader, messagesQueue) as PlayerDiesMessage;
        PlayerManager.Instance.RemovePlayer(_message.PlayerID);

        GameManager.Instance.WriteLine($"{PlayerManager.Instance.GetPlayer(_message.PlayerID).Name} has perished :(");
    }

    private void HandleEndGameMessage(ref DataStreamReader _reader)
    {
        EndGameMessage _message = MessageHandler.ReadMessage<EndGameMessage>(_reader, messagesQueue) as EndGameMessage;

        //Remove any players that are still in the player list for some reason
        foreach (var _player in PlayerManager.Instance.Players)
        {
            PlayerManager.Instance.RemovePlayer(_player.ConnectionID);
        }

        //Display the highscores
        GameManager.Instance.ClientEndGame(_message.PlayerIDHighscorePairs);

        GameManager.Instance.WriteLine("/nThe game has ended, thank you for playing!");
    }
    #endregion

    private void StartStayAliveCheck()
    {
        TimerHandler.Instance.StartTimer("ClientStayAlive", stayAliveInterval, SendStayAliveMessage);
    }

    #region SendMessages

    private void SendStayAliveMessage()
    {
        StayAliveMessage _stayAliveMessage = new StayAliveMessage();

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _stayAliveMessage, connection);

        StartStayAliveCheck();
    }

    public void SendMoveRequestMessage(DirectionEnum _direction)
    {
        MoveRequestMessage _message = new MoveRequestMessage
        {
            Direction = (byte)_direction
        };

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connection);
    }

    public void SendClaimTreasureRequestMessage()
    {
        ClaimTreasureRequestMessage _message = new ClaimTreasureRequestMessage();

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connection);
    }

    public void SendAttackRequestMessage()
    {
        AttackRequestMessage _message = new AttackRequestMessage();

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connection);
    }

    public void SendDefendRequestMessage()
    {
        DefendRequestMessage _message = new DefendRequestMessage();

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connection);
    }

    public void SendLeaveDungeonRequestMessage()
    {
        LeaveDungeonRequestMessage _message = new LeaveDungeonRequestMessage();

        networkJobHandle.Complete();
        MessageHandler.SendMessage(networkDriver, _message, connection);
    }

    #endregion

    private void ProcessMessagesQueue()
    {
        while (messagesQueue.Count > 0)
        {
            var _message = messagesQueue.Dequeue();
            ClientCallbacks[(int)_message.Type].Invoke(_message);
        }
    }

    private void OnDestroy()
    {
        networkJobHandle.Complete();
        networkDriver.Dispose();
    }

    public void OnStart(string _name, string _ipAdress = null)
    {
        ClientCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];
        stayAliveInterval = 10f;

        if (_ipAdress.ToCharArray().Length > 1) { NetworkEndPoint = NetworkEndPoint.Parse(_ipAdress, 9000); Debug.Log(_ipAdress); }
        else { NetworkEndPoint = NetworkEndPoint.LoopbackIpv4; NetworkEndPoint.Port = 9000; }

        if (string.IsNullOrEmpty(_name) || _name.ToCharArray().Length < 2) { _name = GameManager.Instance.RandomNames[Random.Range(0, GameManager.Instance.RandomNames.Length)]; }
        name = _name;

        networkJobHandle.Complete();

        messagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ClientCallbacks.Length; i++)
        {
            ClientCallbacks[i] = new MessageEvent();
        }

        networkDriver = NetworkDriver.Create();
        connection = default;

        connection = networkDriver.Connect(NetworkEndPoint);
    }
}
