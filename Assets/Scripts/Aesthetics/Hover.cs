using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    [SerializeField] float moveFrequency = 0.25f;
    [SerializeField] Vector3 moveDelta = Vector3.up;

    private Vector3 startPos;
    void Awake()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPos + moveDelta * (Mathf.Cos(Time.time * moveFrequency * Mathf.PI * 2));
    }
}
