using System.Collections;
using UnityEngine;

public class FirewallHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    private Vector3 initialScale;
    private SpriteRenderer spriteRenderer;

    private bool isBoosted = false;
    public Sprite[] damageStages; // Assign these in the Inspector or load them dynamically.

    void Start()
    {
        currentHealth = maxHealth;
        initialScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (damageStages == null || damageStages.Length == 0)
        {
            // Example of dynamic loading from Resources
            damageStages = Resources.LoadAll<Sprite>("firewallLongStage_ico"); 
        }
    }

    public void TakeDamage(int amount)
    {
        if (isBoosted)
        {
            amount = Mathf.RoundToInt(amount * 0.5f); // Reduce damage by 50% if boosted
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateVisual();

        if (currentHealth <= 0)
        {
            GameOverManager gom = GameObject.FindFirstObjectByType<GameOverManager>();
            if (gom != null)
            {
                gom.TriggerGameOver();
            }
            Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateVisual();
    }

    public void BoostResistance(float duration)
    {
        StartCoroutine(BoostResistanceCoroutine(duration));
    }

    private IEnumerator BoostResistanceCoroutine(float duration)
    {
        isBoosted = true;
        
        yield return new WaitForSeconds(duration);

        isBoosted = false;
    }

    void UpdateVisual()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        float minScale = 0.9f;

        float scaledX = Mathf.Lerp(initialScale.x * minScale, initialScale.x, healthPercent);
        transform.localScale = new Vector3(scaledX, initialScale.y, initialScale.z);

        if (spriteRenderer != null && damageStages != null && damageStages.Length > 0)
        {
            int stageIndex = Mathf.FloorToInt((1 - healthPercent) * (damageStages.Length - 1));
            stageIndex = Mathf.Clamp(stageIndex, 0, damageStages.Length - 1);
            spriteRenderer.sprite = damageStages[stageIndex];
        }

        // Optional: Change color as well
        //spriteRenderer.color = Color.Lerp(Color.blueViolet, Color.white, healthPercent);
    }
}
