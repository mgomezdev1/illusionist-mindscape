using UnityEngine;

public class Drift : MonoBehaviour
{
    public float forwardSpeed;
    [SerializeField] private LayerMask destroyOnTouch;

    void Update()
    {
        if (GameController.Instance.IsPaused) return;
        transform.position += transform.forward * (forwardSpeed * Time.deltaTime);
        if (Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, 100.0f, destroyOnTouch))
        {
            Destroy(gameObject);
        }
    }
}