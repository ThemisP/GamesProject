using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Resources.Scripts.Weapons;

public class PlayerController : MonoBehaviour {

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform bulletSpawn;
	[SerializeField] private float speed = 6f;
	[SerializeField] private Network network;
	Vector3 movement;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;
	float lastShootTime = 0;

    [Header("Player Abilities")]
    public float DodgeCooldown = 3f;


    private int bulletCount = 0;//used for bullet id
    private float DodgeTimer = 0f;
    private Weapon currentWeapon;

	private PlayerData playerData;

	// Use this for initialization
	void Awake () {
        currentWeapon = Weapons.instance.GetPistol();
		floorMask = LayerMask.GetMask("Floor");
		playerRigidbody = GetComponent<Rigidbody>();
		playerRigidbody.freezeRotation = true;
		playerData = GetComponent<PlayerData>();
        playerData.SetPlayerController(this);
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
			if(lastShootTime>=currentWeapon.GetFirerate()){
                FireGun();
			}
		} 
	}

    void FireGun() {
        for(int i = 0; i <currentWeapon.GetNumberOfBullets(); i++) {
            string bulletId = Network.instance.ClientIndex.ToString() + "_" + bulletCount.ToString();
            if (i == 0) {
                Vector3 rotation = bulletSpawn.rotation.eulerAngles;
                ObjectHandler.instance.InstantiateBullet(bulletSpawn.position, rotation, 
                                                          currentWeapon.GetSpeed(), currentWeapon.GetLifetime(), 
                                                          bulletId);
                Network.instance.SendBullet(bulletSpawn.position, rotation.y, 
                                            currentWeapon.GetSpeed(), currentWeapon.GetLifetime(), bulletId);
            } else if (i == 1) {
                Vector3 rotation = bulletSpawn.rotation.eulerAngles;
                rotation += Vector3.up * +10;
                ObjectHandler.instance.InstantiateBullet(bulletSpawn.position, rotation,
                                                          currentWeapon.GetSpeed(), currentWeapon.GetLifetime(),
                                                          bulletId);
                Network.instance.SendBullet(bulletSpawn.position, rotation.y,
                                            currentWeapon.GetSpeed(), currentWeapon.GetLifetime(), bulletId);
            } else if(i == 2) {
                Vector3 rotation = bulletSpawn.rotation.eulerAngles;
                rotation += Vector3.up * -10;
                ObjectHandler.instance.InstantiateBullet(bulletSpawn.position, rotation,
                                                          currentWeapon.GetSpeed(), currentWeapon.GetLifetime(),
                                                          bulletId);
                Network.instance.SendBullet(bulletSpawn.position, rotation.y,
                                            currentWeapon.GetSpeed(), currentWeapon.GetLifetime(), bulletId);
            }
            bulletCount = (bulletCount + 1) % 1000;
            lastShootTime = 0f;
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
            ObjectHandler.instance.DestroyBullet(bulletScript.GetBulletId());
            playerData.takeDamage(bulletScript.GetBulletDamage());
            Network.instance.SendDestroyBullet(bulletScript.GetBulletId());
			if (playerData.getCurrentHealth() <= 0) {
				Network.instance.HandlePlayerDeath(bulletScript.GetBulletId());
			}
        } 
    }

    #region "setters"
    public void SetWeapon(Weapon weapon) {
        this.currentWeapon = weapon;
    }
    #endregion

}
