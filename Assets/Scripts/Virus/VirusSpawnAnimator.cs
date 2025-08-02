using System.Collections;
using UnityEngine;

public class VirusSpawnAnimator : MonoBehaviour
{
    public float spawnDuration = 0.5f;
    public string virusType = "phishing"; // Default, can be overridden before spawn

    void Start()
    {
        //Debug.Log("VirusSpawnAnimator started for: " + gameObject.name);

        var data = EntityDataManager.Instance.GetVirusData(virusType);
        if (data == null)
        {
            //Debug.LogError($"No virus data found for type: {virusType}");
            return;
        }

        // Initialize the virus behavior with data
        Virus virus = GetComponent<Virus>();
        if (virus != null)
        {
            virus.Init(data);
        }

        StartCoroutine(SpawnAnimation());
    }

    IEnumerator SpawnAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        float time = 0f;
        while (time < spawnDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, time / spawnDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        Virus movement = GetComponent<Virus>();
        if (movement != null)
        {
            movement.StartMoving();
        }
    }
}
