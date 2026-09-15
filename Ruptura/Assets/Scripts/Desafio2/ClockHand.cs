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
    [SerializeField] private bool invertRotation;

    [Header("Feedback de seleção")]
    [SerializeField] private Color selectedOutlineColor = Color.yellow;
    [SerializeField, Range(0f, 10f)] private float selectedOutlineWidth = 4f;

    private Quaternion initialLocalRotation;
    private Outline selectionOutline;

    public HandType Type => handType;

    private void Awake()
    {
        if (visual != null)
            initialLocalRotation = visual.localRotation;

        selectionOutline = GetComponent<Outline>();

        if (selectionOutline != null)
            selectionOutline.enabled = false;
    }

    private void OnDisable()
    {
        SetSelected(false);
    }

    public void ResetRotation()
    {
        if (visual != null)
            visual.localRotation = initialLocalRotation;
    }

    public void Rotate(float rotationAmount)
    {
        if (visual == null)
            return;

        if (invertRotation)
            rotationAmount = -rotationAmount;

        visual.Rotate(Vector3.forward, rotationAmount, Space.Self);
    }

    public void SetSelected(bool selected)
    {
        if (!selected)
        {
            if (selectionOutline != null)
                selectionOutline.enabled = false;

            return;
        }

        if (selectionOutline == null)
            selectionOutline = gameObject.AddComponent<Outline>();

        selectionOutline.OutlineMode = Outline.Mode.OutlineVisible;
        selectionOutline.OutlineColor = selectedOutlineColor;
        selectionOutline.OutlineWidth = selectedOutlineWidth;
        selectionOutline.enabled = true;
    }

    public bool IsAlignedWith(Transform reference, float tolerance)
    {
        if (visual == null || reference == null)
            return false;

        return Quaternion.Angle(visual.rotation, reference.rotation) <= tolerance;
    }
}
