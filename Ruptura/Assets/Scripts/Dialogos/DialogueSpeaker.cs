using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "New Dialogue Speaker", menuName = "Dialogue/Speaker")]
public class DialogueSpeaker : ScriptableObject
{
    [Header("Informações")]
    public string speakerName;

    [Header("Áudio")]
    public EventReference voice;

    [Header("Visual")]
    public Color nameColor = Color.white;
}