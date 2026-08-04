using System.Collections;
using TMPro;
using UnityEngine;
using FMODUnity;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Texto")]
    public float letterSpeed = 0.04f;

    [Header("FMOD")]
    [SerializeField] private EventReference narratorVoice;
    [SerializeField] private EventReference playerVoice;

    private EventReference currentVoice;

    private Coroutine typingCoroutine;

    private string currentText;

    private bool isTyping;
    private bool waitingInput;

    public bool IsDialogueOpen => dialoguePanel.activeSelf;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    // narrator = true -> voz do narrador
    // narrator = false -> voz do player
    public void ShowDialogue(string text, bool narrator)
    {
        currentText = text;

        currentVoice = narrator ? narratorVoice : playerVoice;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        dialoguePanel.SetActive(true);

        dialogueText.text = "";

        isTyping = true;
        waitingInput = false;

        foreach (char letter in currentText)
        {
            dialogueText.text += letter;

            if (char.IsLetterOrDigit(letter))
            {
                RuntimeManager.PlayOneShot(currentVoice);
            }

            yield return new WaitForSeconds(letterSpeed);
        }

        isTyping = false;
        waitingInput = true;
    }

    public void ContinueDialogue()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueText.text = currentText;

            isTyping = false;
            waitingInput = true;

            return;
        }

        if (waitingInput)
        {
            dialoguePanel.SetActive(false);
            waitingInput = false;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (IsDialogueOpen)
        {
            ContinueDialogue();
        }
    }
}