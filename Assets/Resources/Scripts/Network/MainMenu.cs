using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	private GameObject usernameScreen;
	private GameObject roomJoinCreateScreen;
	private TextMeshProUGUI usernameInput;
	private string username;

	void Start(){
		//setting up starting ui
		usernameScreen = GameObject.Find("ChooseUsername");
		if(usernameScreen==null) Debug.Log("username screen not found");
		roomJoinCreateScreen = GameObject.Find("Main");
		if(usernameScreen==null) Debug.Log("room join or create screen not found");
		usernameScreen.SetActive(true);
		roomJoinCreateScreen.SetActive(false);

		//getting necessary fields
		usernameInput = GameObject.Find("UsernameText").GetComponent<TextMeshProUGUI>();
		if(usernameInput==null) Debug.Log("usernameInput not found");
	}

	void ChooseUsername(){
		if(usernameInput.text == null) Debug.Log("Not inputed username");
		username = usernameInput.text;
	}


	public void loadGame(){
		SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
	}
}
