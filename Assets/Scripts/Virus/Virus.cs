using System.Collections;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [Header("Virus Stats")]
    public int resistance;
    public int baseAttack;
    public int bonusPoints = 5;

    [Header("Movement Settings")]
    public float speed;
    public float floatAmplitude;
    public float floatFrequency;

    public string VirusType { get; private set; }
    public string DisplayName { get; private set; }
    public string Description { get; private set; }

    public bool IsDying { get; internal set; }

    private bool canMove = false;
    private float timeOffset;
    private Vector3 targetPosition;

    void Update()
    {
        if (!canMove) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        float floatOffset = Mathf.Sin(Time.time * floatFrequency + timeOffset) * floatAmplitude;
        Vector3 sideDirection = Vector3.Cross(direction, Vector3.forward).normalized;

        transform.position += direction * speed * Time.deltaTime;

        Vector3 sideMovement = sideDirection * floatOffset * Time.deltaTime;
        Vector3 newPosition = transform.position + sideMovement;

        float spriteHeight = GetComponent<SpriteRenderer>()?.bounds.size.y ?? 0.5f;
        float cameraVerticalExtent = Camera.main.orthographicSize;
        float screenMinY = -cameraVerticalExtent + spriteHeight / 2;
        float screenMaxY = cameraVerticalExtent - spriteHeight / 2;

        float panelTopY = GameManager.Instance.GetPanelButtonContainerTopY();
        float minY = Mathf.Max(screenMinY, panelTopY + spriteHeight / 2);

        // Clamp Y to both screen bounds and panel bounds
        newPosition.y = Mathf.Clamp(newPosition.y, minY, screenMaxY);

        transform.position = newPosition;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            GameObject firewall = GameObject.FindWithTag("Firewall");
            firewall?.GetComponent<FirewallHealth>()?.TakeDamage(baseAttack);
            Destroy(gameObject);
        }
    }


    public void Init(VirusData data)
    {
        resistance = Mathf.RoundToInt(data.resistance);
        baseAttack = Mathf.RoundToInt(data.baseAttack);
        speed = data.speed;
        floatAmplitude = data.floatAmplitude;
        floatFrequency = data.floatFrequency;
        //bonusPoints = 5;

        VirusType = data.type;
        DisplayName = data.name;
        Description = data.description;

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
        StartMoving();
    }

    public void StartMoving()
    {
        Transform firewall = GameObject.FindWithTag("Firewall")?.transform;
        if (firewall != null)
        {
            float height = 2f;
            SpriteRenderer sr = firewall.GetComponent<SpriteRenderer>();
            if (sr != null)
                height = sr.bounds.size.y;

            float minY = firewall.position.y - height / 2f;
            float maxY = firewall.position.y + height / 2f;
            float randomY = Random.Range(minY, maxY);

            targetPosition = new Vector3(firewall.position.x, randomY, firewall.position.z);
            canMove = true;
            timeOffset = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    public virtual Sprite GetSprite()
    {
        var sr = GetComponent<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }

    public virtual bool TakeHit(int power)
    {
        resistance -= power;
        if (resistance <= 0)
        {
            // Disable collider immediately
            if (TryGetComponent<Collider2D>(out var col))
                col.enabled = false;
            resistance = 0;
            IsDying = true;
            GameManager.Instance.AddScore(GetBonusPoints());
            //Destroy(gameObject);
            PlayDeathEffectAndDestroy();
            return true;
        }
        return false;
    }

    public void PlayDeathEffectAndDestroy()
    {
        StartCoroutine(FadeAndDestroy());
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
    public int GetBonusPoints()
    {
        float score =
            resistance * 1.2f +         // Heavily weighted: requires more effort
            baseAttack * 1.0f +        // Important: risk of damage
            speed * 5f;                 // Lower weight: threat timing

        return Mathf.RoundToInt(score);
    }
    
    public void ApplySlow(float slowFactor, float duration)
    {
        StartCoroutine(ApplySlowCoroutine(slowFactor, duration));
    }

    private IEnumerator ApplySlowCoroutine(float slowFactor, float duration)
    {
        float originalSpeed = speed;
        speed *= slowFactor;
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }

    public void WeakenResistance(float amount)
    {
        resistance -= Mathf.RoundToInt(amount);
        if (resistance < 0) resistance = 0;
    }

    public void WeakenAttack(float percentage)
    {
        int reduction = Mathf.RoundToInt(baseAttack * percentage);
        baseAttack -= reduction;
        if (baseAttack < 0) baseAttack = 0;
    }

    public void ForceToStart(Vector3 startPosition)
    {
        transform.position = startPosition;
    }


}
