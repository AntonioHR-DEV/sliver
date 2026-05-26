using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Requires:  SceneLoader singleton, SaveSystem singleton.
///            A Canvas with a panel RectTransform that slides in from off-screen.
///            GameInput must expose an OnPauseStarted event.
/// </summary>
public class PauseMenu : BasePanel
{
    public static PauseMenu Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button levelsButton;

    private bool isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseStarted += GameInput_OnPauseStarted;

        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        settingsButton.onClick.AddListener(OpenSettings);
        levelsButton.onClick.AddListener(GoToLevelSelect);

        SnapHidden();
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
            GameInput.Instance.OnPauseStarted -= GameInput_OnPauseStarted;
    }

    private void GameInput_OnPauseStarted(object sender, EventArgs e)
    {
        // Don't allow pausing if the level complete screen is showing
        if (LevelCompleteUI.Instance != null && LevelCompleteUI.Instance.IsVisible) return;

        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Show();
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Hide();
    }

    private void Restart()
    {
        SceneLoader.Instance.ReloadCurrentScene();
    }

    private void OpenSettings()
    {
        Hide();
        SettingsUI.Instance.ShowWithDelay(slideDuration, () => 
        {
            if (isPaused) Show();
        });
    }

    private void GoToLevelSelect()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.Scene.LevelSelect);
    }

    public bool IsPaused => isPaused;
}