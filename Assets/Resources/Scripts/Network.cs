﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

public class Network : MonoBehaviour {

    public static Network instance;

    [Header("Network Settings")]
    public string IP = "127.0.0.1";
    public int Port = 5500;
    public int UdpPort = 5501;
    public bool isConnected;
    public bool shouldHandleData;

    [Header("Other Settings")]
    public MainMenu mainMenu;    
    public CameraFollow cameraScript;
    public GameObject PlayerPrefab;
    public GameObject TeammatePlayerPrefab;
    public GameObject SimplifiedTeamPrefab;
    public GameObject EnemyPlayerPrefab;
    public GameObject HUD;

    public TcpClient TcpClient;
    public NetworkStream TcpStream;
    public StreamReader myReader;
    public StreamWriter myWriter;
    public UdpClient UdpClient;

    public IPEndPoint IPend;
    public PlayerInfo player;
    public PlayerController playerController;
    [HideInInspector] public int teamMateIndex;
    [HideInInspector]  public GameObject Team;
    public Dictionary<int, EnemyPlayerController> playersInGame;

    [HideInInspector] public int ClientIndex;//server related (something like a unique id very simple though)
    private byte[] asyncBuff;

    private Queue<Action> RunOnMainThread = new Queue<Action>();
    private Dictionary<int, Transform> SpawnPoints;

    private bool GameIsReady;
    private int NumberOfFullRooms;
    public bool waitingForAllReceive = false;
    public void Awake() {
        HUD.SetActive(false);
        playersInGame = new Dictionary<int, EnemyPlayerController>();
        SpawnPoints = new Dictionary<int, Transform>();
        instance = this;
        player = new PlayerInfo();
        Transform spawnPointGroup = GameObject.Find("SpawnPoints").transform;
        int i = 1;
        bool loopRun = true;
        while (loopRun) {
            Transform spawnpointX = spawnPointGroup.Find("SpawnPoint (" + i + ")");
            if (spawnpointX != null) {
                SpawnPoints.Add(i, spawnpointX.transform);
                i++;
            } else
                loopRun = false;

        }
    }

    // Use this for initialization
    void Start () {
        ClientHandlePackets.instance.InitMessages();
	}

	// Update is called once per frame
	void Update () {
        if (RunOnMainThread.Count > 0) {
            lock (RunOnMainThread) {
                Action s = RunOnMainThread.Dequeue();
                s();
            }
        }
        // if (player.playerObj == null)
        //     CancelInvoke();
    }

    public void CallFunctionFromAnotherThread(Action functionName) {
        lock (RunOnMainThread) {
            RunOnMainThread.Enqueue(functionName);
        }
    }

    public void ConnectToGameServer(string ip) {
        if (TcpClient != null) {
            if (TcpClient.Connected || isConnected) return;
            TcpClient.Close();
            TcpClient = null;
        }
        this.IP = ip;
        TcpClient = new TcpClient();
        TcpClient.ReceiveBufferSize = 4096;
        TcpClient.SendBufferSize = 4096;
        TcpClient.NoDelay = false;
        Array.Resize(ref asyncBuff, 8192);
        TcpClient.BeginConnect(IP, Port, new AsyncCallback(ConnectCallback), TcpClient);
        isConnected = true;
        Debug.Log("connect send");
        //checkHealthThread = new Thread(() => CheckPlayerHealth(PlayerPrefab, TcpStream));
        //checkHealthThread.Start();
    }

    public void StartUdp() {
        UdpClient = new UdpClient();
        UdpClient.Connect(IP, UdpPort);

        IPend = new IPEndPoint(IPAddress.Any, 0);
        UdpClient.BeginReceive(new AsyncCallback(OnReceiveUdp), null);
    }

    void ConnectCallback(IAsyncResult result) {
        if (TcpClient != null) {
            TcpClient.EndConnect(result);
            if (!TcpClient.Connected) {
                isConnected = false;
                return;
            } else {
                TcpClient.NoDelay = true;
                TcpStream = TcpClient.GetStream();
                TcpStream.BeginRead(asyncBuff, 0, 8192, OnReceiveTcp, null);

                mainMenu.SetMenuState(MainMenu.MenuState.Login);
            }
        }
    }

