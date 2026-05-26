using System;
using UnityEngine;

/// <summary>
/// Tracks elapsed time from StartCheckpoint activation to EndCheckpoint activation.
/// Calculates star rating based on configurable gold and silver time thresholds.
/// </summary>
public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance { get; private set; }

    public event EventHandler OnTimerStarted;
    public event EventHandler OnTimerStopped;

    [Header("Star Thresholds (seconds)")]
    [Tooltip("Complete within this time to earn 3 stars.")]
    [SerializeField] private float goldTime = 20f;
    [Tooltip("Complete within this time to earn 2 stars.")]
    [SerializeField] private float silverTime = 30f;

    private float elapsedTime;
    private bool isRunning;

    public float ElapsedTime => elapsedTime;
    public bool IsRunning => isRunning;

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
        StartCheckpoint.Instance.OnActivated += StartCheckpoint_OnActivated;
        EndCheckpoint.Instance.OnActivated += EndCheckpoint_OnActivated;
    }

    private void OnDestroy()
    {
        StartCheckpoint.Instance.OnActivated -= StartCheckpoint_OnActivated;
        EndCheckpoint.Instance.OnActivated -= EndCheckpoint_OnActivated;
    }

    private void Update()
    {
        if (!isRunning) return;
        elapsedTime += Time.deltaTime;
    }

    private void StartCheckpoint_OnActivated(object sender, EventArgs e)
    {
        elapsedTime = 0f;
        isRunning = true;
        OnTimerStarted?.Invoke(this, EventArgs.Empty);
    }

    private void EndCheckpoint_OnActivated(object sender, EventArgs e)
    {
        isRunning = false;
        OnTimerStopped?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Returns 1, 2, or 3 stars based on elapsed time vs thresholds.
    /// </summary>
    public int GetStarRating()
    {
        if (elapsedTime <= goldTime) return 3;
        if (elapsedTime <= silverTime) return 2;
        return 1;
    }

    /// <summary>
    /// Returns elapsed time formatted as MM:SS.mm
    /// </summary>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int centiseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{centiseconds:00}";
    }
}