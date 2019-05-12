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
    public GameObject EscapeMenu;
    public GameObject BackgroundImage;
    public GameObject VictoryImage;
    public GameObject LostImage;

    public GameObject SpectateMode;

    [Header("InputFields")]
    public InputField ipAddress;
    public InputField username;
    public InputField roomIndexSelect;

    [Header("TextFields")]
    public Text RoomTitle;
    public Text Player1InLobby;
    public Text Player2InLobby;

    public Text TimeUntilGameStart;

    [Header("Other")]
    public Button PlayButton;

    private MenuState _state;
    private float timer = 0;
    private float timer2 = 0;

    private float timer3 =  0;
    private float startTimer = -1f;
    public enum MenuState {
        ConnectIp,
        Login,
        Main,
        Lobby,
        InGame,
        Spectate,
    }

	// Use this for initialization
	void Start () {
        SetMenuState(MenuState.ConnectIp);
	}

    void Update() {
        switch (this._state) {
            case MenuState.Spectate:
                SpectateMode.SetActive(true);
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                EscapeMenu.SetActive(false);
                BackgroundImage.SetActive(false);
                break;
            case MenuState.ConnectIp:
                ConnectMenu.SetActive(true);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                EscapeMenu.SetActive(false);
                BackgroundImage.SetActive(true);
                break;
            case MenuState.Login:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(true);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                EscapeMenu.SetActive(false);
                BackgroundImage.SetActive(true);
                break;
            case MenuState.Main:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(true);
                LobbyMenu.SetActive(false);
                EscapeMenu.SetActive(false);
                BackgroundImage.SetActive(true);
                break;
            case MenuState.Lobby:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(true);
                EscapeMenu.SetActive(false);
                BackgroundImage.SetActive(true);
            
                if (Network.instance.GameReady()) {
                    TimeUntilGameStart.text = "READY!";
                } else if (startTimer >= 0f){
                    TimeUntilGameStart.text = string.Format("starting in: {0} seconds ", startTimer);
                } else{
                    TimeUntilGameStart.text = string.Empty;
                }
                //update every 1 seconds
                if (timer > 1f) {
                    startTimer -= 1f;
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
                    Debug.Log("get players from menu");
                    Network.instance.GetPlayersInRoom();
                    timer2 = 0;
                } else {
                    timer2 += Time.deltaTime;
                }

                // Update every 5 seconds
                if (timer3 > 5f) {
                    Network.instance.IsGameReady(); 
                    timer3 = 0;
                } else {
                    timer3 += Time.deltaTime;
                }
                
                break;
            case MenuState.InGame:
                ConnectMenu.SetActive(false);
                LoginMenu.SetActive(false);
                MainScreenMenu.SetActive(false);
                LobbyMenu.SetActive(false);
                BackgroundImage.SetActive(false);
                bool escape = Input.GetKeyDown(KeyCode.Escape);
                if (escape) {
                    if (!EscapeMenu.activeSelf)
                        EscapeMenu.SetActive(true);
                    else
                        EscapeMenu.SetActive(false);
                }

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

    public void Login() {
        if (string.IsNullOrEmpty(username.text)) return;
        Network.instance.Login(username.text);
    }

    public void LoginResponse(int successful, string msg) {
        if(successful == 1) 
            SetMenuState(MenuState.Main);
        Debug.Log(msg);
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
        if (Network.instance.GameReady()) {
            Network.instance.JoinGame(0);
        }
        else Debug.Log("Can't enter main game, not all players present");
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

    public void LeaveGame() {
        Network.instance.LeaveGame();
    }

    public void SetStartTimer(float startTimer) {
        this.startTimer = startTimer;
    }

    public void SpectateGame() {
        SetMenuState(MenuState.Spectate);
    }
    public void GameOver(bool won) {
        StartCoroutine(GameOverScreen(won));
    }

    IEnumerator GameOverScreen(bool won) {
        if (won) {
            VictoryImage.SetActive(true);
        } else {
            LostImage.SetActive(true);
        }
        yield return new WaitForSecondsRealtime(4);
        VictoryImage.SetActive(false);
        LostImage.SetActive(false);

        username.text = null;
        username.text = null;
        roomIndexSelect.text = null;
        SetMenuState(MenuState.Login);
    }

}
