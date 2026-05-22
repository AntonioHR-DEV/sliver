using UnityEngine;
using Unity.Cinemachine;
using System;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] private float shakeForce = 1f;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        PlayerController.Instance.OnDied += ShakeCamera;
        PlayerController.Instance.OnFellIntoVoid += ShakeCamera;
        StartCheckpoint.Instance.OnActivated += ShakeCamera;
    }

    private void ShakeCamera(object sender, EventArgs e)
    {
        impulseSource?.GenerateImpulse(shakeForce);
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnDied -= ShakeCamera;
            PlayerController.Instance.OnFellIntoVoid -= ShakeCamera;
        }
        StartCheckpoint.Instance.OnActivated -= ShakeCamera;
    }
}