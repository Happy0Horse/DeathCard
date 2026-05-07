using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class MatchmakingUI : MonoBehaviour
{
    [Header("Лобби")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI countdownText;
    public Button readyButton;
    public TextMeshProUGUI readyButtonText;
    public Button leaveLobbyButton;

    [Header("Карточки игроков")]
    public PlayerCard[] playerCards;

    private bool isReady = false;

    void Start()
    {
        NetworkClient.RegisterHandler<LobbyUpdateMessage>(OnLobbyUpdate);
        NetworkClient.RegisterHandler<CountdownMessage>(OnCountdown);
        NetworkClient.RegisterHandler<RoomStartMessage>(OnRoomStart);

        lobbyPanel.SetActive(false);
    }

    public void OnFindGameClicked()
    {
        NetworkClient.Send(new JoinMatchmakingMessage
        {
            playerName = PlayerProfile.instance.playerName,
            avatarData = PlayerProfile.instance.avatarData
        });

        lobbyPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        isReady = false;
        UpdateReadyButton();

        foreach (var card in playerCards)
            card.SetEmpty();
    }

    public void OnReadyClicked()
    {
        isReady = !isReady;
        NetworkClient.Send(new PlayerReadyMessage { isReady = isReady });
        UpdateReadyButton();
    }

    public void OnLeaveLobbyClicked()
    {
        NetworkClient.Send(new LeaveMatchmakingMessage());
        lobbyPanel.SetActive(false);
        isReady = false;
    }

    void UpdateReadyButton()
    {
        readyButtonText.text = isReady ? "NotReady" : "Ready";
        readyButton.image.color = isReady 
            ? new Color(0.6f, 0.2f, 0.2f)
            : new Color(0.2f, 0.6f, 0.2f);
    }

    void OnLobbyUpdate(LobbyUpdateMessage msg)
    {
        if (playerCards == null) return;

        string[] names = { msg.name0, msg.name1, msg.name2, msg.name3 };
        bool[] ready = { msg.ready0, msg.ready1, msg.ready2, msg.ready3 };
        byte[][] avatars = { msg.avatar0, msg.avatar1, msg.avatar2, msg.avatar3 };

        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i] == null) continue;

            if (i < msg.playerCount)
                playerCards[i].Setup(names[i], ready[i], avatars[i]);
            else
                playerCards[i].SetEmpty();
        }

        if (countdownText != null && !msg.isCountingDown)
            countdownText.gameObject.SetActive(false);
    }

    void OnCountdown(CountdownMessage msg)
    {
        if (countdownText == null) return;
        countdownText.gameObject.SetActive(true);
        countdownText.text = $"Game starts: {msg.secondsLeft}";
    }

    void OnRoomStart(RoomStartMessage msg)
    {
        ((GameNetworkManager)NetworkManager.singleton).ChangeToScene("Maze_Scene");
    }
}