using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Code;

public class GameManager : Singleton<GameManager>
{
    public Room MyRoom;
    public RoomData MyRoomData;
    public GameObject Lobby;
    public bool GameHasStarted = false;

    public string[] RandomNames;

    [SerializeField] ushort startHP = 100;
    
    [SerializeField] int maxLineAmount = 20;
    [SerializeField] TextMeshProUGUI outputText;

    [SerializeField] GameObject endScreenCam;
    [SerializeField] GameObject statisticsCam;

    private ClientBehaviour cb;
    private ServerBehaviour sb;

    private List<string> outputLines = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        MyRoom.gameObject.SetActive(false);
    }

    public void WriteLine(string _newLine)
    {
        outputLines.Add(_newLine);
        if (outputLines.Count > maxLineAmount) { outputLines.RemoveAt(0); }

        outputText.text = "";
        foreach (var _line in outputLines)
        {
            outputText.text += $"{_line}\n";
        }
    }

    public void Host()
    {
        sb = gameObject.AddComponent<ServerBehaviour>();
        WriteLine("Started a server");
        Join(true);
    }

    public void Join(bool _isHost = false)
    {
        cb = gameObject.AddComponent<ClientBehaviour>();
        cb.OnStart(UIManager.Instance.NameText.text, UIManager.Instance.IPAdressText.text);
        WriteLine("Joining");

        UIManager.Instance.OnStart(_isHost);
    }

    public void HostStartGame()
    {
        statisticsCam.SetActive(false);
        GameHasStarted = true;
        sb.StartGame(startHP);
    }

    public void ClientStartGame(ushort _startHP)
    {
        statisticsCam.SetActive(false);
        WriteLine("Starting the game");
        CloseLobby();
        DisplayRoom();
    }

    public void CloseLobby()
    {
        Lobby.SetActive(false);
    }

    public void DisplayRoom()
    {
        MyRoom.UpdateRoom(MyRoomData);
        MyRoom.gameObject.SetActive(true);
    }

    public void UpdateRoom()
    {
        MyRoom.UpdateRoom(MyRoomData);
    }

    public void ClientEndGame(HighScorePair[] _highScorePairs)
    {
        endScreenCam.SetActive(true);
        UIManager.Instance.DisplayEndScreen(_highScorePairs);
    }

    #region In-Game Actions
    public void GoNorth()
    {
        cb.SendMoveRequestMessage(DirectionEnum.North);
    }
    public void GoEast()
    {
        cb.SendMoveRequestMessage(DirectionEnum.East);
    }
    public void GoSouth()
    {
        cb.SendMoveRequestMessage(DirectionEnum.South);
    }
    public void GoWest()
    {
        cb.SendMoveRequestMessage(DirectionEnum.West);
    }

    public void Claim()
    {
        cb.SendClaimTreasureRequestMessage();
    }

    public void Attack()
    {
        cb.SendAttackRequestMessage();
    }
    public void Defend()
    {
        cb.SendDefendRequestMessage();
    }

    public void Exit()
    {
        cb.SendLeaveDungeonRequestMessage();
    }
   
    public void ShowStatistics()
    {
        statisticsCam.SetActive(true);
        //#TODO: haal statistics binnen van database
    }

    public void CloseStatistics()
    {
        statisticsCam.SetActive(false);
    }
    #endregion
}

