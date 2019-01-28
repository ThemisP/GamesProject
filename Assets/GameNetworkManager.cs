using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNetworkManager : Photon.PunBehaviour {

	private string username;
	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("v1");
	}

	public bool chooseUsername(string name){
		if(string.IsNullOrEmpty(name)) return false;
		username = name;
		return true;
	}

	public bool createRoom(string name){
		if(string.IsNullOrEmpty(name)) return false;
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 2;
		PhotonNetwork.CreateRoom(name, roomOptions, TypedLobby.Default);
		return true;
	}

	public bool joinRoom(string name){
		if(string.IsNullOrEmpty(name)) return false;
		PhotonNetwork.JoinRoom(name);
		return true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
