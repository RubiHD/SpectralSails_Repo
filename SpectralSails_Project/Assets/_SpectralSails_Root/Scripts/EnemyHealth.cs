using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    private GameObject healthBarUI;
    private Image fillImage;
    private float hideTimer;
    public float hideDelay = 2f;

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject prefab = Resources.Load<GameObject>("EnemyHealthBar");
        healthBarUI = Instantiate(prefab, transform);
        healthBarUI.transform.localPosition = new Vector3(0, 1.5f, 0); // encima del enemigo

        fillImage = healthBarUI.transform.Find("Background/Fill").GetComponent<Image>();
        healthBarUI.SetActive(false);
    }

    private void Update()
    {
        if (healthBarUI.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
                healthBarUI.SetActive(false);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        float ratio = (float)currentHealth / maxHealth;
        fillImage.fillAmount = ratio;

        healthBarUI.SetActive(true);
        hideTimer = hideDelay;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
