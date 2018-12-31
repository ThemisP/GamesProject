using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : Photon.MonoBehaviour {

	float bulletDamage = 10;
	float lifeTime = 2f;
    bool dodged = false;
    PlayerController control;

	Rigidbody rigidbody;
	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.velocity = this.transform.forward * 10f;
	}

	void Update(){
		if(photonView.isMine){
			lifeTime -= Time.deltaTime;
			if(lifeTime < 0)
				PhotonNetwork.Destroy(photonView);
		}	
	}

	private void OnTriggerEnter(Collider collision) {
		if(photonView.isMine){
			GameObject obj = collision.gameObject;
			Debug.Log("triggered");
			if(obj.tag == "Player"){
				PhotonView targetView = obj.GetPhotonView();
                control = GetComponent<PlayerController>();
                dodged = control.dodge;
                if (targetView != null && !dodged);
					targetView.RPC("takeDamage", PhotonTargets.All, this.bulletDamage);
				
			}
			PhotonNetwork.Destroy(photonView);
		}
		
	}
}
