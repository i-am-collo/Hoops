using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI comboText;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject gameOverPanel;
    public GameObject pausedPanel;

    [Header("GameOver Details")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highestComboText;
    public TextMeshProUGUI levelReachedText;

    [Header("Buttons")]
    public UnityEngine.UI.Button playButton;
    public UnityEngine.UI.Button pauseButton;
    public UnityEngine.UI.Button resumeButton;
    public UnityEngine.UI.Button restartButton;
    public UnityEngine.UI.Button mainMenuButton;
    public UnityEngine.UI.Button gameOverRestartButton;
    public UnityEngine.UI.Button gameOverMenuButton;

    [Header("Overlays & Effects")]
    public GameObject comboOverlay;
    public GameObject levelUpOverlay;
    public ParticleSystem confettiEffect;

    [Header("Court Background")]
    public SpriteRenderer courtBackgroundRenderer; // Fix: Dynamically transition the court background

    [Header("Main Menu Additions")]
    public UnityEngine.UI.Button instructionsButton;
    public UnityEngine.UI.Button settingsButton;
    public UnityEngine.UI.Button exitButton;
    public GameObject instructionsPopup;
    public GameObject settingsPopup;
    public UnityEngine.UI.Button instructionsCloseButton;
    public UnityEngine.UI.Button settingsCloseButton;
    public UnityEngine.UI.Slider musicVolumeSlider;
    public UnityEngine.UI.Slider sfxVolumeSlider;

    private void Awake()
    {
        // Scene-local instance registration
        Instance = this;
    }

    private void Start()
    {
        // Add button listeners
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnRestartClicked);
        if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Add Main Menu Button listeners
        if (instructionsButton != null) instructionsButton.onClick.AddListener(OnInstructionsClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);
        if (instructionsCloseButton != null) instructionsCloseButton.onClick.AddListener(OnInstructionsCloseClicked);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(OnSettingsCloseClicked);

        // Add volume slider listeners
        if (musicVolumeSlider != null)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.musicSource.volume;
            }
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.sfxSource.volume;
            }
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        // Ensure popups are closed initially
        if (instructionsPopup != null) instructionsPopup.SetActive(false);
        if (settingsPopup != null) settingsPopup.SetActive(false);

        // Bind ScoreManager events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
            ScoreManager.Instance.OnComboChanged += UpdateComboUI;
            ScoreManager.Instance.OnLevelChanged += UpdateLevelUI;
            ScoreManager.Instance.OnTimerChanged += UpdateTimerUI;
            ScoreManager.Instance.OnComboTriggered += TriggerComboFeedback;
            ScoreManager.Instance.OnLevelUpTriggered += TriggerLevelUpFeedback;

            // Initialize display values
            UpdateScoreUI(ScoreManager.Instance.CurrentScore);
            UpdateComboUI(ScoreManager.Instance.CurrentCombo);
            UpdateLevelUI(ScoreManager.Instance.CurrentLevel);
            UpdateTimerUI(ScoreManager.Instance.CurrentTime);
        }

        // Bind GameManager state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += UpdatePanelVisibility;
            UpdatePanelVisibility(GameManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
            ScoreManager.Instance.OnComboChanged -= UpdateComboUI;
            ScoreManager.Instance.OnLevelChanged -= UpdateLevelUI;
            ScoreManager.Instance.OnTimerChanged -= UpdateTimerUI;
            ScoreManager.Instance.OnComboTriggered -= TriggerComboFeedback;
            ScoreManager.Instance.OnLevelUpTriggered -= TriggerLevelUpFeedback;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= UpdatePanelVisibility;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void UpdatePanelVisibility(GameState state)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(state == GameState.MainMenu);
        if (gameplayPanel != null) gameplayPanel.SetActive(state == GameState.Gameplay || state == GameState.Paused);
        if (pausedPanel != null) pausedPanel.SetActive(state == GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);

        if (state == GameState.GameOver)
        {
            PopulateGameOverDetails();
        }
    }

    private void PopulateGameOverDetails()
    {
        if (ScoreManager.Instance != null)
        {
            if (finalScoreText != null) finalScoreText.text = "Final Score: " + ScoreManager.Instance.CurrentScore;
            if (highestComboText != null) highestComboText.text = "Highest Combo: " + ScoreManager.Instance.HighestCombo;
            if (levelReachedText != null) levelReachedText.text = "Level Reached: " + ScoreManager.Instance.CurrentLevel;
        }
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
            scoreText.color = Color.white;
        }
    }

    private void UpdateComboUI(int combo)
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = "Combo x" + combo;
                comboText.color = Color.white;
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level;
            levelText.color = Color.white;
        }

        // Fix: Dynamically update the background sprite Renderer to match the level's config background
        if (ScoreManager.Instance != null && courtBackgroundRenderer != null)
        {
            LevelConfig config = ScoreManager.Instance.GetCurrentLevelConfig();
            if (config != null && config.backgroundSprite != null)
            {
                courtBackgroundRenderer.sprite = config.backgroundSprite;
            }
        }
    }

    private void UpdateTimerUI(float timer)
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(timer) + "s";
            if (timer < 10f)
            {
                timerText.color = Color.white;
                // Add minor pulsing scale effect
                float pulse = 1f + Mathf.PingPong(Time.time * 2f, 0.15f);
                timerText.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else
            {
                timerText.color = Color.white;
                timerText.transform.localScale = Vector3.one;
            }
        }
    }

    private void TriggerComboFeedback()
    {
        if (comboOverlay != null)
        {
            comboOverlay.SetActive(true);
            CancelInvoke(nameof(HideComboOverlay));
            Invoke(nameof(HideComboOverlay), 1.5f);
        }

        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayComboFanfare();

            // Fix: Trigger the Crowd Cheer sound alongside combo fanfare to eliminate dead asset overhead
            if (AudioManager.Instance.crowdCheerClip != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.crowdCheerClip);
            }
        }
    }

    private void HideComboOverlay()
    {
        if (comboOverlay != null) comboOverlay.SetActive(false);
    }

    private void TriggerLevelUpFeedback()
    {
        if (levelUpOverlay != null)
        {
            levelUpOverlay.SetActive(true);
            CancelInvoke(nameof(HideLevelUpOverlay));
            Invoke(nameof(HideLevelUpOverlay), 2f);
        }

        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelUp();
        }
    }

    private void HideLevelUpOverlay()
    {
        if (levelUpOverlay != null) levelUpOverlay.SetActive(false);
    }

    // Button Callbacks
    private void OnPlayClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    private void OnPauseClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.PauseGame();
    }

    private void OnResumeClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    private void OnRestartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
    }

    private void OnInstructionsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (instructionsPopup != null) instructionsPopup.SetActive(true);
    }

    private void OnInstructionsCloseClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (instructionsPopup != null) instructionsPopup.SetActive(false);
    }

    private void OnSettingsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null && AudioManager.Instance.musicSource != null)
                musicVolumeSlider.value = AudioManager.Instance.musicSource.volume;
            if (sfxVolumeSlider != null && AudioManager.Instance.sfxSource != null)
                sfxVolumeSlider.value = AudioManager.Instance.sfxSource.volume;
        }
        if (settingsPopup != null) settingsPopup.SetActive(true);
    }

    private void OnSettingsCloseClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (settingsPopup != null) settingsPopup.SetActive(false);
    }

    private void OnExitClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        Debug.Log("Exit Button Clicked - Quitting application.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void OnMusicVolumeChanged(float val)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
        {
            AudioManager.Instance.musicSource.volume = val;
        }
    }

    private void OnSfxVolumeChanged(float val)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
        {
            AudioManager.Instance.sfxSource.volume = val;
        }
    }
}

