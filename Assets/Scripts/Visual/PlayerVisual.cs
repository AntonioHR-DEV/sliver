using UnityEngine;
using System;

/// <summary>
/// Reads PlayerController's public state every frame and drives the Animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerVisual : MonoBehaviour
{
    private static readonly int STATE_PARAM_HASH = Animator.StringToHash("State");
    private static readonly int APPEARING_STATE_HASH = Animator.StringToHash("Appearing");
    private static readonly int DISAPPEARING_STATE_HASH = Animator.StringToHash("Disappearing");

    [SerializeField] private ParticleSystem dustParticleSystem;
    [SerializeField] private int runDustEmissionRate = 20;
    [SerializeField] private int jumpDustEmissionRate = 20;
    private Animator animator;

    public bool IsLocked { get; set; } = false;

    private void Awake()
    {
        animator = GetComponent<Animator>(); //
    }

    private void Start()
    {
        if (PlayerController.Instance == null) return; //

        // Explicitly subscribe using named listener methods to avoid memory leaks
        PlayerController.Instance.OnRespawned += PlayerController_OnRespawned; //
        PlayerController.Instance.OnJumped += PlayerController_OnDustBurstEvent;
        PlayerController.Instance.OnWallJumped += PlayerController_OnDustBurstEvent;
        PlayerController.Instance.OnDoubleJumped += PlayerController_OnDustBurstEvent; // Fixed: Now handles double jumps!
        PlayerController.Instance.OnLanded += PlayerController_OnDustBurstEvent;
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance == null) return; //

        // Clean up precisely using identical references
        PlayerController.Instance.OnRespawned -= PlayerController_OnRespawned; //
        PlayerController.Instance.OnJumped -= PlayerController_OnDustBurstEvent;
        PlayerController.Instance.OnWallJumped -= PlayerController_OnDustBurstEvent;
        PlayerController.Instance.OnDoubleJumped -= PlayerController_OnDustBurstEvent;
        PlayerController.Instance.OnLanded -= PlayerController_OnDustBurstEvent;
    }

    private void Update()
    {
        if (PlayerController.Instance == null)
        {
            animator.SetInteger(STATE_PARAM_HASH, (int)PlayerController.PlayerState.Idle);
            return;
        }

        if (!IsLocked && animator.GetInteger(STATE_PARAM_HASH) != (int)PlayerController.Instance.CurrentPlayerState)
        {
            animator.SetInteger(STATE_PARAM_HASH, (int)PlayerController.Instance.CurrentPlayerState);
        }

        AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (currentStateInfo.shortNameHash == APPEARING_STATE_HASH && currentStateInfo.normalizedTime >= 1f)
        {
            PlayerController.Instance.CurrentPlayerState = PlayerController.PlayerState.Falling;
        }

        if (currentStateInfo.shortNameHash == DISAPPEARING_STATE_HASH && currentStateInfo.normalizedTime >= 1f)
        {
            PlayerController.Instance.gameObject.SetActive(false);
        }

        // Manage continuous running dust trails
        var emission = dustParticleSystem.emission;
        if (PlayerController.Instance.CurrentPlayerState == PlayerController.PlayerState.Running)
        {
            emission.rateOverTime = runDustEmissionRate;
        }
        else
        {
            emission.rateOverTime = 0;
        }
    }

    // =========================================================================
    // Event Handlers
    // =========================================================================

    private void PlayerController_OnDustBurstEvent(object sender, EventArgs e)
    {
        if (dustParticleSystem != null)
        {
            dustParticleSystem.Emit(jumpDustEmissionRate);
        }
    }

    private void PlayerController_OnRespawned(object sender, EventArgs e)
    {
        // Force back to Idle so the animator doesn't resume mid-death-clip
        animator.SetInteger(STATE_PARAM_HASH, (int)PlayerController.PlayerState.Idle); //
    }
}