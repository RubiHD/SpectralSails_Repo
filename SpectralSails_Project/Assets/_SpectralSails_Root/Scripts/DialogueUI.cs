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

    [Header("Sistema de Opciones")]
    public GameObject choicesPanel;
    public Button choiceButtonPrefab;

    [Header("Referencia al Jugador")]
    public PlayerController player;

    private DialogueDataSO currentDialogue;
    private DialogueLine[] lines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private ShipBuilderNPC currentNPC;

    private void Start()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(AdvanceDialogue);
        panel.SetActive(false);

        if (choicesPanel != null)
            choicesPanel.SetActive(false);
    }

    public void ShowDialogue(DialogueDataSO dialogueData)
    {
        ShowDialogue(dialogueData, null);
    }

    public void ShowDialogue(DialogueDataSO dialogueData, ShipBuilderNPC npc)
    {
        if (dialogueData == null || dialogueData.lines == null || dialogueData.lines.Length == 0)
        {
            Debug.LogWarning("No hay líneas de diálogo para mostrar.");
            return;
        }

        currentDialogue = dialogueData;
        currentNPC = npc;
        lines = dialogueData.lines;
        currentLineIndex = 0;

        panel.SetActive(true);

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        if (player != null)
            player.canMove = false;

        ShowCurrentLine();
    }

    public void AdvanceDialogue()
    {
        if (lines == null || lines.Length == 0)
            return;

        if (isTyping)
        {
            if (typingCoroutine != null)
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
            if (currentDialogue != null && currentDialogue.hasChoices && currentDialogue.choices != null && currentDialogue.choices.Length > 0)
            {
                ShowChoices();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void ShowCurrentLine()
    {
        if (lines == null || currentLineIndex >= lines.Length)
            return;

        DialogueLine line = lines[currentLineIndex];

        if (nameText != null)
            nameText.text = line.speakerName;

        if (portraitImage != null)
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

    private void ShowChoices()
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(true);

            // Limpiar botones anteriores
            foreach (Transform child in choicesPanel.transform)
            {
                Destroy(child.gameObject);
            }

            // Crear botones para cada opción
            if (currentDialogue != null && currentDialogue.choices != null)
            {
                foreach (DialogueChoice choice in currentDialogue.choices)
                {
                    if (choiceButtonPrefab != null)
                    {
                        Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                        TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();

                        if (buttonText != null)
                            buttonText.text = choice.choiceText;

                        DialogueChoice capturedChoice = choice;
                        choiceButton.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
                    }
                }
            }
        }
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        Debug.Log($"Opción seleccionada: {choice.choiceText}");

        if (choice.checkRequirements && currentNPC != null && player != null)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();

            if (inventory != null && currentNPC.HasRequirements(inventory))
            {
                currentNPC.CompleteQuest(inventory);

                if (choice.nextDialogue != null)
                {
                    ShowDialogue(choice.nextDialogue, currentNPC);
                }
                else
                {
                    EndDialogue();
                }
            }
            else
            {
                if (currentNPC.missingRequirementsDialogue != null)
                {
                    ShowDialogue(currentNPC.missingRequirementsDialogue, currentNPC);
                }
                else
                {
                    EndDialogue();
                }
            }
        }
        else if (choice.nextDialogue != null)
        {
            ShowDialogue(choice.nextDialogue, currentNPC);
        }
        else if (choice.endDialogue)
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        if (panel != null)
            panel.SetActive(false);

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        lines = null;
        currentLineIndex = 0;
        currentDialogue = null;
        currentNPC = null;

        if (player != null)
            player.canMove = true;
    }
}