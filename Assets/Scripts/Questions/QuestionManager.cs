using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance;

    public GameObject panelPrefab;
    public GameObject questionUIPrefab;

    private Dictionary<string, List<QuestionEntry>> questionMap = new();
    private System.Action<bool> onAnsweredCallback;
    private bool questionActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        LoadQuestions();
    }

    void LoadQuestions()
    {
        questionMap.Clear();

        TextAsset[] questionFiles = Resources.LoadAll<TextAsset>("Questions");
        foreach (var file in questionFiles)
        {
            QuestionFile parsedFile = JsonUtility.FromJson<QuestionFile>(file.text);
            if (parsedFile != null && parsedFile.questions != null && parsedFile.questions.Count > 0)
            {
                string key = parsedFile.category.ToLower();
                questionMap[key] = parsedFile.questions;
                //Debug.Log($"QuestionManager: Loaded {parsedFile.questions.Count} questions for tag '{key}'");
            }
            else
            {
                //Debug.LogWarning($"QuestionManager: No valid questions in file {file.name}");
            }
        }
    }


    public void AskContextualQuestion(
        string virusType,
        string protectorType,
        Virus virus,
        Protector protector,
        string level = "any",
        System.Action<bool> callback = null)
    {
        if (questionActive)
        {
            //Debug.Log("Question already active, skipping new one.");
            return;
        }
        questionActive = true;
        onAnsweredCallback = callback;

        List<QuestionEntry> questions = new();

        void TryAdd(string category)
        {
            if (questionMap.TryGetValue(category.ToLower(), out var list))
                questions.AddRange(list);
        }

        TryAdd(virusType);
        if (protectorType != virusType)
            TryAdd(protectorType);

        TryAdd($"{virusType}|{protectorType}");
        TryAdd($"{protectorType}|{virusType}");

        if (questions.Count == 0)
        {
            //Debug.LogWarning($"No questions found for types: {virusType}, {protectorType}");
            callback?.Invoke(false);
            questionActive = false;
            return;
        }

        if (level.ToLower() != "any")
            questions = questions.FindAll(q => q.level.ToLower() == level.ToLower());

        if (questions.Count == 0)
        {
            //Debug.LogWarning("No matching questions after difficulty filter.");
            callback?.Invoke(false);
            questionActive = false;
            return;
        }

        QuestionEntry entry = questions[Random.Range(0, questions.Count)];

        Time.timeScale = 0;
        panelPrefab.SetActive(true);

        //Debug.Log($"Asking question: {entry.question}");
        foreach (Transform child in panelPrefab.transform)
            Destroy(child.gameObject);

        var ui = Instantiate(questionUIPrefab, panelPrefab.transform).GetComponent<QuestionUI>();
        ui.Init(entry, protector, virus);
    }

    public void OnAnswerSelected(bool correct)
    {
        onAnsweredCallback?.Invoke(correct);
        onAnsweredCallback = null;

        Time.timeScale = 1;
        panelPrefab.SetActive(false);
        questionActive = false;

        if (correct)
            GameManager.Instance.AddAuthTokens(20);
    }

    public List<string> GetAllQuestionTags()
    {
        return new List<string>(questionMap.Keys);
    }
}
