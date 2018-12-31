using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScript : Photon.MonoBehaviour {
    float lifetime = 2f;
    private void Update() {
        lifetime = Time.deltaTime;
        if(lifetime % 5.0 == 0) { }
            

    }



    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.name == "PlayerPrefab(Clone)")
            PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }
}

