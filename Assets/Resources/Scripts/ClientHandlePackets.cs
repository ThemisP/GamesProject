using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class ClientHandlePackets{
    public static ClientHandlePackets instance = new ClientHandlePackets();
    private delegate void Packet_(byte[] data);
    private Dictionary<int, Packet_> PacketsTcp;
    private Dictionary<int, Packet_> PacketsUdp;

    public void InitMessages() {
        PacketsTcp = new Dictionary<int, Packet_>();
        PacketsTcp.Add(1, HandleWelcomeMessage);
        //handleLoginSuccess....
        PacketsTcp.Add(3, HandleCreateRoomResponse);
        PacketsTcp.Add(4, HandleGetPlayersInRoom);
        PacketsTcp.Add(5, HandleJoinRoomResponse);
        PacketsTcp.Add(6, HandleJoinGameResponse);
        PacketsTcp.Add(7, HandleGetPlayersInGameResponse);
        PacketsTcp.Add(8, HandleDestroyBullet);

        PacketsTcp.Add(12, HandleDealtDamage);

        PacketsUdp = new Dictionary<int, Packet_>();
        PacketsUdp.Add(1, HandlePlayerPos);
        PacketsUdp.Add(2, HandleReceivePlayersLocations);
        PacketsUdp.Add(3, HandleRecievePlayerBullet);
    }

    public void HandleData(byte[] data) {
        int packetnum;
        Packet_ packet;
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        packetnum = buffer.ReadInt();
        buffer = null;
        
        if (packetnum == 0) return;

        if (PacketsTcp.TryGetValue(packetnum, out packet)) {
            packet.Invoke(data);
        } else {
            Debug.Log("Packet number | " + packetnum + " | does not exist (in TCP) the client doesn't know what to do with the data.");
        }
    }

    public void HandleDataUdp(byte[] data) {
        int packetnum;
        Packet_ packet;
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        packetnum = buffer.ReadInt();
        buffer = null;
        Debug.Log("udp received");
        if (packetnum == 0) return;

        if (PacketsUdp.TryGetValue(packetnum, out packet)) {
            packet.Invoke(data);
        } else {
            Debug.Log("Packet number | " + packetnum + " | does not exist (in UDP) the client doesn't know what to do with the data.");
        }
    }

    #region "Udp Packets"
    //Packetnum = 1
    void HandlePlayerPos(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int playerIndex = buffer.ReadInt();
        string playerName = buffer.ReadString();
        float xpos = buffer.ReadFloat();
        float ypos = buffer.ReadFloat();
        float zpos = buffer.ReadFloat();
        float yrot = buffer.ReadFloat();

    }

    void HandleReceivePlayersLocations(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int numberOfPlayers = buffer.ReadInt();
        for (int i = 0; i < numberOfPlayers; i++) {
            int playerId = buffer.ReadInt();
            float posX = buffer.ReadFloat();
            float posY = buffer.ReadFloat();
            float posZ = buffer.ReadFloat();
            float velX = buffer.ReadFloat();
            float velY = buffer.ReadFloat();
            float velZ = buffer.ReadFloat();

            float rotY = buffer.ReadFloat();

            EnemyPlayerController controller;
            //Debug.Log("testing: xpos: " + posX + ", " + posY + ", " + posZ);
            //Testing
            if(Network.instance.playersInGame.TryGetValue(playerId, out controller)){
                controller.CallFunctionFromAnotherThread(() => {
                    controller.SetPlayerPosAndRot(new Vector3(posX, posY, posZ), 
                                                  new Vector3(0, rotY, 0),
                                                  new Vector3(velX, velY, velZ));
                });
            } else {
                Debug.LogWarning("Getting info for an unregistered player");
                Network.instance.CallFunctionFromAnotherThread(() => {
                    Network.instance.SpawnPlayer(playerId, "greg", 4,
                                                 new Vector3(posX, posY, posZ),
                                                 new Vector3(0, rotY, 0));
                });
            }
        }
    }

    void HandleRecievePlayerBullet(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetNumber = buffer.ReadInt();
        string bulletId = buffer.ReadString();

        float posX = buffer.ReadFloat();
        float posY = buffer.ReadFloat();
        float posZ = buffer.ReadFloat();
        float rotY = buffer.ReadFloat();
        float speed = buffer.ReadFloat();
        float lifeTime = buffer.ReadFloat();

        ObjectHandler.instance.CallFunctionFromAnotherThread(() => {
            ObjectHandler.instance.InstantiateBullet(new Vector3(posX, posY, posZ),
                                                     new Vector3(0, rotY, 0),
                                                     speed,
                                                     lifeTime,
                                                     bulletId);
        });
    }

    #endregion

    #region "Tcp Packets"
    //Packetnum = 1
    void HandleWelcomeMessage(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        string msg = buffer.ReadString();
        int ClientIndex = buffer.ReadInt();
        Network.instance.ClientIndex = ClientIndex;
        Network.instance.StartUdp();
        Debug.Log(msg);

        buffer.Clear();
        buffer.WriteInt(ClientIndex);
        buffer.WriteInt(1);
        buffer.WriteFloat(10.2f);
        //Debug.Log("send");
        Network.instance.UdpClient.Send(buffer.BuffToArray(), buffer.Length());
    }
    //Packetnum = 3
    void HandleCreateRoomResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int finished = buffer.ReadInt();
        int roomIndex = buffer.ReadInt();
        if (finished == 1) {
            Debug.Log("Succeded with roomIndex: " + roomIndex);
            Network.instance.player.JoinRoom(roomIndex);
            Network.instance.mainMenu.CreateGameSuccessfull();
        } else {
            Debug.Log("Failed to create a room!");
        }
    }
    //Packetnum = 4
    void HandleGetPlayersInRoom(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int numberOfPlayers = buffer.ReadInt();
        Debug.Log(numberOfPlayers);
        for (int i = 0; i < numberOfPlayers; i++) {
            string user = buffer.ReadString();
            if (user != Network.instance.player.GetUsername())
                Network.instance.player.SetTeammateUsername(user);
            Debug.Log("User: " + user);
        }
    }
    //Packetnum = 5
    void HandleJoinRoomResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int response = buffer.ReadInt();
        int roomIndex = buffer.ReadInt();
        if(response == 1) {
            Debug.Log("Joined");
            Network.instance.player.JoinRoom(roomIndex);
            Network.instance.mainMenu.JoinRoomSuccessfull();
        } else {
            Debug.Log("Failed");
        }
    }
    //Packetnum = 6
    void HandleJoinGameResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int response = buffer.ReadInt();        
        if (response == 1) {
            int gameIndex = buffer.ReadInt();
            int teamIndex = buffer.ReadInt();
            int playerNumber = buffer.ReadInt();//Spawning purposes
            int teammateIndex = buffer.ReadInt();
            string teammateUsername = buffer.ReadString();
            Debug.Log("Joined");

            //Network.instance.player.JoinGame(gameIndex);
            Network.instance.player.playerNumber = playerNumber;
            Network.instance.player.SetTeamNumber(teamIndex);
            Network.instance.player.SetTeammate(teammateIndex, teammateUsername);
            Network.instance.CallFunctionFromAnotherThread(Network.instance.JoinGame);
            Network.instance.mainMenu.JoinGameSuccessfull();
        } else {
            Debug.Log("Failed");
        }
    }
    //Packetnum = 7
    void HandleGetPlayersInGameResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        int numberOfPlayers = buffer.ReadInt();
        for(int i=0; i<numberOfPlayers; i++) {
            int playerId = buffer.ReadInt();
            int playerTeam = buffer.ReadInt();
            string playerUsername = buffer.ReadString();
            float posX = buffer.ReadFloat();
            float posY = buffer.ReadFloat();
            float posZ = buffer.ReadFloat();
            float rotY = buffer.ReadFloat();
            
            //Testing
            Network.instance.CallFunctionFromAnotherThread(() => {
                Network.instance.SpawnPlayer(playerId ,playerUsername, playerTeam,
                                             new Vector3(posX, posY, posZ),
                                             new Vector3(0, rotY, 0));
            });
        }
    }

    //Packetnum = 8
    void HandleDestroyBullet(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        string bulletId = buffer.ReadString();
        ObjectHandler.instance.CallFunctionFromAnotherThread(() => {
            ObjectHandler.instance.DestroyBullet(bulletId);
        });        
    }

    // Packetnum = 12
    void HandleDealtDamage(byte[] data){
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInt();
        float damageDealt = buffer.ReadFloat();
        Network.instance.CallFunctionFromAnotherThread(() => {
            if (Network.instance.player.playerObj.activeInHierarchy){
                Network.instance.player.playerObj.GetComponent<PlayerData>().UpdateDamageDealt(damageDealt);
            }
        });
    }
    #endregion
}


