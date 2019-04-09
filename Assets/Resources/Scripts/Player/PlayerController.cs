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
    public bool isDodging;

    [Header("Revive")]
    public GameObject reviveTrigger;
    public GameObject playerModel;
    public CapsuleCollider playerCollider;
    private bool dead = false;

    private int bulletCount = 0;//used for bullet id
    private float DodgeTimer = 0f;
    private float StatusTimer = 0f;
    public int clipCount;
    private PlayerData playerData;
    private bool offline = false;

    private bool AbleToRevive = false;
    private float TimeToRevive = 3f;
    private float MaxReviveTime = 3f;

    // Use this for initialization
    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        playerData = GetComponent<PlayerData>();
        anim = GetComponent<Animator>();
        clipCount = playerData.currentWeapon.GetMagazine(); //current number of bullets in clip 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!dead)
        {
            float h = Input.GetAxisRaw("Horizontal");// a and d keys
            float v = Input.GetAxisRaw("Vertical"); // w and s keys
            bool fire = Input.GetMouseButton(0);//pressed primary mouse button
            bool hitDodge = Input.GetMouseButton(1); //pressed rightClick
            bool reload = Input.GetKey(KeyCode.R);
            bool hitHelp = Input.GetKey(KeyCode.H);
            bool hitWeaponsUpgrade = Input.GetKey(KeyCode.E);
            bool hitStatus = Input.GetKey(KeyCode.Q);
            bool reviving = Input.GetKey(KeyCode.F);

            if (AbleToRevive)
            {
                if (reviving)
                {
                    TimeToRevive += Time.deltaTime;
                    playerData.Reviving(TimeToRevive / MaxReviveTime);
                    if (TimeToRevive > MaxReviveTime)
                    {
                        Network.instance.RevivedTeammate();
                        Debug.Log("Revive teammate");
                        TimeToRevive = 0f;
                    }
                    return;
                }
                else
                {
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

            Turning();
            Animating(h, v);
            if (!hitWeaponsUpgrade)
            {
                Fire(fire);
                if (fire)
                {
                    playerData.UpdateMagazine();
                }
                Reload(reload);
                if (reload)
                {
                    playerData.UpdateMagazine();
                }
            }
            Dodge(hitDodge);
        }
    }

    void UpdateDodgeTimer()
    {
        if (DodgeTimer < DodgeCooldown)
            DodgeTimer += Time.deltaTime;
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
        movement = movement.normalized * speed * Time.deltaTime;
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
                //if clip is empty, gun cannot be fired
                if (clipCount != 0)
                {
                    FireGun();
                }
            }
        }
    }

    void Reload(bool reload)
    {
        if (reload)
        {
            clipCount = playerData.currentWeapon.GetMagazine();
        }
    }

    void FireGun()
    {
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
        for (int i = 0; i < playerData.currentWeapon.GetNumberOfBullets(); i++)
        {
            bulletId = Network.instance.ClientIndex.ToString() + "_" + bulletCount.ToString();
            rotation = bulletSpawn.rotation.eulerAngles;
            if (i != 0) rotation += Vector3.up * ((float)Math.Pow(-1, i) * 10); // this is an equation to get alternating + and - for each step.
            ObjectHandler.instance.InstantiateBullet(bulletSpawn.position, rotation,
                                                        playerData.currentWeapon.GetSpeed(),
                                                        playerData.currentWeapon.GetLifetime(),
                                                        bulletId, playerData.currentWeapon.GetDamage(),
                                                        Network.instance.player.GetTeamNumber());
            if (!offline) Network.instance.SendBullet(bulletSpawn.position, rotation.y,
                                         playerData.currentWeapon.GetSpeed(),
                                         playerData.currentWeapon.GetLifetime(), bulletId,
                                         playerData.currentWeapon.GetDamage());
            bulletCount = (bulletCount + 1) % 1000;
            clipCount--;
        }

        // Enable the lights.
        gunLight.enabled = false;
    }

    void Dodge(bool hitDodge)
    {
        if (hitDodge)
        {
            if (DodgeTimer > DodgeCooldown)
            {
                DodgeTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.tag == "Bullet")
        {
            BulletScript bulletScript = obj.GetComponent<BulletScript>();
            //check if teammate its friendly bullet;
            //if(bulletScript.GetBulletId().StartsWith(Network.instance.player.GetTeammateUsername()))
            if (bulletScript.GetBulletTeam() == Network.instance.player.GetTeamNumber())
            {
            }
            else
            {
                playerData.takeDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());

                if (!offline) Network.instance.SendPlayerDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());
                ObjectHandler.instance.DestroyBullet(bulletScript.GetBulletId());
            }
        }
        else if (obj.tag == "Revive")
        {
            Debug.Log("Reviving");
            AbleToRevive = true;
            playerData.ReviveButton(true);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.tag == "Revive")
        {
            Debug.Log("Stop reviving");
            AbleToRevive = false;
            playerData.ReviveButton(false);
        }
        else if (obj.CompareTag("Coin")){
            obj.SetActive(false);
            Boolean succcess = playerData.addCoinsIfAvailable(10);
        }
        else if (obj.CompareTag("Pill")){
            obj.SetActive(false);
            playerData.RefreshHealth();
        }
    }

    public void IsDodging()
    {
        if (Input.GetKey(KeyCode.F) && (DodgeTimer == DodgeCooldown))
        {
            isDodging = true;
        }
        else
        {
            isDodging = false;
        }
    }

    void Animating(float h, float v)
    {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = h != 0f || v != 0f;

        // Tell the animator whether or not the player is walking.
        anim.SetBool("IsWalking", walking);
    }

    #region "setters"
    public void SetOffline(bool set)
    {
        this.offline = set;
    }

    public void SetTeamController(TeamScript script)
    {
        this.teamScript = script;
    }
    public void Died()
    {
        dead = true;
        playerModel.SetActive(false);
        playerCollider.enabled = false;
        reviveTrigger.SetActive(true);
    }
    public void Revived()
    {
        dead = false;
        reviveTrigger.SetActive(false);
        playerCollider.enabled = true;
        playerModel.SetActive(true);
        playerData.SethealthFromRevive();
    }
    #endregion

}
