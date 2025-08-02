using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance;

    [Header("Visual FX Settings")]
    public Canvas visualFXCanvas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CastSpell(SpellData spell)
    {
       if (spell == null)
        {
            //Debug.LogWarning("SpellManager: Unknown spell type: " + spell.type);
            return;
        }

        ApplySpellEffect(spell.type);
        TriggerVisualEffect(spell);
    }

    private void ApplySpellEffect(string spellType)
    {
        switch (spellType)
        {
            case "frost_of_friction":
                ApplyCooldownSnowflakes();
                break;
            case "risk_engine_surge":
                ApplyRiskEngineSurge();
                break;
            case "scope_sniper":
                ApplyOAuthScopeTrim();
                break;
            case "session_rotation":
                ApplySessionRotation();
                break;
            case "token_overload":
                ApplyFirewallBoost();
                break;
            case "protector_patch":
                ApplyPatchManagement();
                break;
            default:
                //Debug.LogWarning("SpellManager: No effect logic implemented for: " + spellType);
                break;
        }
    }

    private void ApplyCooldownSnowflakes()
    {
        foreach (var virus in Object.FindObjectsByType<Virus>(FindObjectsSortMode.None))
        {
            virus.ApplySlow(0.9f, 5f);
        }
    }

    private void ApplyRiskEngineSurge()
    {
        foreach (var virus in Object.FindObjectsByType<Virus>(FindObjectsSortMode.None))
        {
            virus.WeakenResistance(7f);
        }
    }

    private void ApplyOAuthScopeTrim()
    {
        foreach (var virus in Object.FindObjectsByType<Virus>(FindObjectsSortMode.None))
        {
            virus.WeakenAttack(0.7f);
        }
    }

    private void ApplySessionRotation()
    {
        Vector3 startPosition = new Vector3(-10f, 0f, 0f);
        foreach (var virus in Object.FindObjectsByType<Virus>(FindObjectsSortMode.None))
        {
            virus.ForceToStart(startPosition);
        }
    }

    private void ApplyFirewallBoost()
    {
        foreach (var firewall in Object.FindObjectsByType<FirewallHealth>(FindObjectsSortMode.None))
        {
            firewall.Heal(25);
        }
    }

    private void ApplyPatchManagement()
    {
        foreach (var protector in Object.FindObjectsByType<Protector>(FindObjectsSortMode.None))
        {
            protector.HealResistance(4f);
        }
    }

    private void TriggerVisualEffect(SpellData spell)
    {
        if (visualFXCanvas == null || string.IsNullOrEmpty(spell.iconPath)) return;

        // Load all sprite variants for this spell
        var sprites = LoadSpellSprites(spell.iconPath);
        if (sprites.Count == 0)
        {
            //Debug.LogWarning($"SpellManager: No icons found for spell {spell.type} at {spell.iconPath}_0, _1, etc.");
            return;
        }

        StartCoroutine(SpawnIconsSequentially(sprites, Random.Range(20, 30)));
    }

    private IEnumerator SpawnIconsSequentially(List<Sprite> sprites, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var sprite = sprites[Random.Range(0, sprites.Count)];

            GameObject iconGO = new GameObject("SpellIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGO.transform.SetParent(visualFXCanvas.transform, false);

            Image img = iconGO.GetComponent<Image>();
            img.sprite = sprite;
            img.color = new Color(1, 1, 1, 0);

            RectTransform rt = iconGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(128, 128);
            rt.anchoredPosition = new Vector2(
                Random.Range(-visualFXCanvas.pixelRect.width / 2, visualFXCanvas.pixelRect.width / 2),
                Random.Range(-visualFXCanvas.pixelRect.height / 2, visualFXCanvas.pixelRect.height / 2)
            );

            StartCoroutine(AnimateIcon(iconGO));

            yield return new WaitForSeconds(0.05f); // Small delay between spawns
        }
    }


    private List<Sprite> LoadSpellSprites(string basePath)
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>(basePath);
        if (loadedSprites == null || loadedSprites.Length == 0)
        {
            //Debug.LogWarning($"No sprites found at {basePath}.");
        }
        return new List<Sprite>(loadedSprites);
    }




    private IEnumerator AnimateIcon(GameObject icon)
    {
        Image img = icon.GetComponent<Image>();
        RectTransform rt = icon.GetComponent<RectTransform>();

        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 originalScale = rt.localScale;
        Color startColor = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Fade in first half, fade out last half
            float alpha = t < 0.5f ? Mathf.Lerp(0f, 1f, t * 2f) : Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
            img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            // Pulse scale
            float pulse = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
            rt.localScale = originalScale * pulse;

            yield return null;
        }

        Destroy(icon);
    }
}
