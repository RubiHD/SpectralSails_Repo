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

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private DialogueDataSO currentDialogue;
    private DialogueLine[] lines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private ShipBuilderNPC currentNPC;

    // ✅ NUEVO: Variable para saber si el diálogo está activo
    private bool isDialogueActive = false;

    private void Awake()
    {
        Debug.Log("=== DIALOGUE UI AWAKE ===");

        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("✅ Panel desactivado en Awake");
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
            Debug.Log("✅ ChoicesPanel desactivado en Awake");
        }

        if (nextButton == null) Debug.LogError("❌ NextButton es NULL!");
        if (dialogueText == null) Debug.LogError("❌ DialogueText es NULL!");
        if (player == null) Debug.LogError("❌ Player es NULL!");
    }

    private void Start()
    {
        Debug.Log("=== DIALOGUE UI START ===");

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();

            nextButton.onClick.AddListener(() => {
                Debug.Log("🔵 BOTÓN CLICKEADO!");
                AdvanceDialogue();
            });

            Debug.Log("✅ Listener añadido al NextButton");
            nextButton.interactable = true;
        }

        if (panel != null) panel.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);

        Debug.Log("✅ Start completado - Todo oculto");
    }

    // ✅ NUEVO: Update para bloquear TODOS los inputs del jugador durante diálogo
    private void Update()
    {
        if (isDialogueActive && player != null)
        {
            // ✅ BLOQUEAR inputs del jugador
            player.canMove = false;

            // ✅ Si el player tiene PlayerCombat, desactivarlo también
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.enabled = false;
            }
        }
    }

    private void HideAllPanels()
    {
        Debug.Log("🔴 HideAllPanels llamado");

        if (panel != null)
        {
            bool wasActive = panel.activeSelf;
            panel.SetActive(false);
            Debug.Log($"Panel desactivado (antes estaba: {(wasActive ? "activo" : "inactivo")})");
        }

        if (choicesPanel != null)
        {
            bool wasActive = choicesPanel.activeSelf;
            choicesPanel.SetActive(false);

            int childCount = choicesPanel.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(choicesPanel.transform.GetChild(i).gameObject);
            }

            Debug.Log($"ChoicesPanel desactivado (antes: {(wasActive ? "activo" : "inactivo")}), {childCount} hijos eliminados");
        }

        if (dialogueText != null) dialogueText.text = "";
        if (nameText != null) nameText.text = "";
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        Debug.Log("✅ HideAllPanels completado");
    }

    public void ShowDialogue(DialogueDataSO dialogueData)
    {
        ShowDialogue(dialogueData, null);
    }

    public void ShowDialogue(DialogueDataSO dialogueData, ShipBuilderNPC npc)
    {
        Debug.Log("=== SHOW DIALOGUE LLAMADO ===");

        if (dialogueData == null || dialogueData.lines == null || dialogueData.lines.Length == 0)
        {
            Debug.LogError("❌ DialogueData inválido!");
            return;
        }

        Debug.Log($"📖 Mostrando diálogo: '{dialogueData.name}' ({dialogueData.lines.Length} líneas)");

        // ✅ MARCAR que el diálogo está activo
        isDialogueActive = true;

        currentDialogue = dialogueData;
        currentNPC = npc;
        lines = dialogueData.lines;
        currentLineIndex = 0;

        // ✅ ACTIVAR panel
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log($"✅ Panel ACTIVADO");
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
            Debug.Log($"NextButton configurado: Active={nextButton.gameObject.activeSelf}, Interactable={nextButton.interactable}");
        }

        // ✅ DESACTIVAR COMPLETAMENTE el jugador
        if (player != null)
        {
            player.canMove = false;

            // ✅ Desactivar combate
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.enabled = false;
                Debug.Log("🔒 PlayerCombat DESACTIVADO");
            }

           

            Debug.Log("🔒 Jugador bloqueado completamente");
        }

        ShowCurrentLine();
    }

    public void AdvanceDialogue()
    {
        Debug.Log($"⏭️ ADVANCE DIALOGUE (Línea: {currentLineIndex + 1}/{lines?.Length ?? 0}, Typing: {isTyping})");

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("❌ lines inválido!");
            return;
        }

        if (isTyping)
        {
            Debug.Log("⏭️ Completando texto...");

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (currentLineIndex < lines.Length)
            {
                dialogueText.text = lines[currentLineIndex].text;
            }

            isTyping = false;
            return;
        }

        currentLineIndex++;
        Debug.Log($"Avanzando a línea: {currentLineIndex + 1}");

        if (currentLineIndex < lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            Debug.Log("📍 Llegó al final del diálogo");

            if (currentDialogue != null && currentDialogue.hasChoices &&
                currentDialogue.choices != null && currentDialogue.choices.Length > 0)
            {
                Debug.Log($"Tiene {currentDialogue.choices.Length} opciones - mostrando...");
                ShowChoices();
            }
            else
            {
                Debug.Log("No hay opciones - terminando diálogo");
                EndDialogue();
            }
        }
    }

    private void ShowCurrentLine()
    {
        if (lines == null || currentLineIndex >= lines.Length)
        {
            Debug.LogError($"❌ Índice fuera de rango: {currentLineIndex}/{lines?.Length}");
            return;
        }

        DialogueLine line = lines[currentLineIndex];
        string preview = line.text.Length > 50 ? line.text.Substring(0, 50) + "..." : line.text;
        Debug.Log($"📝 Línea {currentLineIndex + 1}/{lines.Length}: '{preview}'");

        if (nameText != null) nameText.text = line.speakerName;

        if (portraitImage != null)
        {
            if (line.speakerPortrait != null)
            {
                portraitImage.sprite = line.speakerPortrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.sprite = null;
                portraitImage.enabled = false;
            }
        }

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
        Debug.Log("✅ Texto completado");
    }

    private void ShowChoices()
    {
        Debug.Log("🔵 ShowChoices llamado");

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            Debug.Log("NextButton ocultado");
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(true);
            Debug.Log("ChoicesPanel ACTIVADO");

            int childCount = choicesPanel.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(choicesPanel.transform.GetChild(i).gameObject);
            }

            if (currentDialogue != null && currentDialogue.choices != null)
            {
                Debug.Log($"Creando {currentDialogue.choices.Length} botones...");

                for (int i = 0; i < currentDialogue.choices.Length; i++)
                {
                    DialogueChoice choice = currentDialogue.choices[i];

                    if (choiceButtonPrefab == null)
                    {
                        Debug.LogError("❌ choiceButtonPrefab es NULL!");
                        continue;
                    }

                    Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                    TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();

                    if (buttonText != null)
                        buttonText.text = choice.choiceText;

                    DialogueChoice capturedChoice = choice;
                    choiceButton.onClick.AddListener(() => OnChoiceSelected(capturedChoice));

                    Debug.Log($"✅ Opción {i + 1} creada: '{choice.choiceText}'");
                }
            }
        }
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        Debug.Log($"✅ Opción seleccionada: '{choice.choiceText}'");

        if (choice.checkRequirements && currentNPC != null && player != null)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();

            if (inventory != null && currentNPC.HasRequirements(inventory))
            {
                Debug.Log("✅ Requisitos cumplidos");
                currentNPC.CompleteQuest(inventory);

                if (choice.nextDialogue != null)
                    ShowDialogue(choice.nextDialogue, currentNPC);
                else
                    EndDialogue();
            }
            else
            {
                Debug.Log("❌ Requisitos NO cumplidos");

                if (currentNPC != null && currentNPC.missingRequirementsDialogue != null)
                    ShowDialogue(currentNPC.missingRequirementsDialogue, currentNPC);
                else
                    EndDialogue();
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
        else
        {
            Debug.LogWarning("⚠️ Opción sin acción - cerrando");
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        Debug.Log("=== END DIALOGUE LLAMADO ===");

        // ✅ MARCAR que el diálogo terminó
        isDialogueActive = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        HideAllPanels();

        lines = null;
        currentLineIndex = 0;
        currentDialogue = null;
        currentNPC = null;
        isTyping = false;

        // ✅ REACTIVAR COMPLETAMENTE el jugador
        if (player != null)
        {
            player.canMove = true;

            // ✅ Reactivar combate
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.enabled = true;
                Debug.Log("🔓 PlayerCombat REACTIVADO");
            }

           
        }

        Debug.Log("=== DIÁLOGO TERMINADO ===");
    }
}