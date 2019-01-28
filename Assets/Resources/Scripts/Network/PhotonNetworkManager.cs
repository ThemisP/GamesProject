using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonNetworkManager : Photon.PunBehaviour {

	[SerializeField] private Text connectText;
	[SerializeField] private GameObject player;
	[SerializeField] private GameObject lobbyCammera;
	[SerializeField] private Transform[] teamspawns;
	private PlayerNetwork playerNetwork;
	private List<int> teams;
	// Use this for initialization
	void Start () {	
		PhotonNetwork.LeaveRoom(); // Leaving room from friends lobby!
		teams = new List<int>();
		for (int i=0; i<100; i++){
			teams.Add(i);
		}

		playerNetwork = GameObject.Find("PlayerNetwork").GetComponent<PlayerNetwork>();
		if(playerNetwork == null) Debug.Log("not found");

		string[] expectedPlayers = {playerNetwork.getPlayerId(), playerNetwork.getTeammateId()};
		Debug.Log(expectedPlayers[0] + " " + expectedPlayers[1]);
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.PublishUserId = true;
		roomOptions.MaxPlayers = 8;
		PhotonNetwork.JoinOrCreateRoom("MainGame",roomOptions,new TypedLobby("MainGameLobby", LobbyType.Default), expectedPlayers);
	}

	virtual public void OnCreatedRoom(){
			Debug.Log("Created");
	}

	virtual public void OnJoinedRoom(){
		Debug.Log("in");
		// PhotonNetwork.automaticallySyncScene = false;
		// bool foundTeamMate = false;
		// int teamnumber = 0;
		// foreach(var player in PhotonNetwork.otherPlayers){
		// 	object team;
		// 	PhotonNetwork.player.customProperties.TryGetValue("Team", out team);
			
		// 	Debug.Log(team.GetHashCode());
		// 	if(player.UserId == playerNetwork.getTeammateId()){
		// 		PhotonNetwork.player.CustomProperties.Add("Team", team.GetHashCode());
		// 		playerNetwork.setTeamNumber(team.GetHashCode());
		// 		foundTeamMate = true;
		// 	}
		// }
		// if(!foundTeamMate){
		// 	teamnumber = teams.ToArray()[0];
 		// 	playerNetwork.setTeamNumber(teamnumber);
		// 	PhotonNetwork.player.CustomProperties.Add("Team", teamnumber);
		// }
		Transform spawnPoint = teamspawns[0];
		// if(!foundTeamMate){
		// 	spawnPoint.position += Vector3.right*2;
		// }
		GameObject obj = PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
		lobbyCammera.GetComponent<CameraFollow>().SetTarget(obj.transform);
	}
	
	// Update is called once per frame
	void Update () {
		connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
	}
}
