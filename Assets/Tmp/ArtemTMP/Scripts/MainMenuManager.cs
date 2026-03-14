using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Persistent Cards")]
    [SerializeField] private CardState titleCard;
    [SerializeField] private CardState aboutCard;

    [Header("Menu Groups")]
    [SerializeField] private GameObject menuAndSettingsGroup;
    [SerializeField] private GameObject customizeGroup;
    [SerializeField] private GameObject aboutGroup;
    [SerializeField] private GameObject matchmakingGroup;

    // Matchmaking nav logic
    public bool matchmakingFlag = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() => OpenMainMenu();

    public void OpenMainMenu()
    {
        if (matchmakingFlag)
        {
            matchmakingFlag = false;
            OpenMatchmakingMenu();
            return;
        }

        // Logic for Persistent Cards
        titleCard.gameObject.SetActive(true);
        aboutCard.gameObject.SetActive(true);
        aboutCard.SetState(true); // "Entrance" button

        ToggleGroups(menuAndSettingsGroup);

        SetFourCardsState(true);
    }

    public void OpenSettingsMenu()
    {
        titleCard.gameObject.SetActive(true);
        ToggleGroups(menuAndSettingsGroup);
        
        SetFourCardsState(false);
    }

    public void OpenCustomizeMenu()
    {
        titleCard.gameObject.SetActive(true);
        ToggleGroups(customizeGroup);
    }

    public void OpenAboutMenu()
    {
        titleCard.gameObject.SetActive(false);
        aboutCard.SetState(false);
        ToggleGroups(aboutGroup);
    }

    public void OpenMatchmakingMenu()
    {
        titleCard.gameObject.SetActive(false);
        aboutCard.gameObject.SetActive(false);
        ToggleGroups(matchmakingGroup);

        matchmakingFlag = true;
    }

    private void ToggleGroups(GameObject activeGroup)
    {
        menuAndSettingsGroup.SetActive(activeGroup == menuAndSettingsGroup);
        customizeGroup.SetActive(activeGroup == customizeGroup);
        aboutGroup.SetActive(activeGroup == aboutGroup);
        matchmakingGroup.SetActive(activeGroup == matchmakingGroup);
    }

    private void SetFourCardsState(bool isMain)
    {
        foreach (CardState card in menuAndSettingsGroup.GetComponentsInChildren<CardState>(true))
        {
            card.SetState(isMain);
        }
    }

    public void BackToMainFromMatchmaking()
    {
        matchmakingFlag = false;
        OpenMainMenu();
    }

    #region Scene & Exit Logic

    public void LoadGameScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in Build Settings!");
        }
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string nameOnly = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (nameOnly == sceneName) return true;
        }
        return false;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}