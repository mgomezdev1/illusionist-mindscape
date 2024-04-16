using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellName", menuName = "ScriptableObject/Spells/Channeled Spell", order = 2)]
public class ChanneledSpell : Spell
{
    public override void Cast(SpellcastingComponent spellcaster) {
        base.Cast(spellcaster);
        spellcaster.StartChanneling(this);
    }
    public virtual void Channel(SpellcastingComponent spellcaster) { }
    public virtual void StopChannel(SpellcastingComponent spellcaster) { }
}
