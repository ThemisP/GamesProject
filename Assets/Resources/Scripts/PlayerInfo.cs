using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {
    private string username;
    private bool inRoom;
    private int roomIndex;
    private string teammateUsername;
    
    public PlayerInfo() {
        this.username = "Default";
        inRoom = false;
    }

    public void SetTeammateUsername(string username) {
        this.teammateUsername = username;
    }

    public void ChangeUsername(string username) {
        this.username = username;
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
	
}
