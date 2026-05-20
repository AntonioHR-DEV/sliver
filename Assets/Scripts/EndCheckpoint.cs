using System;
using UnityEngine;

public class EndCheckpoint : MonoBehaviour
{
    public static EndCheckpoint Instance { get; private set; }
    private static readonly int PRESS_TRIGGER_HASH = Animator.StringToHash("Press");
    public event EventHandler OnActivated;

    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem confettiParticleSystem;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated && collision.gameObject.TryGetComponent<PlayerController>(out _))
        {
            isActivated = true;
            animator.SetTrigger(PRESS_TRIGGER_HASH);
            confettiParticleSystem?.Play();
            OnActivated?.Invoke(this, EventArgs.Empty);
        }
    }
}
