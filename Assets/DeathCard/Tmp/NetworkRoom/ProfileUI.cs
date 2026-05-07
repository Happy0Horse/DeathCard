using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ProfileUI : MonoBehaviour
{
    public GameObject profilePanel;
    public TMP_InputField nameInput;
    public RawImage avatarImage;
    public Button selectAvatarButton;
    public Button saveButton;
    public Button closeButton;

    void Start()
    {
        profilePanel.SetActive(false);
        nameInput.text = PlayerProfile.instance.playerName;

        Texture2D tex = PlayerProfile.instance.GetAvatarTexture();
        if (tex != null)
            avatarImage.texture = tex;
    }

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
        nameInput.text = PlayerProfile.instance.playerName;

        Texture2D tex = PlayerProfile.instance.GetAvatarTexture();
        if (tex != null)
            avatarImage.texture = tex;
    }

    public void OnSelectAvatarClicked()
    {
        // Открываем файловый диалог
        string path = OpenFileDialog();
        if (string.IsNullOrEmpty(path)) return;

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        PlayerProfile.instance.SetAvatar(tex);
        avatarImage.texture = PlayerProfile.instance.GetAvatarTexture();
    }

    public void OnSaveClicked()
    {
        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "Player";

        PlayerProfile.instance.playerName = name;
        PlayerProfile.instance.Save();

        profilePanel.SetActive(false);
    }

    public void OnCloseClicked()
    {
        profilePanel.SetActive(false);
    }

    string OpenFileDialog()
    {
#if UNITY_STANDALONE_LINUX
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "zenity";
            process.StartInfo.Arguments = "--file-selection --file-filter=\"Images | *.png *.jpg *.jpeg\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            string result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return result;
        }
        catch { return ""; }
#elif UNITY_STANDALONE_WIN
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = "-Command \"Add-Type -AssemblyName System.Windows.Forms; " +
                "$f = New-Object System.Windows.Forms.OpenFileDialog; " +
                "$f.Filter = 'Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg'; " +
                "if ($f.ShowDialog() -eq 'OK') { $f.FileName }\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return result;
        }
        catch { return ""; }
        return "";
#else
        return "";
#endif
    }
}