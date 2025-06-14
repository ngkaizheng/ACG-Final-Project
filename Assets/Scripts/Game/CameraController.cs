using Cinemachine;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public static CameraController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
    }

    public void SetFollowTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
        else
        {
            Debug.LogWarning("Virtual Camera is not assigned.");
        }
    }
}