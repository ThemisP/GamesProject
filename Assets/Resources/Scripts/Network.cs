using System;
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
    public Transform[] spawnpoints;
    public CameraFollow cameraScript;
    public GameObject PlayerPrefab;
    public GameObject TeammatePlayerPrefab;
    public GameObject EnemyPlayerPrefab;
    public GameObject HUD;

    public TcpClient TcpClient;
    public NetworkStream TcpStream;
    public StreamReader myReader;
    public StreamWriter myWriter;
    public UdpClient UdpClient;

    public IPEndPoint IPend;
    public PlayerInfo player;
    [HideInInspector] public int teamMateIndex;
    public Dictionary<int, EnemyPlayerController> playersInGame;

    [HideInInspector] public int ClientIndex;//server related (something like a unique id very simple though)
    private byte[] asyncBuff;

    private Queue<Action> RunOnMainThread = new Queue<Action>();

    public void Awake() {
        HUD.SetActive(false);
        playersInGame = new Dictionary<int, EnemyPlayerController>();
        instance = this;
        player = new PlayerInfo();
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
        if (player.playerObj == null)
            CancelInvoke();
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
        Transform spawnpoint = spawnpoints[teamNumber % 2];
        if (player.playerNumber == 1) spawnpoint.position = spawnpoint.position + Vector3.forward * 2;
        else spawnpoint.position = spawnpoint.position + Vector3.forward * -2;
        GameObject playerObj = GameObject.Instantiate(PlayerPrefab, spawnpoint.position, spawnpoint.rotation);
        player.SetPlayerObj(playerObj);
        cameraScript.SetTarget(playerObj.transform);
        InvokeRepeating("SendPlayerPos", 0f, 0.1f); //Every 0.1 seconds, repeated calls to send player position to server.
        //GetPlayersInGame();
    }

    public void JoinGameOffline() {
        HUD.SetActive(true);
        Transform spawnpoint = spawnpoints[0];
        GameObject playerObj = GameObject.Instantiate(PlayerPrefab, spawnpoint.position, spawnpoint.rotation);
        player.SetPlayerObj(playerObj);
        player.SetOffline(true);
        cameraScript.SetTarget(playerObj.transform);

        GameObject teammateObj = GameObject.Instantiate(TeammatePlayerPrefab, spawnpoint.position + Vector3.forward * 2, spawnpoint.rotation);
        //teamMate = teammateObj;
    }

    public void SpawnPlayer(int id, string username, int team, Vector3 pos, Vector3 rot) {
        if (id == ClientIndex) return;
        if (playersInGame.ContainsKey(id)) return;
        GameObject playerObj;
        if (team == player.GetTeamNumber()) {
            playerObj = GameObject.Instantiate(TeammatePlayerPrefab, pos, Quaternion.Euler(rot));
            teamMateIndex = id;
        } else {
            playerObj = GameObject.Instantiate(EnemyPlayerPrefab, pos, Quaternion.Euler(rot));
        }
        EnemyPlayerController controller = playerObj.GetComponent<EnemyPlayerController>();
        if (controller == null) Debug.LogError("Controller not found in spawned player");
        else {
            controller.SetPlayerId(id);
            controller.SetUsername(username);
            controller.SetTeamNumber(team);
            playersInGame.Add(id, controller);
        }


    }

    public void DestroyPlayer(int id, int teamNumber) {
        EnemyPlayerController controller;
        if (playersInGame.TryGetValue(id, out controller)) {
            playersInGame.Remove(id);
            Destroy(controller.gameObject, 0f);
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
        LeaveGame();
    }
    public void HandlePlayerDamage(int id, bool isAlive, float health) {
        EnemyPlayerController controller;
        if(playersInGame.TryGetValue(id, out controller)){
            controller.SetHealth(health);
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

    #endregion

    #endregion
}
