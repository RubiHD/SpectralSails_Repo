using UnityEngine;
using UnityEngine.UI;

public class SwordSlotUI : MonoBehaviour
{
    [Header("Components")]
    public Image swordIcon;
    public Image backgroundImage; // Opcional: fondo del slot
    public GameObject activeIndicator; // Opcional: indicador visual

    private Vector3 targetScale = Vector3.one;
    private Color targetColor = Color.white;
    private bool isActive = false;

    private void Awake()
    {
        // Obtener componentes automáticamente si no están asignados
        if (swordIcon == null)
        {
            // Buscar el hijo llamado "SwordIcon"
            Transform iconTransform = transform.Find("SwordIcon");
            if (iconTransform != null)
            {
                swordIcon = iconTransform.GetComponent<Image>();
                Debug.Log($"SwordIcon encontrado automáticamente en {gameObject.name}");
            }
            else
            {
                Debug.LogError($"❌ No se encontró SwordIcon en {gameObject.name}! Crea un hijo llamado 'SwordIcon' con componente Image.");
            }
        }

        // Buscar indicador hijo (opcional)
        Transform indicatorTransform = transform.Find("ActiveIndicator");
        if (indicatorTransform != null)
        {
            activeIndicator = indicatorTransform.gameObject;
        }
    }

    public void Initialize(Sprite sprite, bool active)
    {
        Debug.Log($"Inicializando slot con sprite: {(sprite != null ? sprite.name : "NULL")}, activo: {active}");

        if (swordIcon != null)
        {
            swordIcon.sprite = sprite;
            swordIcon.enabled = true; // Asegurarse de que está habilitado

            if (sprite == null)
            {
                Debug.LogWarning($"⚠️ Sprite NULL asignado a {gameObject.name}");
            }
            else
            {
                Debug.Log($"✅ Sprite {sprite.name} asignado correctamente a {gameObject.name}");
            }
        }
        else
        {
            Debug.LogError($"❌ swordIcon es NULL en {gameObject.name}!");
        }

        SetActive(active);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // Activar/desactivar indicador visual
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(active);
        }

        Debug.Log($"Slot {gameObject.name} - Activo: {active}");
    }

    public void AnimateScale(float targetScale, float speed)
    {
        this.targetScale = Vector3.one * targetScale;

        if (swordIcon != null)
        {
            swordIcon.transform.localScale = Vector3.Lerp(
                swordIcon.transform.localScale,
                this.targetScale,
                Time.deltaTime * speed
            );
        }
    }

    public void AnimateColor(Color targetColor, float speed)
    {
        this.targetColor = targetColor;

        if (swordIcon != null)
        {
            swordIcon.color = Color.Lerp(
                swordIcon.color,
                this.targetColor,
                Time.deltaTime * speed
            );
        }
    }
}