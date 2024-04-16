using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class SpellcastingComponent : MonoBehaviour
{
    public enum SpellCastLocation
    {
        Hand,
        Center,
        Ground
    }

    public Player Player { get; private set; }
    [SerializeField]
    private List<Spell> spellbook = new List<Spell>();
    public List<Spell> Spellbook { get { return spellbook; } }
    private List<SpellAspect> castAspects = new List<SpellAspect>();
    private ChanneledSpell? channeledSpell = null;

    public UnityEvent<List<SpellAspect>> OnAspectConjured = new();
    public UnityEvent OnAspectsReleased = new();
    public UnityEvent<List<Spell>> OnSpellbookChanged = new();

    private bool casting = false;
    public bool Casting => casting;
    public bool Channeling => channeledSpell != null;

    [Header("CastPositions")]
    public Transform groundPosition;
    public Transform handPosition;

    [Header("Media")]
    public AudioClip SpellFailClip;
    public AudioClip SpellLearnedClip;
    public AudioClip OblivionClip;

    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.IsPaused) return;

        casting = Input.GetButton("Cast");
        bool releaseCast = Input.GetButtonUp("Cast");
        // If channeling...
        if (channeledSpell is not null)
        {
            // Keep channeling
            channeledSpell.Channel(this);
            if (casting) // Until you attempt to try to cast a new spell
            {
                channeledSpell.StopChannel(this);
            }
        }

        bool cursed = Player.HasEffect(Effect.Tag.Cursed);
        if (casting && !cursed)
        {
            if (Input.GetButtonDown("Miasma"))
            {
                AddCastAspect(SpellAspect.Miasma);
            }
            if (Input.GetButtonDown("Mind"))
            {
                AddCastAspect(SpellAspect.Mind);
            }
            if (Input.GetButtonDown("Matter"))
            {
                AddCastAspect(SpellAspect.Matter);
            }
            if (Input.GetButtonDown("Motion"))
            {
                AddCastAspect(SpellAspect.Motion);
            }
        }
        else if (releaseCast)
        {
            TryCastAndClear();
        } 
    }

    public void AddCastAspect(SpellAspect aspect)
    {
        castAspects.Add(aspect);
        OnAspectConjured.Invoke(castAspects);
    }

    public void TryCastAndClear()
    {
        if (!castAspects.Any())
        {
            return; // No aspects were added
        }

        // Check if it matches a spell
        bool anyCast = false;
        foreach (Spell spell in spellbook)
        {
            if (castAspects.Count != spell.Aspects.Length) continue;
            bool matches = true;
            for (int i = 0; i < spell.Aspects.Length; i++)
            {
                if (spell.Aspects[i] != castAspects[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                anyCast = true;
                CastSpell(spell);
                break;
            }
        }
        if (!anyCast) {
            CastFail();
        }
        castAspects.Clear();
        OnAspectsReleased.Invoke();
    }

    public void CastSpell(Spell spell)
    {
        spell.Cast(this);
    }
    public void CastFail()
    {
        SoundManager.Instance.PlayGlobal(SpellFailClip, volumeFactor: 2.0f);
    }

    public void StartChanneling(ChanneledSpell spell)
    {
        channeledSpell = spell;
    }
    public void StopChanneling()
    {
        channeledSpell = null;
    }

    public Transform GetSpellCastTransform(SpellCastLocation spellCastLocation)
    {
        return spellCastLocation switch
        {
            SpellCastLocation.Hand => handPosition,
            SpellCastLocation.Ground => groundPosition,
            _ => transform
        };
    }

    public void ObliviateSpellbook()
    {
        if (Spellbook.Count == 0) return;
        Spellbook.Clear();
        OnSpellbookChanged.Invoke(Spellbook);
        SoundManager.Instance.PlayGlobal(OblivionClip);
    }

    public void LearnSpell(Spell s)
    {
        if (Spellbook.Contains(s)) return;
        Spellbook.Add(s);
        OnSpellbookChanged.Invoke(Spellbook);
        SoundManager.Instance.PlayGlobal(SpellLearnedClip);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oblivion"))
        {
            ObliviateSpellbook();
        }
        if (other.CompareTag("Scroll")) 
        {
            if (other.TryGetComponent<SpellScroll>(out var scroll))
            {
                LearnSpell(scroll.Spell);
            }
        }
    }
}
