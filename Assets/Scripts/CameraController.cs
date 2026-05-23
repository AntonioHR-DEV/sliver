using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float cameraLensSize = 10f;

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        cinemachineCamera.Follow = PlayerController.Instance.transform;
        cinemachineCamera.Lens.OrthographicSize = cameraLensSize;

        PlayerController.Instance.OnDied      += OnPlayerDied;
        PlayerController.Instance.OnRespawned += OnPlayerRespawned;
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance == null) return;
        PlayerController.Instance.OnDied      -= OnPlayerDied;
        PlayerController.Instance.OnRespawned -= OnPlayerRespawned;
    }

    private void OnPlayerDied(object sender, EventArgs e)
    {
        cinemachineCamera.Follow = null;
    }

    private void OnPlayerRespawned(object sender, EventArgs e)
    {
        cinemachineCamera.Follow = PlayerController.Instance.transform;
    }
}