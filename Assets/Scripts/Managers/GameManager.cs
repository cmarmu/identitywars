using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public ProtectorInfoPanel infoPanel;
    public RectTransform panelButtonContainer;

    private int score = 0;
    private int authTokens = 0;
    private const int MAX_AUTH_TOKENS = 100;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI authTokenText;

    [Header("AuthToken UI Colors")]
    public Color normalTokenColor = Color.darkGray;
    public Color maxTokenColor = new Color(1f, 0.84f, 0f); // Gold

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        //Debug.Log("Score increased to " + score);
    }

    public int GetScore()
    {
        return score;
    }

    public void AddAuthTokens(int amount)
    {
        if (amount <= 0) return;

        int previousTokens = authTokens;
        authTokens = Mathf.Min(authTokens + amount, MAX_AUTH_TOKENS);
        //Debug.Log($"AuthTokens increased by {amount}. Total: {authTokens}");

        UpdateAuthTokenUI();
    }

    public void SpendAuthTokens(int amount)
    {
        if (amount <= 0 || authTokens < amount)
        {
            //Debug.LogWarning("Not enough AuthTokens to spend.");
            return;
        }

        authTokens -= amount;
        //Debug.Log($"AuthTokens spent: {amount}. Remaining: {authTokens}");

        UpdateAuthTokenUI();
    }

    public int GetAuthTokens()
    {
        return authTokens;
    }

    public float GetPanelButtonContainerTopY()
    {
        if (panelButtonContainer == null) return float.MinValue;

        Vector3 worldPos = panelButtonContainer.position;
        float halfHeight = panelButtonContainer.rect.height * panelButtonContainer.lossyScale.y / 2f;
        return worldPos.y + halfHeight;
    }
    public void OnClickShowProtectorInfo(string protectorType)
    {
        var data = EntityDataManager.Instance.GetProtectorData(protectorType);
        if (data == null)
        {
            //Debug.LogWarning($"No ProtectorData found for: {protectorType}");
            return;
        }

        if (infoPanel == null)
        {
            //Debug.LogError("infoPanel is not assigned in GameManager!");
            return;
        }

        infoPanel.ShowProtectorInfo(data);
    }

    public void OnClickShowSpellInfo(string spellType)
    {
        var data = EntityDataManager.Instance.GetSpellData(spellType);
        if (data == null)
        {
            //Debug.LogWarning($"No SpellData found for: {spellType}");
            return;
        }

        if (infoPanel == null)
        {
            //Debug.LogError("infoPanel is not assigned in GameManager!");
            return;
        }

        infoPanel.ShowSpellInfo(data);
    }

    private Coroutine authTokenPulseRoutine;

    private void UpdateAuthTokenUI()
    {
        if (authTokenText != null)
        {
            authTokenText.text = "AuthTokens: " + authTokens;

            if (authTokens >= MAX_AUTH_TOKENS)
            {
                authTokenText.color = maxTokenColor;
                if (authTokenPulseRoutine == null)
                    authTokenPulseRoutine = StartCoroutine(PulseAuthTokenText());
            }
            else
            {
                authTokenText.color = normalTokenColor;
                if (authTokenPulseRoutine != null)
                {
                    StopCoroutine(authTokenPulseRoutine);
                    authTokenPulseRoutine = null;
                    authTokenText.transform.localScale = Vector3.one;
                }
            }
        }
    }

    private System.Collections.IEnumerator PulseAuthTokenText()
    {
        float pulseDuration = 0.5f;
        float scaleAmount = 1.2f;

        while (true)
        {
            // Scale up
            yield return ScaleText(Vector3.one * scaleAmount, pulseDuration / 2f);
            // Scale down
            yield return ScaleText(Vector3.one, pulseDuration / 2f);
        }
    }

    private System.Collections.IEnumerator ScaleText(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = authTokenText.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            authTokenText.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }

        authTokenText.transform.localScale = targetScale;
    
    }

    public bool CanSpendAuthTokens(int amount)
    {
        return authTokens >= amount;
    }

    public void FlashAuthTokensInsufficientAndShake()
    {
        if (authTokenText != null)
            StartCoroutine(FlashRedAndShake(authTokenText));
    }

    private IEnumerator FlashRedAndShake(TextMeshProUGUI text)
    {
        Color originalColor = text.color;
        Vector3 originalPos = text.transform.localPosition;
        
        text.color = Color.red;

        float shakeMagnitude = 5f;
        float shakeDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
            text.transform.localPosition = originalPos + new Vector3(x, y, 0);
            yield return null;
        }

        text.transform.localPosition = originalPos;
        text.color = originalColor;
    }


}
