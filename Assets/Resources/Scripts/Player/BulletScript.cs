using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : Photon.MonoBehaviour {

	float bulletDamage = 10;
	float lifeTime = 2f;

	Rigidbody rigidbody;
	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.velocity = this.transform.forward * 10f;
	}

	//procedure call on server
	[PunRPC]
	void BulletDestroy(){
		// if(photonView.isMine)
		// 	Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider collision) {
		GameObject obj = collision.gameObject;
		Debug.Log("triggered");
		if(obj.tag == "Player"){
			PhotonView targetView = obj.GetPhotonView();
			if(targetView != null)
				targetView.RPC("takeDamage", PhotonTargets.All, this.bulletDamage);
			
		}
		photonView.RPC("BulletDestroy", PhotonTargets.All, null);
	}
}
