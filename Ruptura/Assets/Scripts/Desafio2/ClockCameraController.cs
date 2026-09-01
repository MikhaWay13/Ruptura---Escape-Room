using UnityEngine;


public class ClockCameraController : MonoBehaviour
{
    [Header("Câmeras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera clockCamera;


    public bool IsActive { get; private set; }


    private void Awake()
    {
        if (clockCamera != null)
            clockCamera.enabled = false;
    }


    public void EnterClockView()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;


        if (playerCamera == null || clockCamera == null)
        {
            Debug.LogWarning(
                "ClockCameraController: câmeras não configuradas."
            );


            return;
        }


        playerCamera.enabled = false;
        clockCamera.enabled = true;


        IsActive = true;
    }


    public void ExitClockView()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;


        if (clockCamera != null)
            clockCamera.enabled = false;


        if (playerCamera != null)
            playerCamera.enabled = true;


        IsActive = false;
    }
}
