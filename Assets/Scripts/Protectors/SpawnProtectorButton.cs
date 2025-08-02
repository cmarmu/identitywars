using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SpawnProtectorButton : MonoBehaviour
{
    [Header("Protector Settings")]
    public string protectorType;
    public GameObject protectorPrefab;
    public ProtectorSpawner spawner;

    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI cooldownText;

    private float cooldownDuration = 5f;
    private bool isOnCooldown = false;
    private bool isUnlocked = false;
    private float scoreRequirement;

    private static bool isGlobalCooldownActive = false;


    void Awake()
    {
        //Debug.Log($"SpawnProtectorButton: Initializing for protector type: {protectorType}");
        if (button == null)
            button = GetComponent<Button>();
        if (cooldownText == null)
            cooldownText = GetComponentInChildren<TextMeshProUGUI>();

        button.interactable = false;

        var data = EntityDataManager.Instance.GetProtectorData(protectorType);
        if (data != null)
        {
            cooldownDuration = data.spawnCooldown;
            scoreRequirement = data.scoreCost * 3;
            //Debug.Log($"ProtectorSpawnButton: Protector data found for type: {protectorType}, cooldown: {cooldownDuration}, score requirement: {scoreRequirement}");
            cooldownText.text = $"{scoreRequirement}"; //unlocks at this score
            cooldownText.color = Color.darkRed;
        }
        else
        {
            //Debug.LogError($"[ProtectorSpawnButton] Protector data not found for type: {protectorType}");
        }

        button.onClick.AddListener(OnClickSpawn);
    }

    public void Init()
    {
        //Debug.Log($"SpawnProtectorButton: Initializing for protector type: {protectorType}");
        if (button == null)
            button = GetComponent<Button>();
        if (cooldownText == null)
            cooldownText = GetComponentInChildren<TextMeshProUGUI>();

        button.interactable = false;

        var data = EntityDataManager.Instance.GetProtectorData(protectorType);
        if (data != null)
        {
            cooldownDuration = data.spawnCooldown;
            scoreRequirement = data.scoreCost * 3;
            //Debug.Log($"ProtectorSpawnButton: Protector data found for type: {protectorType}, cooldown: {cooldownDuration}, score requirement: {scoreRequirement}");
            cooldownText.text = $"{scoreRequirement}"; //unlocks at this score
            cooldownText.color = Color.darkRed;
        }
        else
        {
            //Debug.LogError($"[ProtectorSpawnButton] Protector data not found for type: {protectorType}");
        }

        button.onClick.AddListener(OnClickSpawn);
    }

    void Update()
    {
        if (!isUnlocked)
        {
            //Debug.Log($"ProtectorSpawnButton: Current score is {GameManager.Instance.GetScore()}, required score is {scoreRequirement}");
            if (GameManager.Instance.GetScore() >= scoreRequirement)
            {
                isUnlocked = true;
                cooldownText.text = "";
                button.interactable = true;
            }
        }
    }

    public void OnClickSpawn()
    {
        if (isOnCooldown || !isUnlocked || protectorPrefab == null || spawner == null || isGlobalCooldownActive)
            return;

        StartCoroutine(StartCooldown());
    }


    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        isGlobalCooldownActive = true;
        SetAllButtonsInteractable(false);

        float timeLeft = cooldownDuration;
        //Debug.Log($"ProtectorSpawnButton: Starting cooldown for {cooldownDuration} seconds");

        while (timeLeft > 0)
        {
            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(timeLeft).ToString();
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        spawner.SpawnProtector(protectorType, protectorPrefab);
        cooldownText.text = "";

        SetAllButtonsInteractable(true);
        isGlobalCooldownActive = false;
        isOnCooldown = false;
    }

    private void SetAllButtonsInteractable(bool value)
    {
        var allButtons = FindObjectsByType<SpawnProtectorButton>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            if (btn.isUnlocked && !btn.isOnCooldown)
                btn.button.interactable = value;
        }
    }
}
