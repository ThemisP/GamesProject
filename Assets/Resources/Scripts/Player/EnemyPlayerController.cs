﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyPlayerController : MonoBehaviour {

    public Slider healthSlider;

    [Header("Effects")]
    public ParticleSystem gunParticles;                    // Reference to the particle system.
    public AudioSource gunAudio;                           // Reference to the audio source.
    public Light gunLight;                                 // Reference to the light component.
    private Animator anim;

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

        if (health > 0)
            healthSlider.value = health / maxHealth;
        else
            healthSlider.value = 0;

        syncTime += Time.deltaTime;
        //Debug.Log("Player pos (" + playerPos + "), player rot (" + playerRot + ")");
        bool walking = false;
        if (Mathf.Abs(Vector3.Distance(transform.position, playerPos)) > 2f) walking = true;
        anim.SetBool("IsWalking", walking);
        if (syncDelay != 0) {
            playerRigidbody.MovePosition(Vector3.Lerp(transform.position, playerPos, syncTime / syncDelay));
        }
        playerRigidbody.rotation = Quaternion.Lerp(playerRigidbody.rotation, Quaternion.Euler(playerRot), .3f);
    }

    public void CallFunctionFromAnotherThread(Action functionName) {
        lock (RunOnMainThread) {
            RunOnMainThread.Enqueue(functionName);
        }
    }

    public void SetPlayerPosAndRot(Vector3 pos, Vector3 rot, Vector3 vel) {
        syncTime = 0f;
        syncDelay = Time.time - lastSynchronizationTime;
        lastSynchronizationTime = Time.time;
        //Debug.Log(pos + " :: " + rot);
        //Debug.Log("received " + rot);
        this.playerPos = pos + vel*syncDelay;
        this.playerRot = rot;
    }

    void Animating(float h, float v) {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = h != 0f || v != 0f;

        // Tell the animator whether or not the player is walking.
        anim.SetBool("IsWalking", walking);
    }

    public void Fire(Vector3 pos, Vector3 rot, float speed, float lifeTime, string bulletId, float damage, int bulletTeam) {
        // Play the gun shot audioclip.
        gunAudio.Play();

        // Enable the lights.
        gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        gunParticles.Stop();
        gunParticles.Play();

        ObjectHandler.instance.CallFunctionFromAnotherThread(() => {
            ObjectHandler.instance.InstantiateBullet(pos,
                                                     rot,
                                                     speed,
                                                     lifeTime,
                                                     bulletId,
                                                     damage,
                                                     bulletTeam);
        });

        // Enable the lights.
        gunLight.enabled = false;
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
    public void SetHealth(float amount) {
        this.health = amount;
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
