using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : Photon.MonoBehaviour {


	float lifeTime = 2f;
	// Update is called once per frame
	void Update () {
		lifeTime -= Time.deltaTime;
		if(lifeTime<0)
			PhotonNetwork.Destroy(GetComponent<PhotonView>());
	}

	private void OnCollisionEnter(Collision other) {
		PhotonNetwork.Destroy(GetComponent<PhotonView>());
	}
}
