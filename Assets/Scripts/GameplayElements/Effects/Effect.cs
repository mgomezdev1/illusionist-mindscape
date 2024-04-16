using System;

public class Effect
{
    [Flags]
    public enum Tag
    {
        None = 0,
        Buzzed = 1 << 0, // Can't communicate with hive mind
        Rooted = 1 << 1, // Can't move
        Cursed = 1 << 2, // Can't attack or use abilities
        Blinded = 1 << 3, // Can't see
        Paralyzed = 1 << 1 | 1 << 2, // Rooted and Cursed
        Slowed = 1 << 4, // Movement speed reduced by SlowEffectFactor
        Hastened = 1 << 5, // Movement speed increased by HastenedEffectFactor
    }

    public readonly EffectTemplate template;
    public EffectTemplate Template => template;

    public const float SlowEffectFactor = 0.5f;
    public const float HastenedEffectFactor = 1.6f;
    public const float BlindEffectVisionRange = 0.8f;

    private float duration;
    public float Duration => duration;
    private float timeLeft;
    public float TimeLeft => timeLeft;

    public Effect(EffectTemplate template, float duration)
    {
        this.template = template;
        this.duration = duration;
        this.timeLeft = duration;
    }

    public void Tick(IEffectTarget target, float deltaTime)
    {
        if (IsSuspended(target)) return;
        // Debug.Log($"Ticking effect {effectName}, t={TimeRemaining:F3}/{Duration:F3}");
        timeLeft -= deltaTime;
    }
    public bool HasExpired => timeLeft < 0.0f;

    // Called right after the effect is added to the effects list
    public void Applied(IEffectTarget target)
    {
        template.Applied(target, this);
    }
    // Called right before the effect is removed from the effects list
    public void Removed(IEffectTarget target)
    {
        template.Removed(target, this);
    }

    public bool IsSuspended(IEffectTarget target)
    {
        return template.IsSuspended(target, this);
    }

    public bool HasTag(Tag tag)
    {
        return template.HasTag(tag);
    }
}