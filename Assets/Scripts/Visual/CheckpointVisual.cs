using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CheckpointVisual : MonoBehaviour
{
    private static readonly int IsActivated = Animator.StringToHash("IsActivated");

    [SerializeField] private Checkpoint checkpoint;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

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
        animator.SetBool(IsActivated, true);
    }
}
