
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class QuestionUI : MonoBehaviour
{
    [Header("Main Text Elements")]
    [SerializeField] private TextMeshProUGUI QueryTxt;
    [SerializeField] private TextMeshProUGUI ProtectorTxtName;
    [SerializeField] private TextMeshProUGUI VirusTxtName;
    [SerializeField] private TextMeshProUGUI CooldownTxt;

    [Header("Images")]
    [SerializeField] private Image ProtectorImg;
    [SerializeField] private Image VirusImg;
    [SerializeField] private Sprite buttonSprite;

    [Header("Protector Info")]
    [SerializeField] private TextMeshProUGUI ProtectorTxtDescription;
    [SerializeField] private TextMeshProUGUI ProtectorTxtAttackPower;
    [SerializeField] private TextMeshProUGUI ProtectorTxtSpeed;

    [Header("Virus Info")]
    [SerializeField] private TextMeshProUGUI VirusTxtDescription;
    [SerializeField] private TextMeshProUGUI VirusTxtResistance;
    [SerializeField] private TextMeshProUGUI VirusTxtSpeed;
    [SerializeField] private TextMeshProUGUI VirusTxtDamage;

    [Header("Responses")]
    [SerializeField] private Transform PanelResponses;



    private List<Button> responseButtons = new();

    [SerializeField] private float totalTime = 15f;
    private float remainingTime;
    private bool countingDown = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    private void Update()
    {
        if (!countingDown) return;

        remainingTime -= Time.unscaledDeltaTime;
        SetCooldownText(remainingTime);

        if (remainingTime <= 0f)
        {
            countingDown = false;
            SetCooldownText(0);
            
            FadeAndClose(false); // false = incorrect by timeout
        }

    }

    public void Init(QuestionEntry entry, Protector protector, Virus virus)
    {
        // Set names and sprites
        ProtectorTxtName.text = protector.DisplayName;
        VirusTxtName.text = virus.DisplayName;
        ProtectorImg.sprite = protector.GetSprite();
        VirusImg.sprite = virus.GetSprite();

        // Set question
        QueryTxt.text = entry.question;
        
        // Set protector info
        ProtectorTxtDescription.text = protector.Description;
        ProtectorTxtAttackPower.text = $"Attack power: {protector.baseAttack}";
        ProtectorTxtSpeed.text = $"Speed: {protector.speed}";

        // Set virus info
        VirusTxtDescription.text = virus.Description;
        VirusTxtResistance.text = $"Resistance: {virus.resistance}";
        VirusTxtSpeed.text = $"Speed: {virus.speed}";
        VirusTxtDamage.text = $"Damage: {virus.baseAttack}";

        // Clear previous buttons
        foreach (Transform child in PanelResponses)
            Destroy(child.gameObject);
        responseButtons.Clear();

        // Create buttons directly in code
        for (int i = 0; i < entry.choices.Count; i++)
        {
            var btnGO = new GameObject("ResponseButton_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(PanelResponses, false);

            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray

            //img.sprite = buttonSprite;
            img.type = Image.Type.Sliced;
            

            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 80);  // Will stretch if PanelResponses uses a VerticalLayoutGroup

            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            var txt = txtGO.AddComponent<TextMeshProUGUI>();

            txt.text = entry.choices[i];
            txt.color = Color.black;
            txt.alignment = TextAlignmentOptions.Center;

            // Margins to prevent overflow clipping at edges
            txt.margin = new Vector4(10, 5, 10, 5);

            // Enable auto-sizing
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 30;
            txt.fontSizeMax = 100;

            // Correct wrapping and overflow handling
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.overflowMode = TextOverflowModes.Ellipsis;

            // Ensure rect transform is fully stretched to parent
            RectTransform txtRT = txt.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;


            Button btn = btnGO.GetComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => OnAnswerSelected(index == entry.correct, btn));
            responseButtons.Add(btn);
        }

        remainingTime = totalTime;
        countingDown = true;

    }

    private void OnAnswerSelected(bool isCorrect, Button clickedBtn)
    {
        foreach (var btn in responseButtons)
            btn.interactable = false;

        var img = clickedBtn.GetComponent<Image>();
        img.color = isCorrect ? new Color(0.5f, 1f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);

        //QuestionManager.Instance.OnAnswerSelected(isCorrect);
        FadeAndClose(isCorrect);
    }
    private void FadeAndClose(bool isCorrect)
    {
        canvasGroup.DOFade(0, 0.3f).SetUpdate(true);
        transform.DOScale(0.9f, 0.3f).SetUpdate(true);
        StartCoroutine(FinishAfterDelay(0.3f, isCorrect));
    }


    private IEnumerator FinishAfterDelay(float delay, bool isCorrect)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1; // ✅ game resumes cleanly
        QuestionManager.Instance.OnAnswerSelected(isCorrect);
        Destroy(gameObject);
    }

    public void SetCooldownText(float timeRemaining)
    {
        CooldownTxt.text = $"{Mathf.Ceil(timeRemaining)}s";
    }
}
