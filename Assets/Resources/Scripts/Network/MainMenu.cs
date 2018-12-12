using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Photon.PunBehaviour {

	[SerializeField] private GameObject usernameScreenMenu;
	[SerializeField] private GameObject MainScreenMenu;
	[SerializeField] private GameObject JoinRoomMenu;
	[SerializeField] private GameObject CreateRoomMenu;

	[SerializeField] private GameObject LobbyRoomMenu;
	[SerializeField] private TMP_InputField usernameInput;
	[SerializeField] private TMP_InputField createRoomName;
	[SerializeField] private TMP_InputField joinRoomName;
	[SerializeField] private TMP_Text usernameDisplay;
	[SerializeField] private TMP_Text LobbyTitle;
	[SerializeField] private TMP_Text player1Display;
	[SerializeField] private TMP_Text player2Display;

	[SerializeField] private Button joinMainGameFromLobbyButton;

	[SerializeField] private PlayerNetwork playerNetwork;


	void Awake(){
		usernameScreenMenu.SetActive(true);
		MainScreenMenu.SetActive(false);
		JoinRoomMenu.SetActive(false);
		CreateRoomMenu.SetActive(false);
		LobbyRoomMenu.SetActive(false);

		PhotonNetwork.ConnectUsingSettings("v1");
	}

	public void ChooseUsername(){
		string name = usernameInput.text;
		if(string.IsNullOrEmpty(name)) {
			Debug.Log("Field is empty");
			return;
		}
		playerNetwork.setUsername(name);
		PhotonNetwork.playerName  = name;
		
		usernameScreenMenu.SetActive(false);
		MainScreenMenu.SetActive(true);
		JoinRoomMenu.SetActive(false);
		CreateRoomMenu.SetActive(false);
		LobbyRoomMenu.SetActive(false);
		usernameDisplay.text = name;
	}

	public void CreateRoom(){
		string name = createRoomName.text;
		if(string.IsNullOrEmpty(name)) {
			Debug.Log("Username field empty");
			return;
		}
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 2;
		PhotonNetwork.CreateRoom(name, roomOptions, TypedLobby.Default);		
	}

	virtual public void OnCreatedRoom(){
		Debug.Log("Created Room");
	}
	
	virtual public void OnPhotonCreateRoomFailed(object[] codeAndMsg){
		Debug.Log(codeAndMsg[0]);
	}

	public void JoinRoom(){
		string name = joinRoomName.text;
		PhotonNetwork.JoinRoom(name);
	}

	virtual public void OnJoinedRoom(){
			usernameScreenMenu.SetActive(false);
			MainScreenMenu.SetActive(false);
			JoinRoomMenu.SetActive(false);
			CreateRoomMenu.SetActive(false);
			LobbyRoomMenu.SetActive(true);
			if(PhotonNetwork.otherPlayers.Length <1){
				joinMainGameFromLobbyButton.interactable = true;
				player1Display.text = PhotonNetwork.playerName;
				LobbyTitle.text = "(" + PhotonNetwork.room.Name + ") Lobby";
			} else {
				joinMainGameFromLobbyButton.interactable = false;
				player1Display.text = PhotonNetwork.otherPlayers[0].NickName;
				player2Display.text = PhotonNetwork.playerName;
				LobbyTitle.text = "(" + PhotonNetwork.room.Name + ") Lobby";
			}
			
	}

	virtual public void OnPhotonJoinRoomFailed(object[] codeAndMsg){
		Debug.Log(codeAndMsg[0]);
	}


	public void loadGame(){
		SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
	}
}
