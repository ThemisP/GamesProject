﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	[SerializeField] public Transform target;
	public float smoothing = 5f;

	Vector3 offset;
	// Use this for initialization
	void Start () {
		offset = transform.position - target.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(target!= null){
			Debug.Log("1");
			//Create a position the camera is aiming for based on the offset from the target
			Vector3 targetCamPos = target.position + offset;

			//Lerp is a smooth interpolation between the camera's current position and its target position
			transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
		}
	}
}
