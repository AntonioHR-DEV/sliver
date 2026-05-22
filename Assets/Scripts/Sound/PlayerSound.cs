using System;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private void Start()
    {
        if (PlayerController.Instance == null) return;

        PlayerController.Instance.OnDied += Player_OnDied;
        PlayerController.Instance.OnFellIntoVoid += Player_OnFellIntoVoid;
        PlayerController.Instance.OnRespawned += Player_OnRespawned;
        PlayerController.Instance.OnJumped += Player_OnJumped;
        PlayerController.Instance.OnWallJumped += Player_OnWallJumped;
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance == null) return;

        PlayerController.Instance.OnDied -= Player_OnDied;
        PlayerController.Instance.OnFellIntoVoid -= Player_OnFellIntoVoid;
        PlayerController.Instance.OnRespawned -= Player_OnRespawned;
        PlayerController.Instance.OnJumped -= Player_OnJumped;
        PlayerController.Instance.OnWallJumped -= Player_OnWallJumped;
    }

    private void Player_OnDied(object sender, EventArgs e)
    {
        SoundManager.Instance?.PlayHurt();
    }

    private void Player_OnFellIntoVoid(object sender, EventArgs e)
    {
        SoundManager.Instance?.PlayHurt();
    }

    private void Player_OnRespawned(object sender, EventArgs e)
    {
        SoundManager.Instance?.PlayRespawn();
    }

    private void Player_OnJumped(object sender, EventArgs e)
    {
        SoundManager.Instance?.PlayJumpNormal();
    }

    private void Player_OnWallJumped(object sender, EventArgs e)
    {
        SoundManager.Instance?.PlayJumpWall();
    }
}
