using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class ProtectorButtonLoader : MonoBehaviour
{
    public GameObject buttonPrefab; // Prefab: ButtonProtectorSpawn
    public Transform buttonContainer; // Panel donde se generan los botones
    public ProtectorSpawner spawner; // Asignar desde el Inspector
    public Sprite defaultIcon;

    float buttonSpacing = 250f; // Adjust based on your button sizes

    private void Start()
    {
        //Debug.Log("Generating buttons...");
        GenerateProtectorButtons();
    }

    private void GenerateProtectorButtons()
    {
        var protectors = EntityDataManager.Instance.GetAllProtectorData()
            .Where(p => !string.IsNullOrEmpty(p.iconPath) && Resources.Load<Sprite>(p.iconPath) != null)
            .OrderBy(p => p.scoreCost)
            .ToList();

        for (int i = 0; i < protectors.Count; i++)
        {
            var protectorData = protectors[i];
            //Debug.Log($"Generating button for protector: {protectorData.type}");
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            buttonGO.transform.SetSiblingIndex(0);

            buttonGO.name = $"ButtonProtector_{protectorData.type}";
            buttonGO.GetComponent<SpawnProtectorButton>().spawner = spawner;

            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-1780f + i * buttonSpacing, 0);

            var spawnButtonScript = buttonGO.GetComponent<SpawnProtectorButton>();
            spawnButtonScript.protectorType = protectorData.type;

            // Icono visual del protector
            Image img = buttonGO.transform.Find("ProtectorImg")?.GetComponent<Image>();
            if (img != null)
            {
                Sprite icon = Resources.Load<Sprite>(protectorData.iconPath);
                img.sprite = icon != null ? icon : defaultIcon;
            }

            // Configurar el botón de información
            var infoButton = buttonGO.transform.Find("ButtonInfoProtector")?.GetComponent<Button>();
            if (infoButton != null)
            {
                infoButton.onClick.AddListener(() => GameManager.Instance.OnClickShowProtectorInfo(protectorData.type));
            }

            buttonGO.GetComponent<SpawnProtectorButton>().Init();
        }
    }
}
