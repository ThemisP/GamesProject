using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour { 
    [Header("Settings")]
    public float smoothing = 5f;
    public Vector3 offset;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Transform target;
    private bool shouldShake = false;
    private float shakeMaxDuration = 0.5f;
    private float shakeDuration = 0f;
    // Use this for initialization
    void Start () {
        originalPos = transform.position;
        originalRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if(target!= null){
			//Create a position the camera is aiming for based on the offset from the target
			Vector3 targetCamPos = target.position + offset;
            Vector3 shakeOffset = Vector3.zero;
            if (shouldShake) {
                if(shakeDuration < shakeMaxDuration) {
                    shakeOffset = Random.insideUnitSphere * 1f;
                    shakeDuration += Time.deltaTime;
                } else {
                    shouldShake = false;
                }
            }
			//Lerp is a smooth interpolation between the camera's current position and its target position
			transform.position = Vector3.Lerp(transform.position, targetCamPos + shakeOffset, smoothing * Time.deltaTime);
		}
	}

    public void ShakeCamera() {
        shouldShake = true;
        shakeDuration = 0f;
    }
    public void SetTarget(Transform target) {
        this.target = target;
    }

    public void SetToOriginalPosition() {
        transform.position = originalPos;
        transform.rotation = originalRot;
    }
}
