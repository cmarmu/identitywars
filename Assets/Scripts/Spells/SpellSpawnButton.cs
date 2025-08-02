using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellSpawnButton : MonoBehaviour
{
    public string spellType;
    public TextMeshProUGUI buttonText;
    public Image iconImage;
    private SpellData spellData;

    public void Init()
    {
        spellData = EntityDataManager.Instance.GetSpellData(spellType);

        buttonText.text = $" ({spellData.cost})";
        buttonText.color = Color.gold;

        if (!string.IsNullOrEmpty(spellData.iconPath))
        {
            var sprite = Resources.Load<Sprite>(spellData.iconPath);
            if (sprite != null) iconImage.sprite = sprite;
        }

        GetComponent<Button>().onClick.AddListener(CastSpell);

        CheckUnlockStatus();
    }

    private void Update()
    {
        CheckUnlockStatus();
    }

    void CheckUnlockStatus()
    {
        if (spellData == null) return;

        int currentScore = GameManager.Instance.GetScore();
        bool unlocked = currentScore >= spellData.unlockAt;

        GetComponent<Button>().interactable = unlocked;

        if (!unlocked)
        {
            buttonText.color = Color.gray;
        }
    }



    void CastSpell()
    {
        if (spellData == null)
        {
            //Debug.LogError($"SpellSpawnButton: Spell data not found for type: {spellData.type}");
            return;
        }
        if (GameManager.Instance.CanSpendAuthTokens(spellData.cost))
        {
            GameManager.Instance.SpendAuthTokens(spellData.cost);
            SpellManager.Instance.CastSpell(spellData);
        }
        else
        {
            //Debug.Log("SpellSpawnButton: Not enough AuthTokens to cast the spell.");
            GameManager.Instance.FlashAuthTokensInsufficientAndShake();
        }
    }
}
