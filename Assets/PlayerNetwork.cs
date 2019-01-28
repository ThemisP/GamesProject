using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : Photon.MonoBehaviour {

	private string playerId;
	private string teammateId;

	private int teamNumber;

	void Awake(){
		DontDestroyOnLoad(this);
	}

	public void setPlayer(string player){
		this.playerId = player;
	}
	public void setTeammate(string player){
		this.teammateId = player;
	}

	public void setTeamNumber(int number){
		this.teamNumber = number;
	}

	public string getPlayerId(){
		return this.playerId;
	}

	public string getTeammateId(){
		return this.teammateId;
	}
}
