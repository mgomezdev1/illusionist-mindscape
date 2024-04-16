using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
[CreateAssetMenu(fileName = "SpellName", menuName = "ScriptableObject/Spells/Summon Spell", order = 2)]
public class SummonSpell : Spell
{
    [SerializeField] protected GameObject summonPrefab;
    [SerializeField] protected float summonDelay;
    [SerializeField] protected SpellcastingComponent.SpellCastLocation summonLocation = SpellcastingComponent.SpellCastLocation.Ground;

    public override void Cast(SpellcastingComponent spellcaster)
    {
        base.Cast(spellcaster);
        spellcaster.StartCoroutine(Summon(spellcaster.GetSpellCastTransform(summonLocation)));
    }

    public IEnumerator Summon(Transform parent)
    {
        yield return new WaitForSeconds(summonDelay);
        Instantiate(summonPrefab, parent.position, Quaternion.LookRotation(parent.forward), parent);
    }
}
