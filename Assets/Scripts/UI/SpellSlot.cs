using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
public class SpellSlot : MonoBehaviour
{
    [SerializeField] Image? mainSpellImage;
    [SerializeField] AspectQueueDisplay aspectHolder;
    [SerializeField] TMP_Text? spellNameText;
    [SerializeField] TMP_Text? spellDescriptionText;

    private Spell? spell = null;
    public Spell? Spell { get => spell; set => SetSpell(value); }
    public void SetSpell(Spell? spell)
    {
        this.spell = spell;
        if (spell == null)
            return;

        if (mainSpellImage != null) 
            mainSpellImage.sprite = spell.SpellSprite;
        if (spellNameText != null)
            spellNameText.text = spell.SpellName;
        if (spellDescriptionText != null)
            spellDescriptionText.text = spell.SpellDescription;
        aspectHolder.SetAspectList(spell.Aspects);
    }
}