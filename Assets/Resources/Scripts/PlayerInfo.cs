using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {
    private string username;
    private bool inRoom;
    private int roomIndex;
    private string teammateUsername;
    private int teammateIndex;
    private int teamNumber;
    public int playerNumber = 1; // used for spawn point purposes
    private int gameIndex;
    private bool offline = false;

    public GameObject playerObj;
    private PlayerController playerController;

    
    public PlayerInfo() {
        this.username = "Default";
        inRoom = false;
    }

    #region "Setters"
    public void SetTeammateUsername(string username) {
        this.teammateUsername = username;
    }

    public void SetTeammate(int index, string username) {
        this.teammateUsername = username;
        this.teammateIndex = index;
    }

    public void SetGameIndex(int gameindex) {
        this.gameIndex = gameindex;
    }
    #endregion
    public void SetPlayerObj(GameObject playerObj) {
        this.playerObj = playerObj;
        this.playerController = playerObj.GetComponent<PlayerController>();
    }

    public void ChangeUsername(string username) {
        this.username = username;
    }

    public void SetTeamNumber(int number) {
        this.teamNumber = number;
    }

    public void SetOffline(bool set) {
        this.offline = set;
        this.playerController.SetOffline(set);
    }

    public void JoinRoom(int roomIndex) {
        this.roomIndex = roomIndex;
        this.inRoom = true;
    }

    public void LeaveRoom() {
        this.inRoom = false;
    }

    public bool InRoom() {
        return this.inRoom;
    }

    public int GetRoomIndex() {
        return this.roomIndex;
    }

    public string GetUsername() {
        return this.username;
    }

    public string GetTeammateUsername() {
        return this.teammateUsername;
    }

    public bool isOffline() {
        return this.offline;
    }
    public int GetTeamNumber() {
        return this.teamNumber;
    }
	
    public GameObject GetPlayerObj() {
        return this.playerObj;
    }

    public int GetGameIndex() {
        return this.gameIndex;
    }
}
