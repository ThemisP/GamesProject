using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform bulletSpawn;
	[SerializeField] private float speed = 6f;
	[SerializeField] private Network network;
	Vector3 movement;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;
	float lastShootTime = 0;
	[SerializeField] private float fireRate = 2f;
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private Quaternion realRotation;

	private PlayerData playerData;

	// Use this for initialization
	void Awake () {
		floorMask = LayerMask.GetMask("Floor");
		playerRigidbody = GetComponent<Rigidbody>();
		playerRigidbody.freezeRotation = true;
		playerData = GetComponent<PlayerData>();
	}
	
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		playerRigidbody.MovePosition(Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay));
		playerRigidbody.rotation = Quaternion.Lerp(playerRigidbody.rotation, realRotation, .3f);
	}

	// Update is called once per frame
	void FixedUpdate () {
		if(true){
			float h = Input.GetAxisRaw("Horizontal");// a and d keys
			float v = Input.GetAxisRaw("Vertical"); // w and s keys
			bool fire = Input.GetMouseButton(0);//pressed primary mouse button

			Move(h,v);
			Turning();
			Fire(fire);
		} else {
			SyncedMovement();
		}
	}

	//void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
	//	if (stream.isWriting) {
	//		stream.SendNext(playerRigidbody.position);
	//		stream.SendNext(playerRigidbody.velocity);
	//		stream.SendNext(playerRigidbody.rotation);
	//	}
 //   	else {
 //       	Vector3 syncPosition = (Vector3)stream.ReceiveNext();
 //       	Vector3 syncVelocity = (Vector3)stream.ReceiveNext();
	//		realRotation = (Quaternion)stream.ReceiveNext();
 
	//		syncTime = 0f;
	//		syncDelay = Time.time - lastSynchronizationTime;
	//		lastSynchronizationTime = Time.time;
	
	//		syncEndPosition = syncPosition + syncVelocity * syncDelay;
	//		syncStartPosition = playerRigidbody.position;
 //   	}
	//}

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
				Network.instance.SendBullet(bulletSpawn.position, bulletSpawn.rotation.eulerAngles, 2f, 2f);
				lastShootTime = Time.fixedTime;
			}
		} 
	}

}
