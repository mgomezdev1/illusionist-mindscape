using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScroll : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Spell spell;
    public Spell Spell => spell;
    [SerializeField]
    private bool isGrimoire;
    
    public void Interact()
    {
        throw new System.NotImplementedException();
    }
}
