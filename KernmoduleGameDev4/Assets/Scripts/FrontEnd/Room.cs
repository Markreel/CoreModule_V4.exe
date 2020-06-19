using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Room : MonoBehaviour
{
    public CanvasGroup CanvasGroup;

    [SerializeField] Button north;
    [SerializeField] Button east;
    [SerializeField] Button south;
    [SerializeField] Button west;

    [SerializeField] GameObject treasure;
    [SerializeField] Button treasureClaimButton;

    [SerializeField] GameObject monster;
    [SerializeField] TextMeshProUGUI monsterHP;

    [SerializeField] GameObject exit;

    [SerializeField] List<GameObject> players = new List<GameObject>();
    [SerializeField] List<TextMeshProUGUI> playerNames = new List<TextMeshProUGUI>();

    [SerializeField] TextMeshProUGUI playerTurnDisplay;
    
    public void UpdateRoom(RoomData _roomData)
    {
        CanvasGroup.interactable = PlayerManager.Instance.CurrentPlayerID == PlayerManager.Instance.MyPlayerID;
        playerTurnDisplay.text = PlayerManager.Instance.CurrentPlayerID == PlayerManager.Instance.MyPlayerID ? "It's your turn!" : "Wait for your turn";

        north.gameObject.SetActive(_roomData.Directions.North);
        east.gameObject.SetActive(_roomData.Directions.East);
        south.gameObject.SetActive(_roomData.Directions.South);
        west.gameObject.SetActive(_roomData.Directions.West);

        north.interactable = east.interactable = south.interactable = west.interactable = _roomData.ContainsMonster ? false : true;

        treasure.SetActive(_roomData.TreasureAmount > 0);
        treasureClaimButton.gameObject.SetActive(_roomData.ContainsMonster ? false : true);

        monster.SetActive(_roomData.ContainsMonster);
        monsterHP.text = _roomData.MonsterHP.ToString();

        exit.SetActive(_roomData.ContainsExit);

        //Remove players from room
        for (int i = players.Count; i > _roomData.NumberOfOtherPlayers; i--)
        {
            players[i-1].SetActive(false);
            playerNames[i-1].text = "";
        }

        //Check if there are even any players left
        if(PlayerManager.Instance.Players.Count == 0) { return; }

        //Add players to room
        for (int i = 0; i < _roomData.NumberOfOtherPlayers; i++)
        {
            players[i].SetActive(true);
            playerNames[i].text = PlayerManager.Instance.GetPlayer(_roomData.OtherPlayersIDs[i]).Name;
        }
    }
}
