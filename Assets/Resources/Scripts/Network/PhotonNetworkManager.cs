using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonNetworkManager : Photon.PunBehaviour {

	[SerializeField] private Text connectText;
	[SerializeField] private GameObject player;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private GameObject lobbyCammera;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("0.1");
	}

	public virtual void OnJoinedLobby(){
		Debug.Log("Connected to lobby");

		PhotonNetwork.JoinOrCreateRoom("Room1", null, null);
	}

	public virtual void OnJoinedRoom(){
		GameObject obj = PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
		lobbyCammera.GetComponent<CameraFollow>().target = obj.transform;
	}
	
	// Update is called once per frame
	void Update () {
		connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
	}
}
