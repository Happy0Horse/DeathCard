using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuWindowManager : MonoBehaviour
{
    [Header("Окна меню")]
    [SerializeField] private List<GameObject> windows;
    
    private GameObject currentWindow;
    private GameObject previousWindow;
    
    void Start()
    {
        currentWindow = windows[0];
    }
    
    public void OpenWindow(GameObject window)
    {
        if (window == null) return;
        
        if (currentWindow == window) return;
        
        if (currentWindow != null && currentWindow.activeSelf)
        {
            previousWindow = currentWindow;
            currentWindow.SetActive(false);
        }
        
        currentWindow = window;
        currentWindow.SetActive(true);
        
        Debug.Log($"Открыто окно: {window.name} | Предыдущее: {(previousWindow != null ? previousWindow.name : "null")}");
    }
    
    public void OnBackButtonClick()
    {
        if (previousWindow != null)
        {
            if (currentWindow != null)
                currentWindow.SetActive(false);
            
            currentWindow.SetActive(false);
            currentWindow = previousWindow;
            currentWindow.SetActive(true);
            
            previousWindow = null;
            
            Debug.Log($"Возврат к окну: {currentWindow.name}");
        }
    }
    
    public void CloseCurrentWindow()
    {
        if (currentWindow != null)
        {
            currentWindow.SetActive(false);
            currentWindow = null;
            previousWindow = null;
        }
    }
}