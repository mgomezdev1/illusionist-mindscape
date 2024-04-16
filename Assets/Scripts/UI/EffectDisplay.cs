using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDisplay : MonoBehaviour
{
    [SerializeField] private Transform effectContainer;
    [SerializeField] private GameObject effectHolderPrefab;
    [SerializeField] private GameObject effectTargetObject;
    private IEffectTarget effectTarget;
    private readonly List<EffectHolder> holders = new();

    private void Awake()
    {
        effectTarget = effectTargetObject.GetComponent<IEffectTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        List<Effect> activeEffects = effectTarget.Effects; 
        for (int i = 0; i < activeEffects.Count; i++) { 
            if (i >= holders.Count)
            {
                GameObject newHolder = Instantiate(effectHolderPrefab, effectContainer);
                holders.Add(newHolder.GetComponent<EffectHolder>());
            }
            if (activeEffects[i] == holders[i].Effect) continue;
            holders[i].SetEffect(activeEffects[i]);
        }
        while (holders.Count > activeEffects.Count)
        {
            EffectHolder lastHolder = holders[^1];
            Destroy(lastHolder.gameObject);
            holders.RemoveAt(holders.Count - 1);
        }
    }
}
