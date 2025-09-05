using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private GridBoard gridBoard;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject pauseMenu;
    
    [Header("Game State")]
    private bool isPaused = false;
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    
    private void InitializeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        
        if (gameUI != null)
            gameUI.SetActive(true);
            
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenu != null)
                pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenu != null)
                pauseMenu.SetActive(false);
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    
    public void QuitGame()
    {
        Debug.Log("Sair do jogo...");
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
