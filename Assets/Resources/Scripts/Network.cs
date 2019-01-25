using System;
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
    public CameraFollow camera;
    public GameObject PlayerPrefab;

    public TcpClient TcpClient;
    public NetworkStream TcpStream;
    public StreamReader myReader;
    public StreamWriter myWriter;
    public UdpClient UdpClient;

    public IPEndPoint IPend;
    public PlayerInfo player;

    public int ClientIndex;//server related (something like a unique id very simple though)
    private byte[] asyncBuff;

    private Queue<string> RunOnMainThread = new Queue<string>();

    private Thread checkHealthThread;

    public void Awake() {
        instance = this;
        player = new PlayerInfo();
    }

    // Use this for initialization
    void Start () {
        ClientHandlePackets.instance.InitMessages();
        ConnectToGameServer();
	}
	
	// Update is called once per frame
	void Update () {
        if (RunOnMainThread.Count > 0) {
            foreach(string s in RunOnMainThread) {
                Invoke(s, 0f);
            }
            RunOnMainThread.Clear();
        }
    }

    public void CallFunctionFromAnotherThread(string functionName) {
        RunOnMainThread.Enqueue(functionName);
    }

    void ConnectToGameServer() {
        if (TcpClient != null) {
            if (TcpClient.Connected || isConnected) return;
            TcpClient.Close();
            TcpClient = null;
        }

        TcpClient = new TcpClient();
        TcpClient.ReceiveBufferSize = 4096;
        TcpClient.SendBufferSize = 4096;
        TcpClient.NoDelay = false;
        Array.Resize(ref asyncBuff, 8192);
        TcpClient.BeginConnect(IP, Port, new AsyncCallback(ConnectCallback), TcpClient);
        isConnected = true;
        checkHealthThread = new Thread(() => CheckPlayerHealth(PlayerPrefab, TcpStream));
        checkHealthThread.Start();
    }

    public void StartUdp() {
        UdpClient = new UdpClient();
        UdpClient.Connect(IP, UdpPort);

        IPend = new IPEndPoint(IPAddress.Any, 0);
        Debug.Log("setup Udp");
        //UdpClient.BeginReceive(new AsyncCallback(OnReceiveUdp), null);
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
            }
        }
    }

    #region "GameRelated"
    void JoinGame() {
        GameObject player = GameObject.Instantiate(PlayerPrefab, spawnpoints[0].position, spawnpoints[0].rotation);
        camera.SetTarget(player.transform);
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
            Debug.Log("reached");
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(myBytes);
            float number2 = buffer.ReadFloat();
            Debug.Log("from server " + number2);
            //Handle Data
            //ClientHandlePackets.instance.HandleData(myBytes);


            if (TcpClient == null) return;
            TcpStream.BeginRead(asyncBuff, 0, 8192, OnReceiveTcp, null);

        }
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

    // Thread which periodically checks the players health status, 
    // on case that player health is 0, sends message to server 
    // to update status of the player and broadcast death other 
    // players
    private static void CheckPlayerHealth(GameObject playerPrefab, NetworkStream tcpStream)
    {
        if (playerPrefab.activeInHierarchy && playerPrefab.GetComponent<PlayerData>().getCurrentHealth() <= 0){
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInt(-1);
            tcpStream.Write(buffer.BuffToArray(), 0, buffer.Length());
            Debug.Log("Player has died");
        }
        else
        {
            Thread.Sleep(5000);
        }
    }
    #endregion

    #endregion
}
