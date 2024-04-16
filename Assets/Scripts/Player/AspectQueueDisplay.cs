using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class AspectQueueDisplay : MonoBehaviour
{
    [SerializeField] private Transform ItemContainer;

    [SerializeField] private GameObject MiasmaPrefab;
    [SerializeField] private GameObject MatterPrefab;
    [SerializeField] private GameObject MotionPrefab;
    [SerializeField] private GameObject MindPrefab;

    private readonly List<AspectCasting> heldAspects = new();

    private bool started = false;
    // Start is called before the first frame update
    void Start()
    {
        if (started) return;
        started = true;
        WipeAspectList();
    }

    public void WipeAspectList()
    {
        Debug.Log($"Wiping aspect list");
        for (int i = ItemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(ItemContainer.GetChild(i).gameObject);
        }
        heldAspects.Clear();
    }
    public void SetAspectList(IList<SpellAspect> aspects)
    {
        if (!started) Start();

        // Remove extra aspects
        while (heldAspects.Count > aspects.Count)
        {
            Debug.LogWarning("Removing held aspect from list");
            Destroy(heldAspects[^1].gameObject);
            heldAspects.RemoveAt(heldAspects.Count - 1);
        }
        // Replace incorrect aspects (should realistically never happen)
        for (int i = 0; i < heldAspects.Count; i++)
        {
            if (heldAspects[i].Aspect != aspects[i])
            {
                Debug.LogWarning($"Replacing held aspect in list at index {i}");
                Destroy(heldAspects[i].gameObject);
                var obj = Instantiate(GetPrefab(aspects[i]), ItemContainer);
                obj.transform.SetSiblingIndex(i);
                heldAspects[i] = obj.GetComponent<AspectCasting>();
            }
        }
        // Add any extra aspects to the list
        for (int i = heldAspects.Count; i < aspects.Count; i++)
        {
            var obj = Instantiate(GetPrefab(aspects[i]), ItemContainer);
            heldAspects.Add(obj.GetComponent<AspectCasting>());
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private GameObject GetPrefab(SpellAspect aspect)
    {
        return (aspect) switch
        {
            SpellAspect.Mind => MindPrefab,
            SpellAspect.Motion => MotionPrefab,
            SpellAspect.Matter => MatterPrefab,
            SpellAspect.Miasma => MiasmaPrefab,
            _ => null
        };
    }
}
