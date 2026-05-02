using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;
    public RawImage avatarImage;
    public Texture2D defaultAvatar;

    public void SetEmpty()
    {
        nameText.text = "Empty";
        statusText.text = "Waiting...";
        statusText.color = Color.gray;
        if (avatarImage != null)
            avatarImage.texture = defaultAvatar;
    }

    public void Setup(string playerName, bool isReady, byte[] avatarData)
    {
        nameText.text = playerName;
        statusText.text = isReady ? "Ready" : "NotReady";
        statusText.color = isReady ? Color.green : Color.red;

        if (avatarImage != null)
        {
            if (avatarData != null && avatarData.Length > 0)
            {
                Texture2D tex = new Texture2D(256, 256);
                tex.LoadImage(avatarData);
                avatarImage.texture = tex;
            }
            else
            {
                avatarImage.texture = defaultAvatar;
            }
        }
    }
}

