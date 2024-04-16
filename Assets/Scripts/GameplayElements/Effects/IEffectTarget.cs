using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IEffectTarget
{
    List<Effect> Effects { get; }
    Transform EffectTransform { get; }
    void TickEffects(float deltaTime)
    {
        foreach (Effect effect in Effects)
        {
            effect.Tick(this, deltaTime);
        }
        
        Effects.RemoveAll(e =>
        {
            if (e.HasExpired)
            {
                e.Removed(this);
                return true;
            }
            return false;
        });
    }
    bool IsAffectedBy(Effect.Tag effectTag)
    {
        foreach (Effect effect in Effects)
        {
            if (effect.IsSuspended(this) || effect.HasExpired) continue;
            if (effect.HasTag(effectTag)) return true;
        }
        return false;
    }
    float GetSpeed(float baseSpeed)
    {
        return baseSpeed * GetSpeedFactor();
    }
    float GetSpeedFactor()
    {
        bool slowed = false;
        bool hastened = false;
        foreach (Effect effect in Effects)
        {
            if (effect.HasTag(Effect.Tag.Rooted)) return 0;
            if (!slowed && effect.HasTag(Effect.Tag.Slowed))
            {
                slowed = true;
            }
            if (!hastened && effect.HasTag(Effect.Tag.Hastened))
            {
                hastened = true;
            }
        }
        return (slowed ? Effect.SlowEffectFactor : 1) * (hastened ? Effect.HastenedEffectFactor : 1);
    }
    void ClearEffect(Effect effect)
    {
        effect.Removed(this);
        Effects.Remove(effect);
    }
    int ClearEffectById(string effectId)
    {
        return Effects.RemoveAll(e =>
        {
            if (e.Template.Id == effectId)
            {
                e.Removed(this);
                return true;
            }
            return false;
        });
    }
    void AddEffect(Effect effect)
    {
        if (effect.Template.Id != "") ClearEffectById(effect.Template.Id);
        Effects.Add(effect);
        Debug.Log($"Applied effect {effect.Template.name} to {EffectTransform.gameObject.name}");
        effect.Applied(this);
    }
}