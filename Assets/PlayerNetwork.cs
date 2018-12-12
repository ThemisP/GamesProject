using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour {

	private string username;

	void Awake(){
		DontDestroyOnLoad(this);
	}

	public void setUsername(string name){
		this.username = name;
	}

	public string getUsername(){
		return username;
	}
}
