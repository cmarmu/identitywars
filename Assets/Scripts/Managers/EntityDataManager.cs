using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VirusData
{
    public string type;
    public string name;
    public string description;
    public string iconPath;
    public float baseAttack;
    public float resistance;
    public float speed;
    public float spawnCooldown;
    public float floatAmplitude;
    public float floatFrequency;
    public float scoreCost;
    public List<string> banter;
    public List<string> weakAgainst;
    public List<string> powerfulAgainst;
}

[System.Serializable]
public class ProtectorData
{
    public string type;
    public string name;
    public string description;
    public string category;
    public string iconPath;
    public float resistance;
    public float baseAttack;
    public float speed;
    public float spawnCooldown;
    public float floatAmplitude;
    public float floatFrequency;
    public float scoreCost;
    public List<string> banter;
    public List<string> weakAgainst;
    public List<string> powerfulAgainst;
}

[System.Serializable]
public class SpellData
{
    public string type;
    public string name;
    public string description;
    public string effect;
    public string iconPath;
    public int unlockAt;
    public int cost;
    public List<string> banter;
}

[System.Serializable]
public class VirusDataList
{
    public List<VirusData> viruses;
}

[System.Serializable]
public class ProtectorDataList
{
    public List<ProtectorData> protectors;
}

[System.Serializable]
public class SpellDataList
{
    public List<SpellData> spells;
}

public class EntityDataManager : MonoBehaviour
{
    public static EntityDataManager Instance;

    public TextAsset virusJson;
    public TextAsset protectorJson;
    public TextAsset spellJson;

    private Dictionary<string, VirusData> virusDataDict;
    private Dictionary<string, ProtectorData> protectorDataDict;
    private Dictionary<string, SpellData> spellDataDict;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    void LoadData()
    {
        virusDataDict = new Dictionary<string, VirusData>();
        protectorDataDict = new Dictionary<string, ProtectorData>();
        spellDataDict = new Dictionary<string, SpellData>();

        LoadVirusData();
        LoadProtectorData();
        LoadSpellData();

        //Debug.Log($"EntityDataManager: Loaded {virusDataDict.Count} virus types, {protectorDataDict.Count} protector types, and {spellDataDict.Count} spells.");
    }

    void LoadVirusData()
    {
        if (virusJson == null) return;

        VirusDataList virusList = JsonUtility.FromJson<VirusDataList>(virusJson.text);
        foreach (var v in virusList.viruses)
        {
            if (!string.IsNullOrEmpty(v.iconPath))
            {
                Sprite testSprite = Resources.Load<Sprite>(v.iconPath);
                if (testSprite != null)
                {
                    virusDataDict[v.type] = v;
                }
                else
                {
                    //Debug.LogWarning($"Skipped virus '{v.type}': icon '{v.iconPath}' not found.");
                }
            }
        }
    }

    void LoadProtectorData()
    {
        if (protectorJson == null) return;

        ProtectorDataList protectorList = JsonUtility.FromJson<ProtectorDataList>(protectorJson.text);
        foreach (var p in protectorList.protectors)
        {
            if (!string.IsNullOrEmpty(p.iconPath))
            {
                Sprite testSprite = Resources.Load<Sprite>(p.iconPath);
                if (testSprite != null)
                {
                    protectorDataDict[p.type] = p;
                }
                else
                {
                    //Debug.LogWarning($"Skipped protector '{p.type}': icon '{p.iconPath}' not found.");
                }
            }
        }
    }

    void LoadSpellData()
    {
        if (spellJson == null) return;

        SpellDataList spellList = JsonUtility.FromJson<SpellDataList>(spellJson.text);
        foreach (var s in spellList.spells)
        {

            if (!string.IsNullOrEmpty(s.iconPath))
            {
                Sprite testSprite = Resources.Load<Sprite>(s.iconPath);
                if (testSprite != null)
                {
                    //Debug.Log($"Loading spell: {s.type}");
                    spellDataDict[s.type] = s;
                }
                else
                {
                    //Debug.LogWarning($"Skipped spell '{s.type}': icon '{s.iconPath}' not found.");
                }
            }
            else
            {
                //Debug.LogWarning($"Skipped spell '{s.type}': icon '{s.iconPath}' not found.");
            }
        }
    }

    public VirusData GetVirusData(string type)
    {
        virusDataDict.TryGetValue(type, out var data);
        return data;
    }

    public ProtectorData GetProtectorData(string type)
    {
        protectorDataDict.TryGetValue(type, out var data);
        return data;
    }

    public SpellData GetSpellData(string type)
    {
        //Debug.Log($"Getting spell data for type: {type}");
        spellDataDict.TryGetValue(type, out var data);
        return data;
    }

    public List<string> GetAllVirusTypes() => new List<string>(virusDataDict.Keys);
    public List<ProtectorData> GetAllProtectorData() => new List<ProtectorData>(protectorDataDict.Values);
    public List<SpellData> GetAllSpellData() => new List<SpellData>(spellDataDict.Values);
}
