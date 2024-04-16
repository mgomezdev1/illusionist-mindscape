using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEEffectApplication : MonoBehaviour
{
    public bool canAffectPlayer = true;
    public EffectTemplate effect;
    public float effectDuration = 0.5f;
    public float effectRadius = 1.0f;
    public float reapplicationInterval = 0.4f;
    public bool requireLoS = false;
    [SerializeField] protected LayerMask visionBlocking;
    private float reapplicationTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (reapplicationTime > 0.0f)
        {
            reapplicationTime -= Time.deltaTime;
            return;
        }
        ApplyEffect();
    }

    public void ApplyEffect()
    {
        reapplicationTime = reapplicationInterval;
        if (canAffectPlayer)
        {
            Player p = GameController.Instance.GetPlayerByIndex(0);
            TryApplyToTarget(p);
        }
        foreach (var enemy in HiveMind.Instance.Subjects)
        {
            TryApplyToTarget(enemy);
        }
    }

    public bool TryApplyToTarget(IEffectTarget target)
    {
        Vector3 position = target.EffectTransform.position;
        Vector3 here = transform.position;
        float distSqrd = MathUtils.SqrDistance2D(position, here);
        if (distSqrd > effectRadius * effectRadius)
        {
            return false;
        }
        Vector3 diff = position - here;
        if (requireLoS && Physics.Raycast(here, diff, diff.magnitude, visionBlocking))
        {
            return false;
        }

        target.AddEffect(new Effect(effect, effectDuration));
        return true;
    }
}
