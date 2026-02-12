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
    public GameObject choicesPanel; // Panel que contiene los botones de elección
    public Button choiceButtonPrefab; // Prefab del botón de elección

    [Header("Referencia al Jugador")]
    public PlayerController player;

    private DialogueDataSO currentDialogue;
    private DialogueLine[] lines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    // ? NUEVO: Referencia al NPC que inició el diálogo
    private ShipBuilderNPC currentNPC;

    private void Start()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(AdvanceDialogue);
        panel.SetActive(false);
        choicesPanel.SetActive(false);
    }

    public void ShowDialogue(DialogueDataSO dialogueData, ShipBuilderNPC npc = null)
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
        choicesPanel.SetActive(false);
        player.canMove = false;

        ShowCurrentLine();
    }

    public void AdvanceDialogue()
    {
        if (lines == null || lines.Length == 0)
            return;

        // Si está escribiendo, mostrar texto completo
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
            // ? NUEVO: Verificar si hay opciones al final del diálogo
            if (currentDialogue.hasChoices && currentDialogue.choices.Length > 0)
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

    // ? NUEVO: Mostrar opciones de respuesta
    private void ShowChoices()
    {
        // Ocultar botón de "siguiente"
        nextButton.gameObject.SetActive(false);

        // Mostrar panel de opciones
        choicesPanel.SetActive(true);

        // Limpiar botones anteriores
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Crear botones para cada opción
        foreach (DialogueChoice choice in currentDialogue.choices)
        {
            Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;

            // Agregar listener al botón
            choiceButton.onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    // ? NUEVO: Manejar selección de opción
    private void OnChoiceSelected(DialogueChoice choice)
    {
        Debug.Log($"Opción seleccionada: {choice.choiceText}");

        // Verificar si debe comprobar requisitos (el jugador dijo "Sí")
        if (choice.checkRequirements && currentNPC != null)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();

            if (currentNPC.HasRequirements(inventory))
            {
                // ? TIENE TODO ? Mostrar diálogo final y construir barco
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
                // ? NO TIENE TODO ? Mostrar diálogo de falta de requisitos
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
            // Ir al siguiente diálogo
            ShowDialogue(choice.nextDialogue, currentNPC);
        }
        else if (choice.endDialogue)
        {
            // Terminar diálogo
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        panel.SetActive(false);
        choicesPanel.SetActive(false);
        nextButton.gameObject.SetActive(true);

        lines = null;
        currentLineIndex = 0;
        currentDialogue = null;
        currentNPC = null;

        player.canMove = true;
    }
}