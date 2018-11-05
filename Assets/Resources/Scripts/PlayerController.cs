﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Photon.MonoBehaviour {

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform bulletSpawn;
	[SerializeField] private float speed = 6f;
	Vector3 movement;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;
	PhotonView photonView;
	float lastShootTime = 0;
	[SerializeField] private float fireRate = 2f;

	// Use this for initialization
	void Awake () {
		floorMask = LayerMask.GetMask("Floor");
		playerRigidbody = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(photonView.isMine){
			float h = Input.GetAxisRaw("Horizontal");// a and d keys
			float v = Input.GetAxisRaw("Vertical"); // w and s keys
			bool fire = Input.GetMouseButton(0);//pressed primary mouse button

			Move(h,v);
			Turning();
			Fire(fire);
		}
	}

	void Move(float h, float v){
		movement.Set(h, 0f, v);
		//Normalise the movement vector to make it proportional to the speed per second
		//Deltatime is the step for the game timer
		movement = movement.normalized * speed * Time.deltaTime;
		playerRigidbody.MovePosition(transform.position + movement);
	}

	void Turning(){
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		RaycastHit floorHit;

		if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask)){
			Vector3 playerToMouse = floorHit.point - transform.position;

			playerToMouse.y = 0f;

			Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

			playerRigidbody.MoveRotation(newRotation);
		}
	}

	void Fire(bool fire){
		if(fire){
			if(lastShootTime+fireRate<Time.fixedTime){
				GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

				bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6f;

				Destroy(bullet, 2.0f);
				lastShootTime = Time.fixedTime;
			}
		} 
	}
}