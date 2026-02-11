using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si usas TextMeshPro (recomendado)

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerGun playerGun; // Referencia al script de disparo

    [Header("UI Elements")]
    public Image ammoIcon; // Imagen del icono de bala
    public TextMeshProUGUI ammoText; // Texto del número de balas (TextMeshPro)
                                     // public Text ammoText; // ← Usa este si usas UI Text normal en vez de TextMeshPro

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color lowAmmoColor = Color.red; // Color cuando quedan pocas balas
    public int lowAmmoThreshold = 3; // Cantidad considerada "poca munición"

    [Header("Animation")]
    public bool animateOnShoot = true;
    public float scaleAmount = 1.2f; // Cuánto se agranda al disparar
    public float animationSpeed = 10f;

    [Header("Effects")]
    public bool blinkWhenEmpty = true;
    public float blinkSpeed = 2f;

    private Vector3 originalScale;
    private bool isAnimating = false;
    private int lastAmmoCount = -1;

    private void Start()
    {
        // Buscar PlayerGun automáticamente si no está asignado
        if (playerGun == null)
        {
            playerGun = FindObjectOfType<PlayerGun>();

            if (playerGun == null)
            {
                Debug.LogError("PlayerGun no encontrado! Asigna manualmente en el Inspector.");
                return;
            }
        }

        // Guardar escala original para animaciones
        if (ammoIcon != null)
        {
            originalScale = ammoIcon.transform.localScale;
        }

        // Actualizar UI inmediatamente
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        if (playerGun == null) return;

        // Verificar si la munición cambió
        if (playerGun.ammo != lastAmmoCount)
        {
            UpdateAmmoDisplay();

            // Animar si disminuyó (disparó)
            if (animateOnShoot && playerGun.ammo < lastAmmoCount)
            {
                AnimateShoot();
            }

            lastAmmoCount = playerGun.ammo;
        }

        // Actualizar animación
        if (isAnimating && ammoIcon != null)
        {
            ammoIcon.transform.localScale = Vector3.Lerp(
                ammoIcon.transform.localScale,
                originalScale,
                Time.deltaTime * animationSpeed
            );

            if (Vector3.Distance(ammoIcon.transform.localScale, originalScale) < 0.01f)
            {
                ammoIcon.transform.localScale = originalScale;
                isAnimating = false;
            }
        }

    }

    private void UpdateAmmoDisplay()
    {
        if (playerGun == null) return;

        int currentAmmo = playerGun.ammo;

        // Actualizar texto
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();

            // Cambiar color si queda poca munición
            if (currentAmmo <= lowAmmoThreshold && currentAmmo > 0)
            {
                ammoText.color = lowAmmoColor;
            }
            else if (currentAmmo == 0)
            {
                ammoText.color = Color.gray;
            }
            else
            {
                ammoText.color = normalColor;
            }
        }

        // Actualizar icono
        if (ammoIcon != null)
        {
            // Cambiar opacidad si no hay munición
            Color iconColor = ammoIcon.color;
            iconColor.a = currentAmmo > 0 ? 1f : 0.3f;
            ammoIcon.color = iconColor;
        }

        {
            // Parpadeo cuando no hay munición
            if (blinkWhenEmpty && playerGun != null && playerGun.ammo == 0)
            {
                float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

                if (ammoText != null)
                {
                    Color textColor = ammoText.color;
                    textColor.a = alpha;
                    ammoText.color = textColor;
                }
            }
            else if (ammoText != null)
            {
                Color textColor = ammoText.color;
                textColor.a = 1f;
                ammoText.color = textColor;
            }
        }
    }

    private void AnimateShoot()
    {
        if (ammoIcon != null)
        {
            ammoIcon.transform.localScale = originalScale * scaleAmount;
            isAnimating = true;
        }
    }

    // ✅ MÉTODO PÚBLICO para forzar actualización (útil para debugging)
    public void ForceUpdate()
    {
        UpdateAmmoDisplay();
    }
}