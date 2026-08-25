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

    [SerializeField] private Transform visual;

    [Header("Rotação")]
    [SerializeField] private float degreesPerStep = 1f;

    private float currentAngle;

    public HandType Type => handType;
    public float CurrentAngle => currentAngle;

    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Repeat(angle, 360f);

        if (visual != null)
        {
            visual.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -currentAngle
                );
        }
    }

    public void Rotate(float amount)
    {
        SetAngle(currentAngle + amount);
    }

    public int GetHour()
    {
        float normalized = Mathf.Repeat(currentAngle, 360f);

        int hour =
            Mathf.RoundToInt(normalized / 30f);

        hour %= 12;

        if (hour == 0)
            hour = 12;

        return hour;
    }

    public int GetMinute()
    {
        float normalized = Mathf.Repeat(currentAngle, 360f);

        int minute =
            Mathf.RoundToInt(normalized / 6f);

        return minute % 60;
    }
}