using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SpellButtonLoader : MonoBehaviour
{
    public GameObject buttonPrefab; // Prefab with SpellSpawnButton component
    public Transform buttonContainer; // Panel to generate the buttons
    public Sprite defaultIcon;

    public float verticalSpacing = 160f;
    public float startY = -50f;
    public float startX = -191f; // Adjust based on your layout

    private void Start()
    {
        //Debug.Log("Generating spell buttons...");
        GenerateSpellButtons();
    }

    private void GenerateSpellButtons()
    {
        var spells = EntityDataManager.Instance.GetAllSpellData()
            .Where(s => !string.IsNullOrEmpty(s.iconPath) && Resources.Load<Sprite>(s.iconPath) != null)
            .OrderBy(s => s.unlockAt)
            .ToList();

        for (int i = 0; i < spells.Count; i++)
        {
            var spellData = spells[i];
            //Debug.Log($"Generating button for spell: {spellData.type}");

            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            buttonGO.transform.SetSiblingIndex(0);

            buttonGO.name = $"ButtonSpell_{spellData.type}";

            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX, startY - i * verticalSpacing);

            var spellButtonScript = buttonGO.GetComponent<SpellSpawnButton>();
            spellButtonScript.spellType = spellData.type;
            spellButtonScript.Init();  // Ensure Init() exists to refresh visuals

            // Set spell icon
            Image img = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
            if (img != null)
            {
                Sprite icon = Resources.Load<Sprite>(spellData.iconPath);
                img.sprite = icon != null ? icon : defaultIcon;
            }

            // Optionally, add info button behavior if desired
            Button infoButton = buttonGO.transform.Find("ButtonInfoSpell")?.GetComponent<Button>();
            if (infoButton != null)
            {
                 infoButton.onClick.AddListener(() => GameManager.Instance.OnClickShowSpellInfo(spellData.type));
            }
        }
    }
}
