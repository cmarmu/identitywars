
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Protector : MonoBehaviour
{
    [Header("Stats")]
    public float speed;
    public float floatAmplitude;
    public float floatFrequency;
    public int baseAttack = 10;
    private string protectorType = "default";
    private string displayName = "Protector";
    private string description = "A security protector.";
    public float resistance;
    public List<string> powerfulAgainst;

    public virtual string ProtectorType => protectorType;
    public virtual string DisplayName => displayName;
    public virtual string Description => description;

    public void Init(ProtectorData data)
    {
        speed = data.speed;
        floatAmplitude = data.floatAmplitude;
        floatFrequency = data.floatFrequency;
        baseAttack = Mathf.RoundToInt(data.baseAttack);

        protectorType = data.type;
        displayName = data.name;
        description = data.description;
        resistance = data.resistance;
        powerfulAgainst = data.powerfulAgainst;

        Sprite sprite = Resources.Load<Sprite>(data.iconPath);
        if (sprite != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;

                // Normalize based on sprite size
                float targetVisualSize = 1f; // Target visual size in world units
                float baseScaleDown = 1f;  // Base factor for large sprite sizes

                float largestDim = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
                float normalizedScale = targetVisualSize / largestDim;

                float finalScale = normalizedScale * baseScaleDown;
                transform.localScale = new Vector3(finalScale, finalScale, 1f);
            }
        }
        // Add behavior dynamically based on category
        AttachCategoryBehaviour(data.category);
    }

    void AttachCategoryBehaviour(string category)
    {
        if (string.IsNullOrEmpty(category)) return;

        // map category to class
        switch (category.ToLower())
        {
            case "multi_factor_auth":
                gameObject.AddComponent<MultiFactorAuthBehaviour>();
                break;
            case "user_awareness":
                //gameObject.AddComponent<UserAwarenessBehaviour>();
                break;
            // Add more as you implement
            default:
                gameObject.AddComponent<MultiFactorAuthBehaviour>();
                //Debug.LogWarning($"No behaviour implemented for category: {category}");
                break;
        }
    }

    public virtual Sprite GetSprite()
    {
        var sr = GetComponent<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }

    public void DisableCollider()
    {
        if (TryGetComponent<Collider2D>(out var collider))
            collider.enabled = false;
    }


    public virtual void PlayDeathEffectAndDestroy()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            //Destroy(gameObject, ps.main.duration);
            StartCoroutine(FadeAndDestroy());
        }
        else
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float fadeDuration = 0.5f;
        float elapsed = 0f;
        Color initialColor = sr.color;
        Vector3 initialScale = transform.localScale;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Fade
            float alpha = Mathf.Lerp(1f, 0f, t);
            sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            // Shrink
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    public void HealResistance(float amount)
    {
        resistance += amount;
    }

    public void BoostAttack(int amount, float duration)
    {
        StartCoroutine(BoostAttackCoroutine(amount, duration));
    }

    private IEnumerator BoostAttackCoroutine(int boostAmount, float duration)
    {
        baseAttack += boostAmount;
        yield return new WaitForSeconds(duration);
        baseAttack -= boostAmount;
        if (baseAttack < 0) baseAttack = 0;
    }

    public void IncreaseResistanceTemporarily(float amount, float duration)
    {
        StartCoroutine(IncreaseResistanceCoroutine(amount, duration));
    }

    private IEnumerator IncreaseResistanceCoroutine(float amount, float duration)
    {
        resistance += amount;
        yield return new WaitForSeconds(duration);
        resistance -= amount;
        if (resistance < 0) resistance = 0;
    }

    
}
