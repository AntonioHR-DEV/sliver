using System;
using UnityEngine;

public class CheckpointSound : MonoBehaviour
{
    [SerializeField] private Checkpoint checkpoint;

    private void Start()
    {
        checkpoint.OnActivated += Checkpoint_OnActivated;
    }

    private void OnDestroy()
    {
        checkpoint.OnActivated -= Checkpoint_OnActivated;
    }

    private void Checkpoint_OnActivated(object sender, EventArgs e)
    {
        SoundManager.Instance?.PlayCheckpoint();
    }
}
