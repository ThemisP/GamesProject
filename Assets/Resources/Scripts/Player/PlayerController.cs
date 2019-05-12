using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Resources.Scripts.Weapons;
using Assets.Resources.Scripts.Statuses;
using Assets.Resources.Scripts.Chain;


public class PlayerController : MonoBehaviour
{

    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float speed = 6f;


    [Header("Effects")]
    public ParticleSystem gunParticles;                    // Reference to the particle system.
    public AudioSource gunAudio;                           // Reference to the audio source.
    public Light gunLight;                                 // Reference to the light component.
    private Animator anim;


    Vector3 movement;
    Vector3 lastKnownPosition;
    Rigidbody playerRigidbody;
    TeamScript teamScript;
    /* Color playerColour = new Color();*/ //a player's colour usually
    Color dodgeColour; //a player's colour when dodging
    Color revertCol; //placeholder to revert colour back to normal once time has elapsed
    int floorMask;
    float camRayLength = 100f;
    float lastShootTime = 0;
    [Header("Player Abilities")]
    public float DodgeCooldown = 3f;

    [Header("Revive")]
    public GameObject reviveTrigger;
    public GameObject playerModel;
    public CapsuleCollider playerCollider;
    private bool dead = false;


    private int bulletCount = 0;//used for bullet id
    private float DodgeTimer = 0f;
    private float StatusTimer = 0f;
    private int clipCount;
    private PlayerData playerData;
    private bool offline = false;
    private bool isDodging = false;

    private bool AbleToRevive = false;
    private float TimeToRevive = 3f;
    private float MaxReviveTime = 3f;
    private float ReloadTime = 0f;
    private bool Reloaded = false;

    private bool waitingForGameStart = true;


    // Use this for initialization
    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        playerData = GetComponent<PlayerData>();

