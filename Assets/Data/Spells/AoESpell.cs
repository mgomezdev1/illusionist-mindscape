using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
[CreateAssetMenu(fileName = "SpellName", menuName = "ScriptableObject/Spells/AoE Spell", order = 1)]
public class AoEEffectSpell : Spell
{
    [SerializeField] protected EffectTemplate spellEffect;
    [SerializeField] protected float effectDuration;
    public EffectTemplate SpellEffect => spellEffect;
    [SerializeField] protected float effectRadius;
    [SerializeField] protected float effectDelay;
    [SerializeField] protected bool requiresLoS;
    [SerializeField] protected LayerMask losBlockingLayers;

    public override void Cast(SpellcastingComponent spellcaster) {

        base.Cast(spellcaster);

        Transform effectCastParent = spellcaster.GetSpellCastTransform(castLocation);
        if (effectDelay == 0)
        {
            ApplyEffectAround(effectCastParent);
        }
        else
        {
            spellcaster.StartCoroutine(DelayAndApplyEffectAround(effectDelay, effectCastParent));
        }
    }
    private IEnumerator DelayAndApplyEffectAround(float time, Transform centerTransform)
    {
        yield return new WaitForSeconds(time);
        ApplyEffectAround(centerTransform);
    }

    public void ApplyEffectAround(Transform centerTransform)
    {
        ApplyEffectAround(centerTransform.position);
    }
    public void ApplyEffectAround(Vector3 centerPosition)
    {
        foreach (Enemy e in HiveMind.Instance.Subjects) {
            if (MathUtils.SqrDistance2D(e.transform.position, centerPosition) > effectRadius * effectRadius) continue;
            Vector3 delta = e.transform.position - centerPosition;
            if (requiresLoS && Physics.Raycast(centerPosition, delta.normalized, delta.magnitude, losBlockingLayers)) continue;
            IEffectTarget target = e;
            target.AddEffect(new Effect(spellEffect, effectDuration));
        } 
    }
}

