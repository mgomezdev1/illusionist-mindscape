using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] float lifetime;
    float timeLived;
    [SerializeField] bool countWhileGamePaused = false;
    [SerializeField] bool detachSelfOnAwake = true;

    private void Awake()
    {
        if (detachSelfOnAwake)
        {
            transform.SetParent(null, true);
        }
    }

    private void Update()
    {
        if (!countWhileGamePaused && GameController.Instance.IsPaused) return;
        
        timeLived += Time.deltaTime;
        if (timeLived > lifetime)
        {
            Destroy(gameObject);
        }
    }
}