using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Camera? cameraToFace;

    // Update is called once per frame
    void Update()
    {
        Camera? target = cameraToFace != null ? cameraToFace : Camera.main;
        transform.LookAt(target.transform);
    }
}