        anim = GetComponent<Animator>();
        clipCount = playerData.currentWeapon.GetMagazine();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!dead) {
            float h = 0f;// a and d keys
            float v = 0f; // w and s keys
            bool fire = false;//pressed primary mouse button
            bool hitDodge = false; //pressed rightClick
            bool hitReload = false;
            if(!waitingForGameStart) {
                h = Input.GetAxisRaw("Horizontal");// a and d keys
                v = Input.GetAxisRaw("Vertical"); // w and s keys
                fire = Input.GetMouseButton(0);//pressed primary mouse button
                hitDodge = Input.GetMouseButton(1); //pressed rightClick
                hitReload = Input.GetKeyDown(KeyCode.R);
            }
            bool hitHelp = Input.GetKey(KeyCode.H);
            bool hitWeaponsUpgrade = Input.GetKey(KeyCode.E);
            bool hitStatus = Input.GetKey(KeyCode.Q);
            bool reviving = Input.GetKey(KeyCode.F);

            if (AbleToRevive) {
                if (reviving) {
                    TimeToRevive += Time.deltaTime;
                    playerData.Reviving(TimeToRevive / MaxReviveTime);
                    if (TimeToRevive > MaxReviveTime) {
                        Network.instance.RevivedTeammate();
                        Debug.Log("Revive teammate");
                        TimeToRevive = 0f;
                    }
                    return;
                } else {
                    playerData.StopReviving();
                    TimeToRevive = 0f;
                }
            }

            Move(h, v);

            playerData.PopupHelp(hitHelp);
            playerData.PopupWeapons(hitWeaponsUpgrade);
            // playerData.PopupStatuses(hitStatus);

            speed = playerData.currentStatus.GetSpeed();
            // IsDodging();
            // playerData.takeDamage(playerData.currentStatus.GetDamage());

            UpdateDodgeTimer();
            UpdateStatusTimer();

            if (hitReload && clipCount < playerData.currentWeapon.GetMagazine()) {                
                ReloadTime = playerData.currentWeapon.GetReloadTime();
                Reloaded = true;
                playerData.ReloadPopupStop();
            }
            Turning();
            Animating(h, v);
            if (ReloadTime >= 0) {
                ReloadTime -= Time.deltaTime;
            } else {
                if (Reloaded) {
                    Reload();
                    Reloaded = false;
                }
            }
            if (!hitWeaponsUpgrade) {
                if (ReloadTime <= 0) {
                    Fire(fire);
                }
            }
            Dodge(hitDodge);
        }
    }

    void UpdateDodgeTimer()
    {
        if (DodgeTimer < DodgeCooldown) {
            DodgeTimer += Time.deltaTime;
        } else {
            DodgeTimer = DodgeCooldown;
        }
        if (DodgeTimer > 0.5f) {
            isDodging = false;

            //anim.SetBool("Dodging", isDodging);
        } else {
            isDodging = true;

            //anim.SetBool("Dodging", isDodging);
        }
        playerData.DodgeCooldown(DodgeCooldown, DodgeTimer);
    }

    void UpdateStatusTimer()
    {
        if (playerData.currentStatus.Equals(Statuses.instance.GetHealthy()))
        {
            return;
        }
        else
        {
            if (StatusTimer < playerData.currentStatus.GetDuration())
            {
                StatusTimer += Time.deltaTime;
            }
            playerData.StatusCooldown(StatusTimer);
            if (StatusTimer > playerData.currentStatus.GetDuration())
            {
                StatusTimer = 0f;
            }
        }
    }

    void Move(float h, float v)
    {
        movement.Set(h, 0f, v);
        //Normalise the movement vector to make it proportional to the speed per second
        //Deltatime is the step for the game timer
        if(isDodging) movement = movement.normalized * (speed+10) * Time.deltaTime;
        else movement = movement.normalized * speed * Time.deltaTime;
        //if (teamScript != null) {
        //    Vector3 springForce;
        //    springForce = teamScript.movementModifier(playerRigidbody.velocity);
        //    playerRigidbody.AddForce(springForce);
        //}

        playerRigidbody.MovePosition(transform.position + movement);
    }

    void Turning()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit floorHit;

        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;

            playerToMouse.y = 0f;

            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

            playerRigidbody.MoveRotation(newRotation);
        }
    }

    void Fire(bool fire)
    {
        if (lastShootTime < 100f)
            lastShootTime += Time.deltaTime;
        if (fire)
        {
            if (lastShootTime >= playerData.currentWeapon.GetFirerate())
            {
                if (clipCount > 0) {
                    FireGun();
                } else {
                    playerData.ReloadPopupShow();
                }
            }
        }
    }

    void FireGun() {
        string bulletId;
        lastShootTime = 0f;
        Vector3 rotation;
        // Play the gun shot audioclip.
        gunAudio.Play();

        // Enable the lights.
        gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        gunParticles.Stop();
        gunParticles.Play();
        for (int i = 0; i <playerData.currentWeapon.GetNumberOfBullets(); i++) {
            bulletId = Network.instance.ClientIndex.ToString() + "_" + bulletCount.ToString();
            rotation = bulletSpawn.rotation.eulerAngles;
            if(i!=0) rotation += Vector3.up * ((float)Math.Pow(-1, i) * 10); // this is an equation to get alternating + and - for each step.
            ObjectHandler.instance.InstantiateBullet(bulletSpawn.position, rotation,
                                                        playerData.currentWeapon.GetSpeed(),
                                                        playerData.currentWeapon.GetLifetime(),
                                                        bulletId, playerData.currentWeapon.GetDamage(),
                                                        Network.instance.player.GetTeamNumber());
            if(!offline) Network.instance.SendBullet(bulletSpawn.position, rotation.y,
                                        playerData.currentWeapon.GetSpeed(),
                                        playerData.currentWeapon.GetLifetime(), bulletId,
                                        playerData.currentWeapon.GetDamage());
            bulletCount = (bulletCount + 1) % 1000;
        }

        clipCount--;

        // Enable the lights.
        gunLight.enabled = false;
    }

    void Dodge(bool hitDodge)
    {
        if (hitDodge)
        {
            if (DodgeTimer >= DodgeCooldown)
            {
                DodgeTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.tag == "EnemyBullet") {
            if (!isDodging) {
                if (!dead) {
                    BulletScript bulletScript = obj.GetComponent<BulletScript>();
                    //check if teammate its friendly bullet;
                    //if(bulletScript.GetBulletId().StartsWith(Network.instance.player.GetTeammateUsername()))
                    if (bulletScript.GetBulletTeam() == Network.instance.player.GetTeamNumber()) {
                    } else {
                        playerData.takeDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());

                        if (!offline) Network.instance.SendPlayerDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());
                        ObjectHandler.instance.DestroyBullet(bulletScript.GetBulletId());
                        CameraFollow script = Camera.main.GetComponent<CameraFollow>();
                        script.ShakeCamera();
                    }
                }
            }
        } else if(obj.tag == "Revive") {
            Debug.Log(obj.tag);
            AbleToRevive = true;
            playerData.ReviveButton(true);
        } else if (obj.CompareTag("Coin")) {
            ObjectHandler.instance.DisableCoin(obj.name);
            if(!offline) Network.instance.SendCollectiblesDestroy(obj.name, 0);// 0 for coins 1 for pills
            bool succcess = playerData.addCoinsIfAvailable(10);
        } else if (obj.CompareTag("Pill")) {
            if (playerData.RefreshHealth(20f)) {
                ObjectHandler.instance.DisablePill(obj.name);
                if (!offline) Network.instance.SendCollectiblesDestroy(obj.name, 1);// 0 for coins 1 for pills
            }
        }
    }

    private void OnTriggerExit(Collider collision) {
        GameObject obj = collision.gameObject;
        if(obj.tag == "Revive") {
            Debug.Log("Stop reviving");
            AbleToRevive = false;
            playerData.ReviveButton(false);
        }
    }
     
    void Animating(float h, float v) {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = h != 0f || v != 0f;

        // Tell the animator whether or not the player is walking.
        anim.SetBool("IsWalking", walking);
    }

    #region "setters"

    public void SetWaitingForGame(bool waiting) {
        waitingForGameStart = waiting;
    }
    public void SetOffline(bool set)
    {
        this.offline = set;
    }

    public void SetTeamController(TeamScript script)
    {
        this.teamScript = script;
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
        playerData.SethealthFromRevive();
    }

    public void Reload() {
        this.clipCount = playerData.currentWeapon.GetMagazine();
    }
    #endregion
    #region "Getters"
    public bool isOffline() {
        return this.offline;
    }

    public int GetBulletsLeft() {
        return this.clipCount;
    }
    #endregion
}
