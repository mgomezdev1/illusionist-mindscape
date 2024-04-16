using System;
using UnityEngine;


[CreateAssetMenu(fileName = "EffectName", menuName = "ScriptableObject/Effects/SimpleEffect", order = 0)]
public class EffectTemplate : ScriptableObject
{
    [SerializeField] private Effect.Tag effectTag = Effect.Tag.None;
    public Effect.Tag EffectTag { get => effectTag; set => effectTag = value; }
    [SerializeField] private string id = "";
    public string Id => id;

    [Header("Presentation")]
    public Sprite effectSprite;
    public string effectName;
    public string effectDescription;

    public virtual void Tick(IEffectTarget target, Effect instance, float deltaTime)
    {
        
    }

    // Called right after the effect is added to the effects list
    public virtual void Applied(IEffectTarget target, Effect instance) { }
    // Called right before the effect is removed from the effects list
    public virtual void Removed(IEffectTarget target, Effect instance)
    {
        Debug.Log($"Removed effect {effectName}, from {target.EffectTransform.name}");
    }

    public virtual bool IsSuspended(IEffectTarget target, Effect instance)
    {
        return false;
    }

    public bool HasTag(Effect.Tag tag)
    {
        return (effectTag & tag) == tag;
    }
}