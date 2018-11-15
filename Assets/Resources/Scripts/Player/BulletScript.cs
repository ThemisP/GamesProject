using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : Photon.MonoBehaviour {

	int bulletDamage = 10;
	float lifeTime = 2f;

	Rigidbody rigidbody;
	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.velocity = this.transform.forward * 10f;
	}

	void Update () {
		lifeTime -= Time.deltaTime;
		if(lifeTime<0)
			Destroy(this);
			// photonView.RPC("BulletDestroy", PhotonTargets.All, null);
	}

	//procedure call on server
	// [PunRPC]
	// void BulletDestroy(){
	// 	Destroy(this);
	// }

	private void OnTriggerEnter(Collider collision) {
		GameObject obj = collision.gameObject;
		Debug.Log("triggered");
		if(obj.tag == "Player"){
			PlayerData data = obj.GetComponent<PlayerData>();
			if(data!=null) data.takeDamage(bulletDamage);
		}
		// photonView.RPC("BulletDestroy", PhotonTargets.All, null);
	}
}
