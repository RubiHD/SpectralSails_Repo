using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwordUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerCombat playerCombat; // Referencia al sistema de combate

    [Header("Configuración de Slots")]
    public GameObject swordSlotPrefab; // Prefab del slot de espada
    public Transform slotsContainer; // Contenedor de los slots
    public float slotSpacing = 80f; // Espacio entre slots

    [Header("Sprites de Espadas")]
    public Sprite basicSwordIcon; // Icono de la espada básica
    public Sprite advancedSwordIcon; // Icono de la espada avanzada
                                     // Añade más sprites aquí para más espadas

    [Header("Visual Settings")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public float activeSwordScale = 1.2f; // Escala de la espada activa
    public float inactiveSwordScale = 1f;

    [Header("Animation")]
    public bool animateSwitch = true;
    public float switchAnimationSpeed = 10f;

    private List<SwordSlotUI> swordSlots = new List<SwordSlotUI>();
    private int lastSwordCount = 0;
    private int currentSwordIndex = 0;

    private void Start()
    {
        // Buscar PlayerCombat automáticamente si no está asignado
        if (playerCombat == null)
        {
            playerCombat = FindObjectOfType<PlayerCombat>();

            if (playerCombat == null)
            {
                Debug.LogError("PlayerCombat no encontrado! Asigna manualmente en el Inspector.");
                return;
            }
        }

        // Inicializar UI
        UpdateSwordSlots();
    }

    private void Update()
    {
        if (playerCombat == null) return;

        // Verificar si la cantidad de espadas cambió
        if (playerCombat.swords.Count != lastSwordCount)
        {
            UpdateSwordSlots();
            lastSwordCount = playerCombat.swords.Count;
        }

        // Actualizar espada activa
        UpdateActiveSword();

        // Animar transiciones
        if (animateSwitch)
        {
            AnimateSwordSlots();
        }
    }

    private void UpdateSwordSlots()
    {
        // Limpiar slots existentes
        foreach (var slot in swordSlots)
        {
            if (slot != null && slot.gameObject != null)
            {
                Destroy(slot.gameObject);
            }
        }
        swordSlots.Clear();

        // Crear slots para cada espada
        for (int i = 0; i < playerCombat.swords.Count; i++)
        {
            GameObject slotObj = Instantiate(swordSlotPrefab, slotsContainer);
            SwordSlotUI slotUI = slotObj.GetComponent<SwordSlotUI>();

            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<SwordSlotUI>();
            }

            // Configurar posición del slot
            RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * slotSpacing, 0);

            // Asignar sprite según tipo de espada
            Sprite swordSprite = GetSwordSprite(playerCombat.swords[i]);
            slotUI.Initialize(swordSprite, i == 0);

            swordSlots.Add(slotUI);
        }

        Debug.Log($"UI de espadas actualizada: {playerCombat.swords.Count} espadas");
    }

    private void UpdateActiveSword()
    {
        // Obtener índice de espada activa mediante reflexión (ya que es privado)
        int activeSwordIndex = GetCurrentSwordIndex();

        if (activeSwordIndex != currentSwordIndex)
        {
            currentSwordIndex = activeSwordIndex;

            // Actualizar todos los slots
            for (int i = 0; i < swordSlots.Count; i++)
            {
                if (swordSlots[i] != null)
                {
                    swordSlots[i].SetActive(i == currentSwordIndex);
                }
            }
        }
    }

    private void AnimateSwordSlots()
    {
        for (int i = 0; i < swordSlots.Count; i++)
        {
            if (swordSlots[i] == null) continue;

            bool isActive = (i == currentSwordIndex);

            // Animar escala
            float targetScale = isActive ? activeSwordScale : inactiveSwordScale;
            swordSlots[i].AnimateScale(targetScale, switchAnimationSpeed);

            // Animar color
            Color targetColor = isActive ? activeColor : inactiveColor;
            swordSlots[i].AnimateColor(targetColor, switchAnimationSpeed);
        }
    }

    private Sprite GetSwordSprite(Sword sword)
    {
        // Determinar qué sprite usar según el tipo de espada
        if (sword is BasicSword)
        {
            return basicSwordIcon;
        }
        else if (sword is AdvancedSword)
        {
            return advancedSwordIcon;
        }

        // Sprite por defecto
        return basicSwordIcon;
    }

    // ✅ Método para obtener el índice actual (usando reflexión)
    private int GetCurrentSwordIndex()
    {
        if (playerCombat != null)
        {
            return playerCombat.GetCurrentSwordIndex();
        }

        return 0;
    }

    // ✅ MÉTODO PÚBLICO para forzar actualización
    public void ForceUpdate()
    {
        UpdateSwordSlots();
    }
}