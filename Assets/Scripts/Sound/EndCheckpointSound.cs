using UnityEngine;

public class EndCheckpointSound : MonoBehaviour
{
    private void Start()
    {
        EndCheckpoint.Instance.OnActivated += EndCheckpoint_OnActivated;
    }

    private void OnDestroy()
    {
        EndCheckpoint.Instance.OnActivated -= EndCheckpoint_OnActivated;
    }

    private void EndCheckpoint_OnActivated(object sender, System.EventArgs e)
    {
        SoundManager.Instance?.PlayAchievement();
    }
}