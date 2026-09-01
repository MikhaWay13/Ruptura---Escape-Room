using UnityEngine;

public class ClockHand : MonoBehaviour
{
    public enum HandType
    {
        Hours,
        Minutes
    }

    [Header("Configuração")]
    [SerializeField] private HandType handType;

    [Header("Visual")]
    [SerializeField] private Transform visual;

    [Header("Rotação")]
    [SerializeField] private float rotationSpeed = 0.35f;
    [SerializeField] private bool invertRotation = false;

    private float currentAngle;
    private Quaternion initialVisualRotation;

    public HandType Type => handType;
    public float CurrentAngle => currentAngle;

    private void Awake()
    {
        currentAngle = 0f;

        if (visual != null)
            initialVisualRotation = visual.localRotation;

        ApplyRotation();
    }

    public void RotateFromMouse(float mouseDeltaX)
    {
        float direction = invertRotation ? -1f : 1f;

        currentAngle += mouseDeltaX * rotationSpeed * direction;
        currentAngle = Mathf.Repeat(currentAngle, 360f);

        ApplyRotation();
    }

    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Repeat(angle, 360f);
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        if (visual == null)
            return;

        visual.localRotation = initialVisualRotation * Quaternion.Euler(0f, 0f, -currentAngle);
    }

    public int GetMinute()
    {
        float angle = Mathf.Repeat(currentAngle, 360f);
        int minute = Mathf.RoundToInt(angle / 6f);

        return minute % 60;
    }

    public float GetHourValue()
    {
        float angle = Mathf.Repeat(currentAngle, 360f);

        return angle / 30f;
    }

    public int GetHour()
    {
        int hour = Mathf.RoundToInt(GetHourValue());

        hour %= 12;

        if (hour == 0)
            hour = 12;

        return hour;
    }
}