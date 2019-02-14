﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Resources.Scripts.Weapons;

public class PlayerController : MonoBehaviour {

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform bulletSpawn;
	[SerializeField] private float speed = 8f;
	[SerializeField] private Network network;
	Vector3 movement;
	Rigidbody playerRigidbody;
   /* Color playerColour = new Color();*/ //a player's colour usually
    Color dodgeColour; //a player's colour when dodging
    Color revertCol; //placeholder to revert colour back to normal once time has elapsed
    int floorMask;
	float camRayLength = 100f;
	float lastShootTime = 0;

    [Header("Player Abilities")]
    public float DodgeCooldown = 3f;


    private int bulletCount = 0;//used for bullet id
    private float DodgeTimer = 0f;

	private PlayerData playerData;
    private bool offline = false;

	// Use this for initialization
	void Awake () {
		floorMask = LayerMask.GetMask("Floor");
		playerRigidbody = GetComponent<Rigidbody>();
		playerRigidbody.freezeRotation = true;
		playerData = GetComponent<PlayerData>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float h = Input.GetAxisRaw("Horizontal");// a and d keys
		float v = Input.GetAxisRaw("Vertical"); // w and s keys
		bool fire = Input.GetMouseButton(0);//pressed primary mouse button
        bool hitDodge = Input.GetKey(KeyCode.F);

        bool hitHelp = Input.GetKey(KeyCode.H);
        bool hitWeaponsUpgrade = Input.GetKey(KeyCode.E);


        playerData.PopupHelp(hitHelp);
        playerData.PopupWeapons(hitWeaponsUpgrade);

        updateDodgeTimer();

		Move(h,v);
		Turning();
        if (!hitWeaponsUpgrade) {
            Fire(fire);
        }
        Dodge(hitDodge);
	}

    void updateDodgeTimer() {
        if (DodgeTimer < DodgeCooldown)
            DodgeTimer += Time.deltaTime;
        playerData.DodgeCooldown(DodgeCooldown, DodgeTimer);
    }

	void Move(float h, float v){
		movement.Set(h, 0f, v);
		//Normalise the movement vector to make it proportional to the speed per second
		//Deltatime is the step for the game timer
		movement = movement.normalized * speed * Time.deltaTime;
		playerRigidbody.MovePosition(transform.position + movement);
	}

	void Turning(){
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		RaycastHit floorHit;

		if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask)){
			Vector3 playerToMouse = floorHit.point - transform.position;

			playerToMouse.y = 0f;

			Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

			playerRigidbody.MoveRotation(newRotation);
		}
	}

	void Fire(bool fire){
        if(lastShootTime<100f)
            lastShootTime += Time.deltaTime;
		if(fire){
			if(lastShootTime>=playerData.currentWeapon.GetFirerate()){
                FireGun();
			}
		} 
	}

    void FireGun() {
        string bulletId;
        lastShootTime = 0f;
        Vector3 rotation;
        for (int i = 0; i <playerData.currentWeapon.GetNumberOfBullets(); i++) { 
            bulletId = Network.instance.ClientIndex.ToString() + "_" + bulletCount.ToString();
            rotation = bulletSpawn.rotation.eulerAngles;
            if(i!=0) rotation += Vector3.up * ((float)Math.Pow(-1, i) * 10);
            ObjectHandler.instance.InstantiateBullet(bulletSpawn.position, rotation,
                                                        playerData.currentWeapon.GetSpeed(), 
                                                        playerData.currentWeapon.GetLifetime(), 
                                                        bulletId, playerData.currentWeapon.GetDamage());
            if(!offline) Network.instance.SendBullet(bulletSpawn.position, rotation.y,
                                        playerData.currentWeapon.GetSpeed(), 
                                        playerData.currentWeapon.GetLifetime(), bulletId,
                                        playerData.currentWeapon.GetDamage());
            bulletCount = (bulletCount + 1) % 1000;
        }
    }

    void Dodge(bool hitDodge) {
        if (hitDodge) {            
            if (DodgeTimer > DodgeCooldown) {
                DodgeTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider collision) {
        GameObject obj = collision.gameObject;
        Debug.Log("triggered");
        if (obj.tag == "Bullet") {
            BulletScript bulletScript = obj.GetComponent<BulletScript>();
            //check if teammate its friendly bullet;
            //if(bulletScript.GetBulletId().StartsWith(Network.instance.player.GetTeammateUsername()))
            playerData.takeDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());

            if (!offline) Network.instance.SendPlayerDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());
            ObjectHandler.instance.DestroyBullet(bulletScript.GetBulletId());
        } 
    }

    #region "setters"
    public void SetOffline(bool set) {
        this.offline = set;
    }
    #endregion

}
