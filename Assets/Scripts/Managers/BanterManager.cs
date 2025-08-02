using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BanterManager : MonoBehaviour
{
    public static BanterManager Instance;

    [Header("UI Elements")]
    public CanvasGroup meanHackerGroup;
    public TextMeshProUGUI meanHackerText;

    public CanvasGroup ethicHackerGroup;
    public TextMeshProUGUI ethicHackerText;

    [Header("Timing")]
    public float bubbleDuration = 3f;
    public float cooldownTime = 5f;

    private bool isMeanBanterShowing = false;
    private bool isEthicBanterShowing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TryShowVirusBanter(string virusType)
    {
        //Debug.Log($"BanterManager: Attempting to show banter for virus type: {virusType}");
        if (Random.value > 0.5f || isMeanBanterShowing) return;

        var data = EntityDataManager.Instance.GetVirusData(virusType);
        if (data?.banter == null || data.banter.Count == 0) return;

        string banter = data.banter[Random.Range(0, data.banter.Count)];
        StartCoroutine(ShowBanter(meanHackerGroup, meanHackerText, banter, true));
    }

    public void TryShowProtectorBanter(string protectorType)
    {
        if (Random.value > 0.5f || isEthicBanterShowing) return;

        var data = EntityDataManager.Instance.GetProtectorData(protectorType);
        if (data?.banter == null || data.banter.Count == 0) return;

        string banter = data.banter[Random.Range(0, data.banter.Count)];
        StartCoroutine(ShowBanter(ethicHackerGroup, ethicHackerText, banter, false));
    }

    private IEnumerator ShowBanter(CanvasGroup group, TextMeshProUGUI textField, string message, bool isMean)
    {
        if (group == null || textField == null) yield break;

        if (isMean) isMeanBanterShowing = true;
        else isEthicBanterShowing = true;

        textField.text = message;
        StartCoroutine(FadeCanvasGroup(group, 0, 1, 0.3f));

        yield return new WaitForSeconds(bubbleDuration);

        group.alpha = 0;

        yield return new WaitForSeconds(cooldownTime);

        if (isMean) isMeanBanterShowing = false;
        else isEthicBanterShowing = false;
    }
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = to;
    }

}
