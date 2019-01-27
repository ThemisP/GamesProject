using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerController : MonoBehaviour {

    private int PlayerID;
    private string Username;
    private int TeamNumber;

    private Rigidbody playerRigidbody;
    private Vector3 playerPos;
    private Vector3 playerRot;
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;

    // Use this for initialization
    void Start () {
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null) Debug.LogError("Not found rigid body component");
        playerRigidbody.freezeRotation = true;
        //playerPos = transform.position;
        //playerRot = transform.rotation.eulerAngles;
    }
	
	void FixedUpdate () {
        syncTime += Time.deltaTime;
        Debug.Log("Player pos (" + playerPos + "), player rot (" + playerRot + ")");
        if (syncDelay > 0)
            playerRigidbody.MovePosition(Vector3.Lerp(transform.position, playerPos, syncTime / syncDelay));
        //playerRigidbody.rotation = Quaternion.Lerp(playerRigidbody.rotation, Quaternion.Euler(playerRot), .3f);
    }

    public void SetPlayerPosAndRot(Vector3 pos, Vector3 rot) {
        syncTime = 0f;
        syncDelay = Time.time - lastSynchronizationTime;
        lastSynchronizationTime = Time.time;
        Debug.Log(pos + " :: " + rot);
        this.playerPos = pos;
        this.playerRot = rot;
    }

    #region "Setters"
    public void SetUsername(string username) {
        this.Username = username;
    }
    public void SetTeamNumber(int teamNumber) {
        this.TeamNumber = teamNumber;
    }
    public void SetPlayerId(int id) {
        this.PlayerID = id;
    }
    #endregion

    #region "Getters"
    public string GetUsername() {
        return this.Username;
    }
    public int GetTeamNumber() {
        return this.TeamNumber;
    }
    #endregion
}
