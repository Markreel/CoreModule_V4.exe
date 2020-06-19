using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] List<TextMeshProUGUI> lobbyPlayers = new List<TextMeshProUGUI>();

    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button startGameButton;

    [SerializeField] GameObject ipAdressParent;
    [SerializeField] TextMeshProUGUI ipAdressText;
    public TextMeshProUGUI IPAdressText { get { return ipAdressText; } }

    [SerializeField] GameObject nameInputParent;
    [SerializeField] TextMeshProUGUI nameText;
    public TextMeshProUGUI NameText { get { return nameText; } }

    [SerializeField] CanvasGroup endScreen;
    [SerializeField] GameObject endScorePrefab;

    protected override void Awake()
    {
        base.Awake();
        endScreen.gameObject.SetActive(false);
    }

    public void UpdateLobbyPlayerUI(List<Player> _players)
    {
        foreach (var _player in _players)
        {
            lobbyPlayers[_player.ConnectionID].text = _player.Name;
            lobbyPlayers[_player.ConnectionID].color = _player.Color;
        }
    }

    public void OnStart(bool _isHost)
    {
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        ipAdressParent.SetActive(false);
        nameInputParent.SetActive(false);

        if (_isHost) { startGameButton.gameObject.SetActive(true); }
    }

    public void DisplayEndScreen(HighScorePair[] _highscores)
    {
        endScreen.gameObject.SetActive(true);

        foreach (var _highScore in _highscores)
        {
            GameObject _obj = Instantiate(endScorePrefab, endScreen.transform);

            string _name = PlayerManager.Instance.GetRemovedPlayer(_highScore.playerID).Name;
            _obj.GetComponent<TextMeshProUGUI>().text = $"{_name} | {_highScore.score}";
        }
    }
}
