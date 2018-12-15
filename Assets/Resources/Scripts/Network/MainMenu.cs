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
	[SerializeField] private InputField createRoomName;
	[SerializeField] private InputField joinRoomName;
	[SerializeField] private TMP_Text usernameDisplay;
	[SerializeField] private TMP_Text LobbyTitle;
	[SerializeField] private TMP_Text player1Display;
	[SerializeField] private TMP_Text player2Display;

	[SerializeField] private Button joinMainGameFromLobbyButton;

	[SerializeField] private PlayerNetwork playerNetwork;

	private bool inLobby = false;


	void Awake(){
		usernameScreenMenu.SetActive(true);
		MainScreenMenu.SetActive(false);
		JoinRoomMenu.SetActive(false);
		CreateRoomMenu.SetActive(false);
		LobbyRoomMenu.SetActive(false);

		PhotonNetwork.ConnectUsingSettings("v1");
		playerNetwork.setPlayer(PhotonNetwork.player.UserId);
	}

	void Start(){
		InvokeRepeating("SlowUpdate", 0.0f, 0.2f);
	}

	void SlowUpdate(){
		if(inLobby){
			if(PhotonNetwork.otherPlayers.Length>0){
				player2Display.text = PhotonNetwork.otherPlayers[0].NickName;
			} else {
				player2Display.text = "--Not Connected--";
			}
		}
	}

	public void ChooseUsername(){
		string name = usernameInput.text;
		if(string.IsNullOrEmpty(name)) {
			Debug.Log("Field is empty");
			return;
		}
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
		roomOptions.PublishUserId = true;
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
		PhotonNetwork.automaticallySyncScene = true;
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
			player2Display.text = PhotonNetwork.otherPlayers[0].NickName;
			player1Display.text = PhotonNetwork.playerName;
			LobbyTitle.text = "(" + PhotonNetwork.room.Name + ") Lobby";
			playerNetwork.setTeammate(PhotonNetwork.otherPlayers[0].UserId);
		}
		inLobby = true;
	}

	virtual public void OnPhotonJoinRoomFailed(object[] codeAndMsg){
		Debug.Log(codeAndMsg[0]);
	}

	public void LeaveRoom(){
		PhotonNetwork.LeaveRoom();
	}

	virtual public void OnLeftRoom(){
		usernameScreenMenu.SetActive(false);
		MainScreenMenu.SetActive(true);
		JoinRoomMenu.SetActive(false);
		CreateRoomMenu.SetActive(false);
		LobbyRoomMenu.SetActive(false);
		inLobby = false;
	}


	public void loadGame(){
		// if(PhotonNetwork.playerList.Length <2) return; //Only able to join the game if there are two players in room.
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.LoadLevel("MainGameScene");
		
	}
}
