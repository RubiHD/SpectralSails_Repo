using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public Button nextButton;

    public PlayerController player;

    private DialogueLine[] lines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    public void ShowDialogue(DialogueDataSO dialogueData)
    {
        if (dialogueData == null || dialogueData.lines == null || dialogueData.lines.Length == 0)
        {
            Debug.LogWarning("No hay líneas de diálogo para mostrar.");
            return;
        }

        lines = dialogueData.lines;
        currentLineIndex = 0;
        panel.SetActive(true);
        player.canMove = false;

        ShowCurrentLine();
    }

    public void AdvanceDialogue()
    {
        if (lines == null || lines.Length == 0)
            return;

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = lines[currentLineIndex].text;
            isTyping = false;
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = lines[currentLineIndex];

        nameText.text = line.speakerName;
        portraitImage.sprite = line.speakerPortrait;

        StartTypingLine(line.text);
    }

    private void StartTypingLine(string line)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        panel.SetActive(false);
        lines = null;
        currentLineIndex = 0;
        player.canMove = true;
    }

    private void Start()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(AdvanceDialogue);
        panel.SetActive(false);
    }
}