using UnityEngine;
using System.Collections.Generic;

public class MeanHacker : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject virusPrefab; // Just one prefab
    public Transform spawnOrigin;
    public float verticalRange = 4f;

    private float spawnTimer;
    private float currentInterval;

    private List<string> virusTypes;

    void Start()
    {
        currentInterval = 5f;
        spawnTimer = currentInterval;

        // Cache available virus types
        virusTypes = new List<string>(EntityDataManager.Instance.GetAllVirusTypes());
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnVirus();
            AdjustSpawnInterval();
            spawnTimer = currentInterval;
        }
    }

    void SpawnVirus()
    {
        if (virusPrefab == null || spawnOrigin == null || virusTypes.Count == 0)
        {
            //Debug.LogWarning("MeanHacker: virusPrefab, spawnOrigin or virusTypes not configured.");
            return;
        }

        string selectedType = SelectWeightedVirusType();
        VirusData data = EntityDataManager.Instance.GetVirusData(selectedType);
        if (data == null)
        {
            //Debug.LogWarning($"MeanHacker: No data for virus type {selectedType}");
            return;
        }

        bool spawnSwarm = Random.value < 0.15f; // 15% chance for a swarm
        int spawnCount = spawnSwarm ? Random.Range(3, 6) : 1;

        if (spawnSwarm)
            BanterManager.Instance.TryShowVirusBanter($"Brace yourself! A swarm of {selectedType} incoming!");

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleVirus(selectedType);
        }
    }

    void SpawnSingleVirus(string virusType)
    {
        Vector3 basePos = spawnOrigin.position;
        float halfRange = verticalRange / 2f;
        float randomY = Random.Range(basePos.y - halfRange, basePos.y + halfRange);
        Vector3 spawnPosition = new Vector3(basePos.x, randomY, basePos.z);

        GameObject virusGO = Instantiate(virusPrefab, spawnPosition, Quaternion.identity);
        VirusSpawnAnimator animator = virusGO.GetComponent<VirusSpawnAnimator>();
        if (animator != null)
            animator.virusType = virusType;
    }


    string SelectWeightedVirusType()
    {
        int currentScore = GameManager.Instance.GetScore();
        List<(string virusType, float weight)> weightedList = new();

        foreach (var type in virusTypes)
        {
            var data = EntityDataManager.Instance.GetVirusData(type);
            if (data == null) continue;

            // Strict filter: only include viruses whose scoreCost <= currentScore
            if (data.scoreCost > currentScore) continue;

            // Inverse weight based on scoreCost
            float weight = Mathf.Max(1f, (currentScore - data.scoreCost + 10f) / 10f);

            weightedList.Add((type, weight));
        }

        if (weightedList.Count == 0)
        {
            //Debug.LogWarning("MeanHacker: No eligible virus types for current score.");
            return virusTypes[0]; // fallback to first virus type
        }

        float totalWeight = 0f;
        foreach (var entry in weightedList)
            totalWeight += entry.weight;

        float randomValue = Random.Range(0, totalWeight);
        float cumulative = 0f;

        foreach (var entry in weightedList)
        {
            cumulative += entry.weight;
            if (randomValue <= cumulative)
                return entry.virusType;
        }

        return weightedList[0].virusType; // fallback
    }

    void AdjustSpawnInterval()
    {
        int score = GameManager.Instance.GetScore();

        if (score < 1000)
            currentInterval = 5f;
        else if (score < 2000)
            currentInterval = 4f;
        else if (score < 3000)
            currentInterval = 3f;
        else if (score < 400)
            currentInterval = 2f;
        else
            currentInterval = 1f;
    }
}
