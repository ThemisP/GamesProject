using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyPlayerController : MonoBehaviour {

    public Slider healthSlider;
    public Text UsernameDisplay;

    [Header("Effects")]
    public ParticleSystem gunParticles;                    // Reference to the particle system.
    public AudioSource gunAudio;                           // Reference to the audio source.
    public Light gunLight;                                 // Reference to the light component.
    private Animator anim;

    [Header("Revive")]
    public GameObject reviveTrigger;
    public GameObject playerModel;
    public CapsuleCollider playerCollider;

    private int PlayerID;
    private string Username;
    private int TeamNumber;
    private float maxHealth = 100f;
    private float health = 100f;

    private Rigidbody playerRigidbody;
    private Vector3 playerPos;
    private Vector3 playerRot;
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;

    private bool dead = false;

    private Queue<Action> RunOnMainThread = new Queue<Action>();

    // Use this for initialization
    void Start () {
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null) Debug.LogError("Not found rigid body component");
        playerRigidbody.freezeRotation = true;
        playerPos = transform.position; 
        playerRot = transform.rotation.eulerAngles;
        anim = GetComponent<Animator>();
    }
	
	void FixedUpdate () {
        if (RunOnMainThread.Count > 0) {
            lock(RunOnMainThread) {
                Action s = RunOnMainThread.Dequeue();
                s();
            }
        }
        if (!dead) {
            if (health > 0)
                healthSlider.value = health / maxHealth;
            else
                healthSlider.value = 0;

            syncTime += Time.deltaTime;
            //Debug.Log("Player pos (" + playerPos + "), player rot (" + playerRot + ")");
            bool walking = false;
            if (Mathf.Abs(Vector3.Distance(transform.position, playerPos)) > 0.5f) walking = true;
            anim.SetBool("IsWalking", walking);
            if (syncDelay != 0) {
                playerRigidbody.MovePosition(Vector3.Lerp(transform.position, playerPos, syncTime / syncDelay));
            }
            playerRigidbody.rotation = Quaternion.Lerp(playerRigidbody.rotation, Quaternion.Euler(playerRot), .3f);
        }
    }

    public void CallFunctionFromAnotherThread(Action functionName) {
        lock (RunOnMainThread) {
            RunOnMainThread.Enqueue(functionName);
        }
    }

    public void SetPlayerPosAndRot(Vector3 pos, Vector3 rot, Vector3 vel, float health) {
        syncTime = 0f;
        syncDelay = Time.time - lastSynchronizationTime;
        lastSynchronizationTime = Time.time;
        //Debug.Log(pos + " :: " + rot);
        //Debug.Log("received " + rot);
        this.playerPos = pos + vel*syncDelay;
        this.playerRot = rot;
        this.health = health;
    }


    public void Fire(Vector3 pos, Vector3 rot, float speed, float lifeTime, string bulletId, float damage, int bulletTeam) {
        // Play the gun shot audioclip.
        gunAudio.Play();

        // Enable the lights.
        gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        gunParticles.Stop();
        gunParticles.Play();

        
        ObjectHandler.instance.InstantiateBullet(pos,rot,speed,lifeTime,bulletId,damage,bulletTeam);
        

        // Enable the lights.
        gunLight.enabled = false;
    }

    #region "Setters"
    public void SetUsername(string username) {
        this.Username = username;
        UsernameDisplay.text = username;
    }
    public void SetTeamNumber(int teamNumber) {
        this.TeamNumber = teamNumber;
    }
    public void SetPlayerId(int id) {
        this.PlayerID = id;
    }
    public void SetHealth(float amount) {
        this.health = amount;
    }
    public void Died() {
        dead = true;
        playerModel.SetActive(false);
        playerCollider.enabled = false;
        reviveTrigger.SetActive(true);
    }
    public void Revived() {
        dead = false;
        reviveTrigger.SetActive(false);
        playerCollider.enabled = true;
        playerModel.SetActive(true);
        health = 30f;
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
