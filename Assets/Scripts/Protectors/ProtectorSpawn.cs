// ProtectorSpawner.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProtectorSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public float verticalRange = 4f;


    public void SpawnProtector(string protectorType, GameObject protectorPrefab)
    {
        if (protectorPrefab == null || spawnPoint == null)
        {
            //Debug.LogError("Protector prefab or spawn point not assigned.");
            return;
        }

        BanterManager.Instance.TryShowProtectorBanter(protectorType);

        Vector3 basePos = spawnPoint.position;
        float halfRange = verticalRange / 2f;
        float randomY = Random.Range(basePos.y - halfRange, basePos.y + halfRange);

        GameObject protector = Instantiate(protectorPrefab, spawnPoint.position, Quaternion.identity);
        // After instantiating the prefab
        Protector protectorScript = protector.GetComponent<Protector>();
        if (protectorScript != null)
        {
            var data = EntityDataManager.Instance.GetProtectorData(protectorType);
            protectorScript.Init(data);
        }
    }
}
