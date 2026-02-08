using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CoinCounter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI coinText; // Si usas TextMeshPro
    // [SerializeField] private Text coinText; // Si usas UI Text normal (descomentar si lo usas)
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Icono de Moneda (Opcional)")] // ✅ NUEVO
    [SerializeField] private Image coinIcon; // Imagen de la moneda al lado del número
    [SerializeField] private bool rotateCoinIcon = true; // ¿Hacer girar el icono?
    [SerializeField] private float iconRotationSpeed = 50f; // Velocidad de rotación del icono

    [Header("Formato del Texto")]
    [SerializeField] private string prefix = ""; // Ej: "Monedas: "
    [SerializeField] private string suffix = ""; // Ej: ""

    [Header("Animación del Contador")]
    [SerializeField] private bool animateOnChange = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);

    [Header("Efectos de Sonido (Opcional)")]
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] private AudioSource audioSource;

    private int displayedCoins = 0;
    private Coroutine animationCoroutine;

    private void Start()
    {
        // Buscar PlayerInventory si no está asignado
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
        }

        // Verificar que tengamos las referencias necesarias
        if (coinText == null)
        {
            Debug.LogError("¡CoinText no está asignado en CoinCounter!");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError("¡PlayerInventory no encontrado!");
            return;
        }

        // Suscribirse al evento de cambio de monedas
        playerInventory.OnCoinsChanged.AddListener(UpdateCoinDisplay);

        // Inicializar display
        displayedCoins = playerInventory.coinCount;
        UpdateCoinDisplay(displayedCoins);
    }

    private void Update()
    {
        // ✅ NUEVO: Rotar el icono de moneda constantemente
        if (rotateCoinIcon && coinIcon != null)
        {
            coinIcon.transform.Rotate(0f, 0f, iconRotationSpeed * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir
        if (playerInventory != null)
        {
            playerInventory.OnCoinsChanged.RemoveListener(UpdateCoinDisplay);
        }
    }

    public void UpdateCoinDisplay(int newCoinCount)
    {
        if (coinText == null) return;

        // Actualizar texto
        coinText.text = $"{prefix}{newCoinCount}{suffix}";

        // Reproducir sonido
        if (newCoinCount > displayedCoins && coinCollectSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(coinCollectSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(coinCollectSound, Camera.main.transform.position);
            }
        }

        // Animar
        if (animateOnChange && newCoinCount != displayedCoins)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateCoinChange());
        }

        displayedCoins = newCoinCount;
    }

    private IEnumerator AnimateCoinChange()
    {
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Aplicar curva de escala
            float scaleMultiplier = scaleCurve.Evaluate(t);
            transform.localScale = originalScale * scaleMultiplier;

            yield return null;
        }

        // Asegurar que vuelva a escala original
        transform.localScale = originalScale;
    }

    // ✅ Método público para actualizar manualmente
    public void ForceUpdate()
    {
        if (playerInventory != null)
        {
            UpdateCoinDisplay(playerInventory.coinCount);
        }
    }
}