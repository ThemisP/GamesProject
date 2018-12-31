using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScript : Photon.Monobehaviour {

    void Update() {}

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.name == "PlayerPrefab(Clone)")
        PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }

}

