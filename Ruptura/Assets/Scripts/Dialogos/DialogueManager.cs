using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText;

    [Header("Texto")]
    [SerializeField] private float letterSpeed = 0.04f;

    [Header("Teste")]
    [SerializeField] private DialogueData testDialogue;

    private Coroutine typingCoroutine;

    private DialogueData currentDialogue;
    private int currentLineIndex;

    private string currentText;

    private bool isTyping;
    private bool waitingInput;

    public bool IsDialogueOpen => dialoguePanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        dialoguePanel.SetActive(false);
    }

    private void Start()
    {
        // Serve apenas para testar o sistema.
        // Depois você pode desativar/remover essa parte.
        if (testDialogue != null)
        {
            StartDialogue(testDialogue);
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("DialogueManager: Nenhum DialogueData foi fornecido.");
            return;
        }

        if (dialogue.lines == null || dialogue.lines.Count == 0)
        {
            Debug.LogWarning("DialogueManager: O DialogueData não possui falas.");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;

        dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null)
            return;

        if (currentLineIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogue.lines[currentLineIndex];

        if (line.speaker == null)
        {
            Debug.LogWarning("DialogueManager: Uma fala não possui Speaker.");
            return;
        }

        currentText = line.text;

        speakerNameText.text = line.speaker.speakerName;
        speakerNameText.color = line.speaker.nameColor;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(line.speaker.voice));
    }

    private IEnumerator TypeText(EventReference voice)
    {
        dialogueText.text = "";

        isTyping = true;
        waitingInput = false;

        foreach (char letter in currentText)
        {
            dialogueText.text += letter;

            if (char.IsLetterOrDigit(letter))
            {
                RuntimeManager.PlayOneShot(voice);
            }

            yield return new WaitForSeconds(letterSpeed);
        }

        isTyping = false;
        waitingInput = true;
    }

    public void ContinueDialogue()
    {
        if (!IsDialogueOpen)
            return;

        // Se ainda está escrevendo,
        // completa a frase.
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialogueText.text = currentText;

            isTyping = false;
            waitingInput = true;

            return;
        }

        // Se terminou a frase,
        // passa para a próxima.
        if (waitingInput)
        {
            currentLineIndex++;

            ShowCurrentLine();
        }
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        dialoguePanel.SetActive(false);

        dialogueText.text = "";
        speakerNameText.text = "";

        currentDialogue = null;

        currentLineIndex = 0;

        isTyping = false;
        waitingInput = false;
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