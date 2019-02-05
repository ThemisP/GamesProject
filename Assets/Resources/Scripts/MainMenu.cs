using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour {

    [Header("Menus")]
    public GameObject ConnectMenu;
    public GameObject LoginMenu;
    public GameObject MainScreenMenu;
    public GameObject LobbyMenu;

    [Header("InputFields")]
    public InputField ipAddress;
    public InputField username;
    public InputField roomIndexSelect;

    [Header("TextFields")]
    public Text RoomTitle;
    public Text Player1InLobby;
    public Text Player2InLobby;

    [Header("Other")]
    public Button PlayButton;

    private MenuState _state;
    private float timer = 0;
    private float timer2 = 0;
    public enum MenuState {
        ConnectIp,
        Login,
        Main,
        Lobby,
        InGame
    }

	// Use this for initialization
	void Start () {
        SetMenuState(MenuState.ConnectIp);
	}

    void Update() {
        switch (this._state) {
            case MenuState.ConnectIp:
                ConnectMenu.SetActive(true);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                break;
            case MenuState.Login:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(true);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                break;
            case MenuState.Main:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(true);
                LobbyMenu.SetActive(false);
                break;
            case MenuState.Lobby:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(true);

                //update every 1 seconds
                if (timer > 1f) {
                    Player1InLobby.text = Network.instance.player.GetUsername();
                    string p2 = Network.instance.player.GetTeammateUsername();
                    if (p2 != null)
                        Player2InLobby.text = p2;
                    RoomTitle.text = "Room Index: " + Network.instance.player.GetRoomIndex();
                    timer = 0;
                } else {
                    timer += Time.deltaTime;
                }

                //update every 2 seconds
                if(timer2 > 2f) {
                    Network.instance.GetPlayersInRoom();
                    timer2 = 0;
                } else {
                    timer2 += Time.deltaTime;
                }
                
                break;
            case MenuState.InGame:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                break;
        }
    }

    public void SetMenuState(MenuState state) {
        this._state = state;
    }

    public void PlayOffline() {
        SetMenuState(MenuState.InGame);
        Network.instance.JoinGameOffline();
    }

    public void ConnectToServer() {
        if (string.IsNullOrEmpty(ipAddress.text)) return;
        Network.instance.ConnectToGameServer(ipAddress.text);
    }
    public void ConnectedSuccesfull() {
        SetMenuState(MenuState.Login);
    }

    public void Login() {
        if (string.IsNullOrEmpty(username.text)) return;
        Network.instance.Login(username.text);
        SetMenuState(MenuState.Main);
    }

    public void CreateGame() {
        PlayButton.interactable = true;
        Network.instance.CreateGame(2);
    }
    public void CreateGameSuccessfull() {
        SetMenuState(MenuState.Lobby);
    }

    public void JoinGame() {
        if(string.IsNullOrEmpty(roomIndexSelect.text)) {
            Debug.Log("Enter room index to join");
            return;
        }
        int roomNum;
        int.TryParse(roomIndexSelect.text, out roomNum);
        Debug.Log(roomNum);
        PlayButton.interactable = false;
        Network.instance.JoinRoom(roomNum);
    }
    public void JoinRoomSuccessfull() {
        SetMenuState(MenuState.Lobby);
    }

    public void EnterMainGame() {
        Network.instance.JoinGame(0);
    }

    public void JoinGameSuccessfull() {
        Debug.Log("Joined Game");
        SetMenuState(MenuState.InGame);
    }

    public void Quit() {
        SetMenuState(MenuState.Login);
    }

    public void QuitMain() {
        SetMenuState(MenuState.Main);
    }

}
