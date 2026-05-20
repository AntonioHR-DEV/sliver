using System;
using UnityEngine;

public class StartCheckpoint : MonoBehaviour
{
    public static StartCheckpoint Instance { get; private set; }

    private static readonly int MOVE_TRIGGER_HASH = Animator.StringToHash("Move");

    public event EventHandler OnActivated;

    [SerializeField] private Animator animator;
    private bool isActivated = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActivated && collision.gameObject.TryGetComponent<PlayerController>(out _))
        {
            isActivated = true;
            animator.SetTrigger(MOVE_TRIGGER_HASH);
            OnActivated?.Invoke(this, EventArgs.Empty);
        }
    }
}
