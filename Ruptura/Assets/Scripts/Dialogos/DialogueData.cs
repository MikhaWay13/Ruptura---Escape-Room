using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}

[Serializable]
public class DialogueLine
{
    [Header("Quem fala")]
    public DialogueSpeaker speaker;

    [Header("Texto")]
    [TextArea(2, 5)]
    public string text;
}