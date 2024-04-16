using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, IEffectTarget
{
    public SpellcastingComponent Spellcasting { get; set; }
    public PlayerMovement Movement { get; set; }

    private readonly List<Effect> effects = new();
    public List<Effect> Effects => effects;

    public Transform EffectTransform => transform;
    private int playerIndex = -1;
    public int PlayerIndex => playerIndex;

    public UnityEvent OnDeath = new();
    private bool dead = false;
    public bool IsDead => dead;

    void Awake()
    {
        Spellcasting = GetComponent<SpellcastingComponent>();
        Movement = GetComponent<PlayerMovement>();
        playerIndex = GameController.Instance.RegisterPlayer(this);
    }

    public bool HasEffect(Effect.Tag tag)
    {
        return AsEffectTarget().IsAffectedBy(tag);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.IsPaused) return;
        AsEffectTarget().TickEffects(Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Damage"))
        {
            dead = true;
            OnDeath.Invoke();
        }
        if (other.CompareTag("Victory"))
        {
            GameController.Instance.TriggerVictory();
        }
    }

    public IEffectTarget AsEffectTarget()
    {
        return this;
    }
}
