using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ProtectorInfoPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI StatsText;
    public Image Icon;
    public float fadeDuration = 0.3f;

    public void ShowProtectorInfo(ProtectorData data)
    {
        NameText.text = data.name;
        DescriptionText.text = data.description;
        StatsText.text =
            $"Attack Power: {data.baseAttack}\n" +
            $"Speed: {data.speed}\n" +
            $"UnlocksAt: {data.scoreCost}";

        if (data.powerfulAgainst != null && data.powerfulAgainst.Count > 0)
            StatsText.text += $"\nPowerful Against: {string.Join(", ", data.powerfulAgainst)}";


        Icon.sprite = !string.IsNullOrEmpty(data.iconPath)
            ? Resources.Load<Sprite>(data.iconPath)
            : null;

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        Time.timeScale = 0;
    }

    public void ShowSpellInfo(SpellData data)
    {
        NameText.text = data.name;
        DescriptionText.text = data.description;
        StatsText.text =
            $"Effect: {data.effect}\n" +
            $"Cost: {data.cost}\n" +
            $"UnlocksAt: {data.unlockAt}";


        Icon.sprite = !string.IsNullOrEmpty(data.iconPath)
            ? Resources.Load<Sprite>(data.iconPath)
            : null;

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        Time.timeScale = 0;
    }

    public void ClosePanel()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0;
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}
