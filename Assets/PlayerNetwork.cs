using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : Photon.MonoBehaviour {

	private string playerId;
	private string teammateId;

	void Awake(){
		DontDestroyOnLoad(this);
	}

	public void setPlayer(string player){
		this.playerId = player;
	}
	public void setTeammate(string player){
		this.teammateId = player;
	}
}
