using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour { 
    [Header("Settings")]
    public float smoothing = 5f;
    public Vector3 offset;

    private Transform target;
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(target!= null){
			//Create a position the camera is aiming for based on the offset from the target
			Vector3 targetCamPos = target.position + offset;

			//Lerp is a smooth interpolation between the camera's current position and its target position
			transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
		}
	}

    public void SetTarget(Transform target) {
        this.target = target;
    }
}
