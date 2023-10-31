using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public float smoothTime = 0.25f;

    private Vector3 velocity = Vector3.zero;

    void Start() { }

    void LateUpdate() {
        Vector3 camPosition = target.transform.position;
        camPosition.z = transform.position.z;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, camPosition, ref velocity, smoothTime);
        transform.position = newPos;
    }
}