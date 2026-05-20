using UnityEngine;

public class StartCheckpointSound : MonoBehaviour
{
    private void Start()
    {
        StartCheckpoint.Instance.OnActivated += StartCheckpoint_OnActivated;
    }

    private void OnDestroy()
    {
        StartCheckpoint.Instance.OnActivated -= StartCheckpoint_OnActivated;
    }

    private void StartCheckpoint_OnActivated(object sender, System.EventArgs e)
    {
        SoundManager.Instance?.PlayStart();
    }
}