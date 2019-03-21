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
        PacketsTcp.Add(9, HandlePlayerTookDamage);
        PacketsTcp.Add(10, HandlePlayerDeath);
        PacketsTcp.Add(11, HandleOtherPlayerDeath);
        PacketsTcp.Add(12, HandleDealtDamage);
        PacketsTcp.Add(13, HandleLeaveGame);
        PacketsTcp.Add(14, HandleRecievePlayerBullet);
        PacketsTcp.Add(15, HandleGameReady);
        
        PacketsUdp = new Dictionary<int, Packet_>();
        PacketsUdp.Add(2, HandleReceivePlayersLocations);
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
            packet.Invoke(data.Skip(4).ToArray());
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

        if (packetnum == 0) return;

        if (PacketsUdp.TryGetValue(packetnum, out packet)) {
            packet.Invoke(data.Skip(4).ToArray());
        } else {
            Debug.Log("Packet number | " + packetnum + " | does not exist (in UDP) the client doesn't know what to do with the data.");
        }
    }

    #region "Udp Packets"
    //Packetnum = 2
    void HandleReceivePlayersLocations(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int numberOfPlayers = buffer.ReadInt();
        for (int i = 0; i < numberOfPlayers; i++) {
            int playerId = buffer.ReadInt();
            string playerUsername = buffer.ReadString();
            int playerTeam = buffer.ReadInt();
            float posX = buffer.ReadFloat();
            float posY = buffer.ReadFloat();
            float posZ = buffer.ReadFloat();
            float velX = buffer.ReadFloat();
            float velY = buffer.ReadFloat();
            float velZ = buffer.ReadFloat();

            float rotY = buffer.ReadFloat();

            EnemyPlayerController controller;
            if (Network.instance.playersInGame.TryGetValue(playerId, out controller)) {
                controller.CallFunctionFromAnotherThread(() => {
                    controller.SetPlayerPosAndRot(new Vector3(posX, posY, posZ),
                                                  new Vector3(0, rotY, 0),
                                                  new Vector3(velX, velY, velZ));
                });
            } else {
                Debug.LogWarning("Getting info for an unregistered player");
                Network.instance.CallFunctionFromAnotherThread(() => {
                    Network.instance.SpawnPlayer(playerId, playerUsername, playerTeam,
                                                    new Vector3(posX, posY, posZ),
                                                    new Vector3(0, rotY, 0));
                });
            }
        }
    }
    

    #endregion

    #region "Tcp Packets"
    //Packetnum = 1
    void HandleWelcomeMessage(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
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
        int finished = buffer.ReadInt();
        int roomIndex = buffer.ReadInt();
        if (finished == 1) {
            Network.instance.player.JoinRoom(roomIndex);
            Network.instance.mainMenu.CreateGameSuccessfull();
        } else if (finished == -1) {
            Debug.Log("Failed to create a room, all game rooms taken, find another game to join");
        }
        else {
            Debug.Log("Failed to create a room!");
        }
    }
    //Packetnum = 4
    void HandleGetPlayersInRoom(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int numberOfPlayers = buffer.ReadInt();
        for (int i = 0; i < numberOfPlayers; i++) {
            string user = buffer.ReadString();
            if (user != Network.instance.player.GetUsername())
                Network.instance.player.SetTeammateUsername(user);
        }
    }
    //Packetnum = 5
    void HandleJoinRoomResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int response = buffer.ReadInt();
        int roomIndex = buffer.ReadInt();
        if(response == 1) {
            Network.instance.player.JoinRoom(roomIndex);
            Network.instance.mainMenu.JoinRoomSuccessfull();
        } else {
            Debug.Log("Failed to join room (response)");
        }
    }
    //Packetnum = 6
    void HandleJoinGameResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int response = buffer.ReadInt();        
        if (response == 1) {
            int gameIndex = buffer.ReadInt();
            int teamIndex = buffer.ReadInt();
            int playerNumber = buffer.ReadInt();//Spawning purposes
            int teammateIndex = buffer.ReadInt();
            string teammateUsername = buffer.ReadString();

            Network.instance.player.SetGameIndex(gameIndex);
            Network.instance.player.playerNumber = playerNumber;
            Network.instance.player.SetTeamNumber(teamIndex);
            Network.instance.player.SetTeammate(teammateIndex, teammateUsername);
            Network.instance.CallFunctionFromAnotherThread(Network.instance.JoinGame);
            Network.instance.mainMenu.JoinGameSuccessfull();
        } else {
            Debug.Log("Failed to join game (response)");
        }
    }
    //Packetnum = 7
    void HandleGetPlayersInGameResponse(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
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


    //Packetnum = 9
    void HandlePlayerTookDamage(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int clientId = buffer.ReadInt();
        int teamNumber = buffer.ReadInt();
        string bulletId = buffer.ReadString();
        bool isAlive = (buffer.ReadInt() == 1) ? true : false;
        float health = buffer.ReadFloat();
        ObjectHandler.instance.CallFunctionFromAnotherThread(() => {
            ObjectHandler.instance.DestroyBullet(bulletId);
        });
        Network.instance.HandlePlayerDamage(clientId, isAlive, health);
        if (!isAlive) {
            Network.instance.CallFunctionFromAnotherThread(() => {
                Network.instance.DestroyPlayer(clientId, teamNumber);
            });
        }
    }

    //Packetnum = 10
    void HandlePlayerDeath(byte[] data) {

        Network.instance.CallFunctionFromAnotherThread(() => {
            Network.instance.Died();
        });
    }

    //Packetnum = 11
    void HandleOtherPlayerDeath(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int playerIndex = buffer.ReadInt();
        int playerTeam = buffer.ReadInt();

        Network.instance.CallFunctionFromAnotherThread(() => {
            Network.instance.DestroyPlayer(playerIndex, playerTeam);
        });

    }

    // Packetnum = 12
    // TODO: This exists for points
    void HandleDealtDamage(byte[] data){
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        float damageDealt = buffer.ReadFloat();
        //Network.instance.CallFunctionFromAnotherThread(() => {
        //    if (Network.instance.player.playerObj.activeInHierarchy){
        //        Network.instance.player.playerObj.GetComponent<PlayerData>().UpdateDamageDealt(damageDealt);
        //    }
        //});
    }

    //Packetnum = 13
    void HandleLeaveGame(byte[] data) {
        //    ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        //    buffer.WriteBytes(data);
        //    float damageDealt = buffer.ReadFloat();
        Network.instance.CallFunctionFromAnotherThread(() => {
            Network.instance.LeaveGameLogic();
        });
    }

    //Packetnum = 14
    void HandleRecievePlayerBullet(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        string bulletId = buffer.ReadString();
        int bulletTeam = buffer.ReadInt();
        float posX = buffer.ReadFloat();
        float posY = buffer.ReadFloat();
        float posZ = buffer.ReadFloat();
        float rotY = buffer.ReadFloat();
        float speed = buffer.ReadFloat();
        float lifeTime = buffer.ReadFloat();
        float damage = buffer.ReadFloat();

        ObjectHandler.instance.CallFunctionFromAnotherThread(() => {
            ObjectHandler.instance.InstantiateBullet(new Vector3(posX, posY, posZ),
                                                     new Vector3(0, rotY, 0),
                                                     speed,
                                                     lifeTime,
                                                     bulletId,
                                                     damage,
                                                     bulletTeam);
        });
    }

    // PacketNum 15
    void HandleGameReady(byte[] data) {
        ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        buffer.WriteBytes(data);
        int gameReady = buffer.ReadInt();
        int numberOfFullRooms = buffer.ReadInt();
        float timer = buffer.ReadFloat();
        Network.instance.mainMenu.SetStartTimer(timer);
        Network.instance.CallFunctionFromAnotherThread(() => {
            Network.instance.SetGameReady(numberOfFullRooms, gameReady);
        });
    }
    #endregion
}


