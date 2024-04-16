using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellUIDisplay : MonoBehaviour
{
    [SerializeField] private SpellcastingComponent spellcaster;
    [SerializeField] private Transform spellSlotHolder;
    [SerializeField] private GameObject spellSlotPrefab;

    private List<SpellSlot> spellSlots = new();

    public void Awake()
    {
        UpdateSpellList();
        spellcaster.OnSpellbookChanged.AddListener(UpdateSpellList);
    }

    public void UpdateSpellList()
    {
        UpdateSpellList(spellcaster.Spellbook);
    }

    public void UpdateSpellList(IList<Spell> newSpells)
    {
        // Remove extra spells
        while (spellSlots.Count > newSpells.Count)
        {
            Destroy(spellSlots[^1].gameObject);
            spellSlots.RemoveAt(spellSlots.Count - 1);
        }
        // Replace incorrect spells
        for (int i = 0; i < spellSlots.Count; i++)
        {
            if (spellSlots[i].Spell != newSpells[i])
            {
                spellSlots[i].Spell = newSpells[i];
            }
        }
        // Add any extra spells to the list
        for (int i = spellSlots.Count; i < newSpells.Count; i++)
        {
            var obj = Instantiate(spellSlotPrefab, spellSlotHolder);
            SpellSlot slot = obj.GetComponent<SpellSlot>();
            slot.Spell = newSpells[i];
            spellSlots.Add(slot);
        }
    }
}
