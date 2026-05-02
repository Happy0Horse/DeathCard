using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile instance;

    public string playerName = "Player";
    public byte[] avatarData = new byte[0];

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Load()
    {
        playerName = PlayerPrefs.GetString("playerName", "Player");
        string avatarBase64 = PlayerPrefs.GetString("avatarData", "");
        avatarData = avatarBase64.Length > 0 
            ? System.Convert.FromBase64String(avatarBase64) 
            : new byte[0];
    }

    public void Save()
    {
        PlayerPrefs.SetString("playerName", playerName);
        PlayerPrefs.SetString("avatarData", System.Convert.ToBase64String(avatarData));
        PlayerPrefs.Save();
    }

    public void SetAvatar(Texture2D texture)
    {
        // Ресайз до 128x128
        Texture2D resized = ResizeTexture(texture, 256, 256);
        // Сжимаем в JPG с качеством 75
        avatarData = resized.EncodeToJPG(75);
        Save();
    }

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    public Texture2D GetAvatarTexture()
    {
        if (avatarData == null || avatarData.Length == 0) return null;
        Texture2D tex = new Texture2D(256, 256);
        tex.LoadImage(avatarData);
        return tex;
    }
}