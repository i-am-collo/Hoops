using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Gameplay,
    GameOver,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState> OnStateChanged;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Handle the initial scene already active when game is launched
        InitializeForScene(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeForScene(scene);
    }

    private void InitializeForScene(Scene scene)
    {
        if (scene.name == "Gameplay")
        {
            SetState(GameState.Gameplay);
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetState();
                ScoreManager.Instance.StartTimer();
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicForLevel(1);
            }
        }
        else if (scene.name == "MainMenu")
        {
            SetState(GameState.MainMenu);
        }
        else if (scene.name == "GameOver")
        {
            SetState(GameState.GameOver);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver();
            }
        }
        else
        {
            SetState(GameState.MainMenu);
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        // Adjust time scale if paused
        if (newState == GameState.Paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void EndGame()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Gameplay)
        {
            SetState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetState(GameState.Gameplay);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

