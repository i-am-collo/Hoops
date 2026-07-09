using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Events for UI and gameplay updates
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action<int> OnLevelChanged;
    public event Action<float> OnTimerChanged;
    public event Action OnComboTriggered; // Fired when 3, 6, 9 etc. combo is hit
    public event Action OnLevelUpTriggered; // Fired when level increases

    [Header("Level Configs")]
    public LevelConfig[] levelConfigs;

    [Header("Settings")]
    public float startingTimer = 60f;
    public float maxTimer = 90f;
    public int pointsPerLevel = 5;
    public int comboThreshold = 3;
    public float comboTimerReward = 3f;

    // Runtime state
    public int CurrentScore { get; private set; }
    public int CurrentCombo { get; private set; }
    public int HighestCombo { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentTime { get; private set; }
    public bool IsTimerRunning { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetState();
    }

    private void Update()
    {
        if (IsTimerRunning)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0f)
            {
                CurrentTime = 0f;
                IsTimerRunning = false;
                GameManager.Instance.EndGame();
            }
            OnTimerChanged?.Invoke(CurrentTime);
        }
    }

    public void ResetState()
    {
        CurrentScore = 0;
        CurrentCombo = 0;
        HighestCombo = 0;
        CurrentLevel = 1;
        CurrentTime = startingTimer;
        IsTimerRunning = false;

        OnScoreChanged?.Invoke(CurrentScore);
        OnComboChanged?.Invoke(CurrentCombo);
        OnLevelChanged?.Invoke(CurrentLevel);
        OnTimerChanged?.Invoke(CurrentTime);
    }

    public void StartTimer()
    {
        IsTimerRunning = true;
    }

    public void StopTimer()
    {
        IsTimerRunning = false;
    }

    public void AddScore()
    {
        CurrentScore++;
        CurrentCombo++;

        if (CurrentCombo > HighestCombo)
        {
            HighestCombo = CurrentCombo;
        }

        OnScoreChanged?.Invoke(CurrentScore);
        OnComboChanged?.Invoke(CurrentCombo);

        // Check Combo Reward
        if (CurrentCombo > 0 && CurrentCombo % comboThreshold == 0)
        {
            CurrentTime = Mathf.Min(CurrentTime + comboTimerReward, maxTimer);
            OnComboTriggered?.Invoke();
        }

        // Check Level Progression
        int targetLevel = 1 + (CurrentScore / pointsPerLevel);
        if (targetLevel > CurrentLevel)
        {
            CurrentLevel = targetLevel;
            OnLevelChanged?.Invoke(CurrentLevel);
            OnLevelUpTriggered?.Invoke();
        }
    }

    public void ResetCombo()
    {
        if (CurrentCombo > 0)
        {
            CurrentCombo = 0;
            OnComboChanged?.Invoke(CurrentCombo);
        }
    }

    public LevelConfig GetCurrentLevelConfig()
    {
        if (levelConfigs == null || levelConfigs.Length == 0) return null;
        int index = Mathf.Clamp(CurrentLevel - 1, 0, levelConfigs.Length - 1);
        return levelConfigs[index];
    }
}
