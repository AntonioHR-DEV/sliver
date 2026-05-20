using UnityEngine;
using System;

/// <summary>
/// Reads PlayerController's public state every frame and drives the Animator.
/// No logic lives here — this is purely a translator between controller state
/// and animation state.
///
/// Requires:  Animator component with an int parameter named "State"
///
/// Animator "State" parameter values match PlayerController.PlayerState enum:
///   0 = Appearing
///   1 = Idle
///   2 = Running
///   3 = Jumping
///   4 = DoubleJumping
///   5 = Falling
///   6 = WallSliding
///   7 = Dead
///   8 = Disappearing
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerVisual : MonoBehaviour
{
    private static readonly int STATE_PARAM_HASH = Animator.StringToHash("State");
    private static readonly int APPEARING_STATE_HASH = Animator.StringToHash("Appearing");
    private static readonly int DISAPPEARING_STATE_HASH = Animator.StringToHash("Disappearing");

    private Animator animator;

    public bool IsLocked { get; set; } = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (PlayerController.Instance == null) return;

        PlayerController.Instance.OnRespawned += PlayerController_OnRespawned;
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
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance == null) return;

        PlayerController.Instance.OnRespawned -= PlayerController_OnRespawned;
    }

    private void PlayerController_OnRespawned(object sender, EventArgs e)
    {
        // Force back to Idle so the animator doesn't resume mid-death-clip
        animator.SetInteger(STATE_PARAM_HASH, (int)PlayerController.PlayerState.Idle);
    }
}