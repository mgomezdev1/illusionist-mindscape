using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
public class EffectHolder : MonoBehaviour
{
    private Effect? effect;
    public Effect? Effect => effect;
    [SerializeField] private Image effectBg;
    [SerializeField] private Image effectTimer;

    [SerializeField] private Color bgColor;

    // Start is called before the first frame update
    void Start()
    {
        // effectBg.color = Color.clear;
        // effectTimer.color = Color.clear;
    }

    // Update is called once per frame
    void Update()
    {
        if (effect == null) return;
        effectTimer.fillAmount = effect.TimeLeft / effect.Duration;
    }

    public void SetEffect(Effect effect)
    {
        this.effect = effect;
        effectBg.color = bgColor;
        effectTimer.color = Color.white;
        effectBg.sprite = effect.Template.effectSprite;
        effectTimer.sprite = effect.Template.effectSprite;
    }
}