    #region "GameRelated"
    public void JoinGame() {
        HUD.SetActive(true);
        int teamNumber = this.player.GetTeamNumber();

        Transform spawnpoint;
        if (SpawnPoints.Count > 0 && SpawnPoints.TryGetValue((teamNumber + 1) % SpawnPoints.Count, out spawnpoint)) {
            if (player.playerNumber == 1) spawnpoint.position = spawnpoint.position + Vector3.forward * 2;
            else spawnpoint.position = spawnpoint.position + Vector3.forward * -2;
            GameObject playerObj = GameObject.Instantiate(PlayerPrefab, spawnpoint.position, spawnpoint.rotation);
            PlayerController playerContr = playerObj.GetComponent<PlayerController>();
            if (playerContr == null) Debug.LogError("PlayerController not found when joining game");
            else playerController = playerContr;
            player.SetPlayerObj(playerObj);
            cameraScript.SetTarget(playerObj.transform);
            ObjectHandler.instance.InstantiateCollectibles();
            InvokeRepeating("SendPlayerPos", 0f, 0.1f); //Every 0.1 seconds, repeated calls to send player position to server.
        } else {
            Debug.LogError("SpawnPoint transform not found");
        }
    }

    public void JoinGameOffline() {
        HUD.SetActive(true);
        Transform spawnpoint;
        if (SpawnPoints.Count > 0 && SpawnPoints.TryGetValue((0 + 1) % SpawnPoints.Count, out spawnpoint)) {
            GameObject playerObj = GameObject.Instantiate(PlayerPrefab, spawnpoint.position, spawnpoint.rotation);
            player.SetPlayerObj(playerObj);
            PlayerController playerContr = playerObj.GetComponent<PlayerController>();
            if (playerContr == null) Debug.LogError("PlayerController not found when joining game");
            else playerController = playerContr;

            GameObject teammateObj = GameObject.Instantiate(TeammatePlayerPrefab, spawnpoint.position + Vector3.forward * 2, spawnpoint.rotation);
            Team = GameObject.Instantiate(SimplifiedTeamPrefab, new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(0, 0, 0)));

            TeamScript script = Team.GetComponent<TeamScript>();
            if (script != null) StartCoroutine(SpawnChainAndSetPlayers(playerObj.transform, teammateObj.transform, script));
            else Debug.Log("teamScript error");

            PlayerController controller = playerObj.GetComponent<PlayerController>();
            if (controller != null) controller.SetTeamController(script);
            else Debug.Log("cannot find controller");

            player.SetOffline(true);
            cameraScript.SetTarget(playerObj.transform);
            ShrinkCircle.instance.StartCircle();
        } else {
            Debug.LogError("Spawnpoint transform not found");
        }
    }

    IEnumerator SpawnChainAndSetPlayers(Transform p1, Transform p2, TeamScript script) {
        yield return new WaitForSecondsRealtime(2);
        script.SetPlayers(p1, p2);
        playerController.SetWaitingForGame(false);
    }

    public void SpawnPlayer(int id, string username, int team, Vector3 pos, Vector3 rot, float health) {
        if (id == ClientIndex) return;
        if (playersInGame.ContainsKey(id)) return;
        GameObject playerObj;
        if (team == player.GetTeamNumber()) {
            playerObj = GameObject.Instantiate(TeammatePlayerPrefab, pos, Quaternion.Euler(rot));
            Team = GameObject.Instantiate(SimplifiedTeamPrefab, new Vector3(0,1,0), Quaternion.Euler(new Vector3(0,0,0)));

            TeamScript script = Team.GetComponent<TeamScript>();
            if (script != null) StartCoroutine(SpawnChainAndSetPlayers(player.GetPlayerObj().transform, playerObj.transform, script));
            else Debug.Log("teamScript error");

            PlayerController teammateController = player.GetPlayerObj().GetComponent<PlayerController>();
            if (teammateController != null) teammateController.SetTeamController(script);
            else Debug.Log("Cannot find controller");
            teamMateIndex = id;
        } else {
            playerObj = GameObject.Instantiate(EnemyPlayerPrefab, pos, Quaternion.Euler(rot));
        }
        EnemyPlayerController controller = playerObj.GetComponent<EnemyPlayerController>();
        if (controller == null) Debug.LogError("Controller not found in spawned player");
        else {
            Debug.Log("register " + id);
            controller.SetPlayerId(id);
            controller.SetUsername(username);
            controller.SetTeamNumber(team);
            controller.SetHealth(health);
            playersInGame.Add(id, controller);
        }


    }

    public void DestroyPlayer(int id) {
        EnemyPlayerController controller;
        if (playersInGame.TryGetValue(id, out controller)) {
            playersInGame.Remove(id);
            Destroy(controller.gameObject, 2f);
        }
    }

    public void SetHealthPlayer(int id, float amount) {
        EnemyPlayerController controller;
        if (playersInGame.TryGetValue(id, out controller)) {
            playersInGame.Remove(id);
            controller.SetHealth(amount);
        }
    }

    public void LeaveGameLogic() {
        CancelInvoke();
        Destroy(player.playerObj);
        foreach(KeyValuePair<int, EnemyPlayerController> enemy in playersInGame) {
            Destroy(enemy.Value.gameObject);
        }
        cameraScript.SetToOriginalPosition();
        mainMenu.SetMenuState(MainMenu.MenuState.Main);
    }

    public void Died() {        
        try {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInt(16);
            buffer.WriteInt(player.GetTeamNumber());
            buffer.WriteInt(player.GetGameIndex());
            buffer.WriteInt(player.GetRoomIndex());
            TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
            
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    public void HandlePlayerDamage(int id, bool isAlive, float health) {
        EnemyPlayerController controller;
        if(playersInGame.TryGetValue(id, out controller)){
            controller.SetHealth(health);
            if (!isAlive)
                controller.Died();
        }
    }

    public bool GameReady() {
        return GameIsReady;
    }

    public void SetGameReady() {
        GameIsReady = true;
    }

    public void GameOver(bool won) {
        Team.GetComponent<TeamScript>().DestroyChain();
        HUD.SetActive(false);
        playerController.playerData.EndGame();
        Destroy(player.playerObj, 2f);
        waitingForAllReceive = false;
        GameIsReady = false;
        EnemyPlayerController controller;
        mainMenu.GameOver(won);
        ObjectHandler.instance.InstantiateCollectibles();
        ShrinkCircle.instance.ResetRadious();
        foreach (KeyValuePair<int, EnemyPlayerController> enemy in playersInGame) {
            if (playersInGame.TryGetValue(enemy.Key, out controller)) {
                playersInGame.Remove(enemy.Key);
                Destroy(controller.gameObject, 0f);
            }
        }
    }
    #endregion

    #region "Server Communication"

    #region "UDP communication"
    void OnReceiveUdp(IAsyncResult result) {
        if (UdpClient != null) {
            if (UdpClient == null) return;

            byte[] myBytes = UdpClient.EndReceive(result,ref IPend);

            if (myBytes.Length == 0) {
                Debug.Log("You got disconnected");
                UdpClient.Close();
                return;
            }
            //Handle Data
            ClientHandlePackets.instance.HandleDataUdp(myBytes);

            UdpClient.BeginReceive(new AsyncCallback(OnReceiveUdp), null);

        }
    }

    //Once you join a game this is invoked every .2 seconds to update the player's location on the server.
    public void SendPlayerPos() {
        if (player.playerObj == null) return;
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(ClientIndex);
        buffer.WriteInt(2);
        Transform playerTransform = player.playerObj.transform;
        buffer.WriteString(DateTime.Now.ToString());
        buffer.WriteFloat(playerTransform.position.x);
        buffer.WriteFloat(playerTransform.position.y);
        buffer.WriteFloat(playerTransform.position.z);
        Rigidbody rigidbod = player.playerObj.GetComponent<Rigidbody>();
        if (rigidbod == null) Debug.LogError("rigidbody not found!!!");
        buffer.WriteFloat(rigidbod.velocity.x);
        buffer.WriteFloat(rigidbod.velocity.y);
        buffer.WriteFloat(rigidbod.velocity.z);

        buffer.WriteFloat(playerTransform.rotation.eulerAngles.y);
        UdpClient.Send(buffer.BuffToArray(), buffer.Length());
    }
    #endregion

    #region "TCP communication"

    void OnReceiveTcp(IAsyncResult result) {
        if (TcpClient != null) {
            if (TcpClient == null) return;

            int byteArray = TcpStream.EndRead(result);
            byte[] myBytes = null;
            Array.Resize(ref myBytes, byteArray);
            Buffer.BlockCopy(asyncBuff, 0, myBytes, 0, byteArray);

            if (byteArray == 0) {
                Debug.Log("You got disconnected");
                TcpClient.Close();
                return;
            }

            //Handle Data
            ClientHandlePackets.instance.HandleData(myBytes);

            if (TcpClient == null) return;
            TcpStream.BeginRead(asyncBuff, 0, 8192, OnReceiveTcp, null);

        }
    }

    public void SendBullet(Vector3 pos, float rotY, float speed, float lifeTime, string bulletId, float damage) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(3);
        buffer.WriteString(DateTime.Now.ToString());
        buffer.WriteString(bulletId);
        buffer.WriteInt(player.GetTeamNumber());
        buffer.WriteFloat(pos.x);
        buffer.WriteFloat(pos.y);
        buffer.WriteFloat(pos.z);
        buffer.WriteFloat(rotY);
        buffer.WriteFloat(speed);
        buffer.WriteFloat(lifeTime);
        buffer.WriteFloat(damage);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }
    public void Login(string username) {
        if(TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(1);
        buffer.WriteString(username);
        player.ChangeUsername(username);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void CreateGame(int MaxPlayers) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(2);
        buffer.WriteInt(MaxPlayers);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void GetPlayersInRoom() {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        if (!player.InRoom()) return;
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(5);
        buffer.WriteInt(player.GetRoomIndex());
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void JoinRoom(int roomIndex) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(6);
        buffer.WriteInt(roomIndex);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void JoinGame(int GameIndex) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(9);
        buffer.WriteInt(GameIndex);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void GetPlayersInGame() {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(10);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }


    public void SendPlayerDamage(float damageTake, string bulletId) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(12);
        buffer.WriteString(bulletId);
        buffer.WriteFloat(damageTake);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void HandlePlayerDeath(string bulletId) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }

        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(-1);
        buffer.WriteString(bulletId);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void LeaveGame() {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }

        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(13);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void LeaveRoom() {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }

        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(14);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void IsGameReady() {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }
        if (waitingForAllReceive) {
            return;
        }
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(15);
        buffer.WriteInt(player.GetGameIndex());
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void RevivedTeammate() {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }

        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(18);
        buffer.WriteInt(teamMateIndex);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    //coinOrPill 0 == coin / 1 == Pill
    public void SendCollectiblesDestroy(string id, int coinOrPill) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }

        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(20);
        buffer.WriteInt(coinOrPill);
        buffer.WriteString(id);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }

    public void SendPlayerHealth(float health) {
        if (TcpClient == null || !TcpClient.Connected) {
            TcpClient.Close();
            TcpClient = null;
            Debug.Log("Disconnected");
            return;
        }

        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteInt(21);
        buffer.WriteFloat(health);
        TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
    }
    #endregion

    #endregion
    

    public void ReceiveGameReady() {
        try {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInt(19);
            buffer.WriteInt(player.GetGameIndex());
            TcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }
}
