using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea]
    public string text;
    public string speakerName;
    public Sprite speakerPortrait;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText; // "Sí" o "No"
    public DialogueDataSO nextDialogue; // A qué diálogo lleva (opcional)
    public bool checkRequirements; // Si debe verificar inventario
    public bool endDialogue; // Si termina el diálogo
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueData")]
public class DialogueDataSO : ScriptableObject
{
    public DialogueLine[] lines;

    [Header("Opciones de Respuesta (Opcional)")]
    public bool hasChoices = false;
    public DialogueChoice[] choices;
}