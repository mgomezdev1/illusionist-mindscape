using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellAspect
{
    Miasma,
    Mind,
    Matter,
    Motion
}

#nullable enable
[CreateAssetMenu(fileName = "SpellName", menuName = "ScriptableObject/Spells/BaseSpell", order = 0)]
public class Spell : ScriptableObject
{
    [SerializeField] protected EffectTemplate? applyToCasterImmediately;
    [SerializeField] protected float casterEffectDuration;
    [SerializeField] protected Sprite spellSprite;
    public Sprite SpellSprite => spellSprite;
    [SerializeField] protected string spellName;
    [TextArea]
    [SerializeField] protected string spellDescription;
    public string SpellName { get { return spellName; } }
    public string SpellDescription { get { return spellDescription; } }
    [SerializeField] protected SpellAspect[] aspects;
    public SpellAspect[] Aspects { get { return aspects; } }
    [SerializeField] protected GameObject? vfxPrefab;
    [SerializeField] protected SpellcastingComponent.SpellCastLocation castLocation = SpellcastingComponent.SpellCastLocation.Ground;


    public virtual void Cast(SpellcastingComponent spellcaster) {
        if (applyToCasterImmediately != null)
        {
            IEffectTarget casterEffectTarget = spellcaster.Player;
            Debug.Log($"Applying {applyToCasterImmediately.name} to {casterEffectTarget.EffectTransform.name}");
            casterEffectTarget.AddEffect(new Effect(applyToCasterImmediately, casterEffectDuration));
        }

        Transform effectCastParent = spellcaster.GetSpellCastTransform(castLocation);
        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, effectCastParent);
        }
    }
}
