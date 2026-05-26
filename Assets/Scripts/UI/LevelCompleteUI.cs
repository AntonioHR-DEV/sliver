using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteUI : BasePanel
{
    public static LevelCompleteUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image[] starImageArray;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button levelsButton;

    [Header("Stars")]
    [SerializeField] private Color starActivatedColor = Color.yellow;
    [SerializeField] private Color starDeactivatedColor = Color.grey;

    [Header("Title Blinking")]
    [SerializeField] private Color[] titleColors;
    [SerializeField] private float blinkInterval = .1f;
    private float elapsedBlinkTime = 0f;
    private int titleColorIndex = 0;

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
        EndCheckpoint.Instance.OnActivated += EndCheckpoint_OnActivated;

        nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        retryButton.onClick.AddListener(OnRetryClicked);
        levelsButton.onClick.AddListener(OnLevelsClicked);

        ResetStars();
        SnapHidden();
    }

    private void Update()
    {
        elapsedBlinkTime += Time.unscaledDeltaTime;
        if (elapsedBlinkTime >= blinkInterval)
        {
            elapsedBlinkTime = 0f;
            titleColorIndex = (titleColorIndex + 1) % titleColors.Length;
            titleText.color = titleColors[titleColorIndex];
        }
    }

    private void OnDestroy()
    {
        EndCheckpoint.Instance.OnActivated -= EndCheckpoint_OnActivated;
    }

    private void EndCheckpoint_OnActivated(object sender, EventArgs e)
    {
        UpdateVisual();

        float showDelay = 1f;
        ShowWithDelay(showDelay);
        
        int levelIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SetLevelCompleted(levelIndex, LevelTimer.Instance.GetStarRating());
        }
    }

    private void UpdateVisual()
    {
        statsText.text =
            "Time: " + LevelTimer.Instance.GetFormattedTime() + "\n" +
            "Deaths: " + PlayerController.Instance.DeathCount;

        ResetStars();

        int starCount = LevelTimer.Instance.GetStarRating();
        for (int i = 0; i < starCount; i++)
        {
            starImageArray[i].color = starActivatedColor;
        }
    }

    private void ResetStars()
    {
        foreach (Image starImage in starImageArray)
            starImage.color = starDeactivatedColor;
    }

    private void OnRetryClicked()
    {
        SceneLoader.Instance.ReloadCurrentScene();
    }

    private void OnNextLevelClicked()
    {
        SceneLoader.Instance.LoadNextScene();
    }

    private void OnLevelsClicked()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.Scene.LevelSelect);
    }
}