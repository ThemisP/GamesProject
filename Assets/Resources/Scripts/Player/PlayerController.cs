using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Resources.Scripts.Weapons;
using Assets.Resources.Scripts.Statuses;
using Assets.Resources.Scripts.Chain;


public class PlayerController : MonoBehaviour
{

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float speed = 6f;
    [SerializeField] private Network network;
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


    private int bulletCount = 0;//used for bullet id
    private float DodgeTimer = 0f;
    private float StatusTimer = 0f;

    private PlayerData playerData;
    private bool offline = false;

    // Use this for initialization
    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        playerData = GetComponent<PlayerData>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");// a and d keys
        float v = Input.GetAxisRaw("Vertical"); // w and s keys
        bool fire = Input.GetMouseButton(0);//pressed primary mouse button
        bool hitDodge = Input.GetKey(KeyCode.F);

        bool hitHelp = Input.GetKey(KeyCode.H);
        bool hitWeaponsUpgrade = Input.GetKey(KeyCode.E);
        bool hitStatus = Input.GetKey(KeyCode.Q);

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
        if (!hitWeaponsUpgrade)
        {
            Fire(fire);
        }
        Dodge(hitDodge);
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
                                                        bulletId, playerData.currentWeapon.GetDamage(),
                                                        Network.instance.player.GetTeamNumber());
            if(!offline) Network.instance.SendBullet(bulletSpawn.position, rotation.y,
                                        playerData.currentWeapon.GetSpeed(), 
                                        playerData.currentWeapon.GetLifetime(), bulletId,
                                        playerData.currentWeapon.GetDamage());
            bulletCount = (bulletCount + 1) % 1000;
        }
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
        if (obj.tag == "Bullet") {
            BulletScript bulletScript = obj.GetComponent<BulletScript>();
            //check if teammate its friendly bullet;
            //if(bulletScript.GetBulletId().StartsWith(Network.instance.player.GetTeammateUsername()))
            if (bulletScript.GetBulletTeam() == Network.instance.player.GetTeamNumber()) {
            } else { 
                playerData.takeDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());

                if (!offline) Network.instance.SendPlayerDamage(bulletScript.GetBulletDamage(), bulletScript.GetBulletId());
                ObjectHandler.instance.DestroyBullet(bulletScript.GetBulletId());
            }
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

    #region "setters"
    public void SetOffline(bool set)
    {
        this.offline = set;
    }

    public void SetTeamController(TeamScript script)
    {
        this.teamScript = script;
    }
    #endregion

}
