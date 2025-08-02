using UnityEngine;

public class BehavioralDefenseBehavior : ProtectorCategoryBehaviour
{
    private Protector protector;
    private float suckRange = 3f;
    private float suckSpeed = 3f;

    void Awake()
    {
        protector = GetComponent<Protector>();
    }

    void Update()
    {
        FloatMotion();
        SuckNearbyViruses();
    }

    void FloatMotion()
    {
        if (protector == null) return;

        float floatOffset = Mathf.Sin(Time.time * protector.floatFrequency) * protector.floatAmplitude;
        transform.position += new Vector3(0, floatOffset * Time.deltaTime, 0);
    }

    void SuckNearbyViruses()
    {
        Virus[] allViruses = Object.FindObjectsByType<Virus>(FindObjectsSortMode.None);

        foreach (var virus in allViruses)
        {
            if (virus == null || virus.IsDying) continue;

            float distance = Vector3.Distance(transform.position, virus.transform.position);
            if (distance < suckRange)
            {
                virus.transform.position = Vector3.MoveTowards(
                    virus.transform.position,
                    transform.position,
                    suckSpeed * Time.deltaTime
                );

                if (distance < 0.5f)
                {
                    AttackVirus(virus);
                }
            }
        }
    }

    void AttackVirus(Virus virus)
    {
        if (protector.baseAttack <= 0)
        {
            Die();
            return;
        }

        //Debug.Log($"BehavioralDefense: Attacking virus {virus.name}");
        bool defeated = virus.TakeHit(protector.baseAttack);

        if (!defeated)
        {
            protector.baseAttack -= virus.resistance;
            if (protector.baseAttack <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = false;
        }
        protector.PlayDeathEffectAndDestroy();
    }

    public override void ApplyBehavior(Virus target, Protector protector)
    {
        // Not used in this behavior
    }
}
