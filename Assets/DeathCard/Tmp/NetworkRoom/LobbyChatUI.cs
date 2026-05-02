using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class LobbyChatUI : MonoBehaviour
{
    public TextMeshProUGUI chatText;
    public TMP_InputField chatInput;
    public Button sendButton;
    public ScrollRect scrollRect;

    public int maxMessages = 50;
    private System.Collections.Generic.List<string> messages = new System.Collections.Generic.List<string>();

    void Start()
    {
        NetworkClient.RegisterHandler<ChatMessage>(OnChatMessage);
        sendButton.onClick.AddListener(OnSendClicked);
        chatInput.onSubmit.AddListener(_ => OnSendClicked());
        chatText.text = "";
    }

    void OnSendClicked()
    {
        string text = chatInput.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        NetworkClient.Send(new SendChatMessage { text = text });
        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    void OnChatMessage(ChatMessage msg)
    {
        if (messages.Count >= maxMessages)
            messages.RemoveAt(0);

        messages.Add($"<color=#aaaaff>{msg.senderName}:</color> {msg.text}");
        chatText.text = string.Join("\n", messages);

        // Скроллим вниз
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}