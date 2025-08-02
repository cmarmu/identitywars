using UnityEngine;

public class MultiFactorAuthBehaviour : ProtectorCategoryBehaviour
{
    private Virus currentTarget;
    private float maxScale = 2f;
    private int maxAttackPower = 100;
    private bool hasAttacked = false;
    private bool awaitingAnswer = false;
    private Protector protector;

    void Awake()
    {
        protector = GetComponent<Protector>();
    }

    void Start() => FindNewTarget();

    void Update()
    {
        FloatMotion();

        if (currentTarget == null || !IsValidTarget(currentTarget))
        {
            //Debug.Log("ProtectorMFA: Current target invalid or null, finding new target.");
            currentTarget = null;
            hasAttacked = false;
            FindNewTarget();
        }

        if (currentTarget != null)
        {
            MoveToTarget();
        }
    }

    void FindNewTarget()
    {
        Virus[] allViruses = Object.FindObjectsByType<Virus>(FindObjectsSortMode.None);
        float closestDist = float.MaxValue;
        Virus closestVirus = null;

        foreach (Virus v in allViruses)
        {
            if (!IsValidTarget(v)) continue;

            float dist = Vector3.Distance(transform.position, v.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestVirus = v;
            }
        }

        if (closestVirus != null)
        {
            currentTarget = closestVirus;
            hasAttacked = false;
            if (TryGetComponent<Collider2D>(out var collider)) collider.enabled = true;
            //Debug.Log($"ProtectorMFA: New target acquired - {currentTarget.name}");
        }
        else
        {
            //Debug.Log("ProtectorMFA: No valid targets found.");
        }
    }

    void MoveToTarget()
    {
        if (protector == null || currentTarget == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.transform.position,
            protector.speed * Time.deltaTime
        );
    }

    void FloatMotion()
    {
        if (protector == null) return;

        float floatOffset = Mathf.Sin(Time.time * protector.floatFrequency) * protector.floatAmplitude;
        Vector3 newPosition = transform.position + new Vector3(0, floatOffset * Time.deltaTime, 0);

        float spriteHeight = GetComponent<SpriteRenderer>()?.bounds.size.y ?? 0.5f;
        float verticalExtent = Camera.main.orthographicSize;
        float screenMinY = -verticalExtent + spriteHeight / 2;
        float screenMaxY = verticalExtent - spriteHeight / 2;

        float panelTopY = GameManager.Instance.GetPanelButtonContainerTopY();
        float minY = Mathf.Max(screenMinY, panelTopY + spriteHeight / 2);

        newPosition.y = Mathf.Clamp(newPosition.y, minY, screenMaxY);
        transform.position = newPosition;
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasAttacked || awaitingAnswer || currentTarget == null) return;
        if (!other.CompareTag("Virus") || other.gameObject != currentTarget.gameObject) return;

        Virus virus = other.GetComponent<Virus>();
        if (!IsValidTarget(virus) || protector == null) return;

        //Debug.Log("ProtectorMFA: Virus detected!");

        if (Random.value < 0.25f)
        {
            awaitingAnswer = true;
            //Debug.Log("ProtectorMFA: Asking contextual question.");
            QuestionManager.Instance.AskContextualQuestion(
                virus.VirusType,
                protector.ProtectorType,
                virus,
                protector,
                "any",
                (bool correct) => OnQuestionAnswered(virus, correct)
            );
        }
        else
        {
            //Debug.Log("ProtectorMFA: Skipping question. Attacking directly.");
            PerformAttack(virus, false);
        }
    }

    void OnQuestionAnswered(Virus virus, bool correct)
    {
        awaitingAnswer = false;
        //Debug.Log($"ProtectorMFA: Answer received. Correct: {correct}");
        PerformAttack(virus, correct);
    }

    void PerformAttack(Virus virus, bool correct)
    {
        if (!IsValidTarget(virus) || hasAttacked) return;

        hasAttacked = true;
        //Debug.Log("ProtectorMFA: Performing attack.");
        ApplyAttack(virus, correct);
    }

    void ApplyAttack(Virus virus, bool correct)
    {
        if (!IsValidTarget(virus)) return;

        //Debug.Log("ApplyAttack called on Virus: " + virus.name);

        int effectiveAttack = correct ? protector.baseAttack : Mathf.RoundToInt(protector.baseAttack * 0.5f);
        if (effectiveAttack <= 0)
        {
            //Debug.LogWarning("ProtectorMFA: Effective attack is zero or negative, aborting attack.");
            Die();
            return;
        }
        // Calculate resistance based on whether the protector is powerful against the virus type
        int virusResistance = protector.powerfulAgainst.Contains(virus.VirusType)
                ? virus.resistance
                : (int)(virus.resistance / 3f);
        bool virusDefeated = virus.TakeHit(effectiveAttack);

        protector.baseAttack -= virusResistance;
        if (protector.baseAttack < 0) protector.baseAttack = 0;

        //Debug.Log($"ProtectorMFA: Virus hit! Remaining attack power: {protector.baseAttack} (Effective: {effectiveAttack}) - Virus Resistance: {virus.resistance}");

        if (!correct)
        {
            // Reduce protector's resistance based on the virus's attack power
            float resistanceLoss = virus.baseAttack / 10f;

            protector.resistance -= resistanceLoss;

            if (protector.resistance < 0) protector.resistance = 0;

            //Debug.Log($"ProtectorMFA: Resistance reduced by {resistanceLoss}, new resistance: {protector.resistance}");
        }

        if (!virusDefeated || protector.baseAttack <= 0 || protector.resistance <= 0)
        {
            //Debug.Log("ProtectorMFA: Protector is dead or virus survived.");
            Die();
            return;
        }

        if (correct)
        {
            GrowProtector();
            BoostAttackPower();
        }

        currentTarget = null;
        hasAttacked = false;
        //Debug.Log("Attack completed. Virus defeated: " + virusDefeated);
    }


    private void Die()
    {
        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = false;
            //Debug.Log("ProtectorMFA: Collider disabled.");
        }
        protector.PlayDeathEffectAndDestroy();
    }

    private bool IsValidTarget(Virus virus)
    {
        return virus != null && !virus.IsDying;
    }

    private void GrowProtector()
    {
        float growthFactor = 1.2f;
        Vector3 newScale = transform.localScale * growthFactor;

        if (newScale.x <= maxScale)
        {
            transform.localScale = newScale;
            //Debug.Log("Protector grew from correct answer!");
        }
    }

    private void BoostAttackPower()
    {
        if (protector == null) return;

        int bonus = Mathf.CeilToInt(protector.baseAttack * 0.2f);
        protector.baseAttack = Mathf.Min(protector.baseAttack + bonus, maxAttackPower);
        //Debug.Log($"Protector attackPower boosted by {bonus}, new power: {protector.baseAttack}");
    }

    public override void ApplyBehavior(Virus target, Protector protector)
    {
        // Optional: logic triggered when auto-attacking via game design, if used
    }
}
