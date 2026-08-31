/*using UnityEngine;

public class ClockInteraction : MonoBehaviour, IRaycastInteractable
{
    [Header("Sistema do Relógio")]
    [SerializeField] private ClockPuzzle clockPuzzle;

    public void Interact()
    {
        if (clockPuzzle == null)
        {
            Debug.LogWarning(
                "ClockInteraction: ClockPuzzle não foi configurado."
            );

            return;
        }

        clockPuzzle.OpenClock();
    }
}*/