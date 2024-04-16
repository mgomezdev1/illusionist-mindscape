using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AspectCasting : MonoBehaviour
{
    [SerializeField] private float fadeInTime;
    [SerializeField] private AudioClip castSound;
    [SerializeField] private SpellAspect aspect;
    public SpellAspect Aspect => aspect;
    private Image image;
    private float time;

    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<Image>();
        if (castSound != null)
        {
            SoundManager.Instance.PlayGlobal(castSound, SoundManager.Channel.SFX);
        }
    }

    private void Update()
    {
        if (time < fadeInTime)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Min(time / fadeInTime, 1);
            image.color = image.color.WithAlpha(alpha);
        }

    }
}
