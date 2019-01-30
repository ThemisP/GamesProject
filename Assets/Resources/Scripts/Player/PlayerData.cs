using Assets.Resources.Scripts.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour {

	[SerializeField] private float maxHealth = 100f;
	private Slider healthSlider;
	private float currentHealth;
    private Text coinCount;
    private Text nodeCount;
    private Slider dodgeSlider;
    private int kills = 0, assists = 0, deaths = 0;

    private GameObject HUDCanvas;
    private GameObject popupHelp;
    private GameObject popupWeapon;
    private PlayerController playerController;

	private int coins = 0;
    private int nodePoints = 0;
    private float damageDealt = 0;

    //Change back to Start when fixed HUD
    void Start() {
        HUDCanvas = GameObject.Find("HUDPrefab");
        RectTransform hud = HUDCanvas.GetComponent<RectTransform>();
        GameObject canvas = GameObject.Find("Health");
        GameObject balance = GameObject.Find("Balance");
        GameObject nodes = GameObject.Find("NodesBalance");
        popupHelp = hud.Find("Help_Popup").gameObject;
        popupWeapon = hud.Find("Weapons_Popup").gameObject;
        dodgeSlider = hud.Find("Dodge Cooldown").GetComponent<Slider>();
        
        Button pistolButton = popupWeapon.GetComponent<RectTransform>().Find("Pistol").GetComponent<Button>();
        Button shotgunButton = popupWeapon.GetComponent<RectTransform>().Find("Shotgun").GetComponent<Button>();
        Button rifleButton = popupWeapon.GetComponent<RectTransform>().Find("Rifle").GetComponent<Button>();
        Button sniperButton = popupWeapon.GetComponent<RectTransform>().Find("Sniper").GetComponent<Button>();


        pistolButton.onClick.AddListener(() => ChangeWeapon(0, playerController));
        shotgunButton.onClick.AddListener(() => ChangeWeapon(1, playerController));
        rifleButton.onClick.AddListener(() => ChangeWeapon(2, playerController));
        sniperButton.onClick.AddListener(() => ChangeWeapon(3, playerController));

        HUDCanvas.SetActive(true);
        popupHelp.SetActive(false);
        popupWeapon.SetActive(false);
        healthSlider = canvas.GetComponent<Slider>();
        coinCount = balance.GetComponent<Text>();
        coinCount.text = "0";
        nodeCount = nodes.GetComponent<Text>();
        nodeCount.text = "0";
        currentHealth = maxHealth;
        healthSlider.value = currentHealth / maxHealth;
    }
    public void SetPlayerController(PlayerController pc) {
        this.playerController = pc;
        Debug.Log(this.playerController);
    }

    void ChangeWeapon(int weaponNumber, PlayerController pc) {
        switch (weaponNumber) {
            case 0:
                pc.SetWeapon(Weapons.instance.GetPistol());
                break;
            case 1:
                pc.SetWeapon(Weapons.instance.GetShotgun());
                break;
            case 2:
                Debug.Log(playerController);
                Debug.Log(Weapons.instance.GetAssaultRifle());
                pc.SetWeapon(Weapons.instance.GetAssaultRifle());
                break;
            case 3:
                pc.SetWeapon(Weapons.instance.GetSniper());
                break;
            default:
                Debug.Log("Incorrect weapon number in change weapon");
                break;
        }
    }

    public void takeDamage(float amount){
        if(true){
             if(currentHealth - amount > 0){
                currentHealth -= amount;
                healthSlider.value = currentHealth/maxHealth;
            } else {
                currentHealth = 0;
                healthSlider.value = currentHealth/maxHealth;
            }
        }
       
    }
    //finds the amount of skill points held by the user
    public int getSkillPoints()
    {
        return coins;
    }

    public void DodgeCooldown(float cooldown, float timer) {
        if (timer > cooldown) {
            dodgeSlider.value = 1;
        } else {
            dodgeSlider.value = timer / cooldown;
        }
    }

    //updates the skill points held by the user
    public bool addCoinsIfAvailable(int newCoins)
    {
        //This statement is when we use -ve value coins to buy items,
        // it checks whether there are enough coins to use.
        if(this.coins + newCoins > 0){
            coins += newCoins;
            return true;
        } else {
            return false;
        }
        
    }

    public int getNodePoints()
    {
        return nodePoints;
    }

    public void addNodePoints(int newPoints)
    {
        nodePoints += newPoints;
    }

    public void PopupHelp(bool active) {
        if (active) {
            popupHelp.SetActive(true);
        } else {
            popupHelp.SetActive(false);
        }        
    }

    public void PopupWeapons(bool active) {
        if (active) {
            popupWeapon.SetActive(true);
        } else {
            popupWeapon.SetActive(false);
        }
    }

    public float getCurrentHealth()
    {
        return currentHealth;
    }  

    public void UpdateDamageDealt(float damageDealt){
        this.damageDealt += damageDealt;
    }   
}
