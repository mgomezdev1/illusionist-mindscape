using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUIManager : MonoBehaviour
{
    private Enemy enemy;
    [SerializeField] private Image awarenessBg;
    [SerializeField] private Image awarenessMeter;
    [SerializeField] private Image searchTimerBg;
    [SerializeField] private Image searchTimer;
    [SerializeField] private GameObject awarenessHolder;
    [SerializeField] private GameObject searchTimerHolder;

    [Header("Colors")]
    [SerializeField] private Gradient awarenessGradient;
    [SerializeField] private Gradient searchGradient;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public void SetAwareness(float awareness)
    {
        if (enemy.Searching) { return; }
        float awarenessHolderOpacity = Mathf.Clamp01(awareness * 4.0f);
        awarenessBg.color = awarenessBg.color.WithAlpha(awarenessHolderOpacity);
        awarenessMeter.fillAmount = awareness;
        awarenessMeter.color = awarenessGradient.Evaluate(awareness);
    }

    public void SetSearchTimer(float searchTimeLeft, float maxSearchTime)
    {
        if (!enemy.Searching) { return; }
        float fraction = searchTimeLeft / maxSearchTime;
        searchTimer.fillAmount = fraction;
        searchTimer.color = searchGradient.Evaluate(fraction);
    }

    public void Update()
    {
        if (enemy.Searching)
        {
            if (!searchTimerHolder.activeSelf)
            {
                searchTimerHolder.SetActive(true);
                awarenessHolder.SetActive(false);
            }
            SetSearchTimer(enemy.SearchTimeLeft, enemy.SearchTimeMax);
        }
        else
        {
            if (!awarenessHolder.activeSelf)
            {
                awarenessHolder.SetActive(true);
                searchTimerHolder.SetActive(false);
            }
            SetAwareness(enemy.Vision.Awareness);
        }
    }
}
