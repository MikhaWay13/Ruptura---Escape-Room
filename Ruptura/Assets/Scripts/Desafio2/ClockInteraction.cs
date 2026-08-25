using UnityEngine;

public class ClockInteraction : MonoBehaviour, IRaycastInteractable
{
    [SerializeField] private ClockPuzzle clockPuzzle;

    public void Interact()
    {
        Debug.Log("CLOCK INTERACTION FUNCIONOU!");

        if (clockPuzzle != null)
        {
            clockPuzzle.OpenClock();
        }
    }
}