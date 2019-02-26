using Assets.Resources.Scripts.Weapons;
using Assets.Resources.Scripts.Statuses;
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
    private GameObject popupStatus;
    private PlayerController playerController;

	private int coins = 0;
    private int nodePoints = 0;
    private float damageDealt = 0;

    public Weapon currentWeapon = Weapons.instance.GetPistol();
    public Status currentStatus = Statuses.instance.GetHealthy();

    //Change back to Start when fixed HUD
    void Start() {
        HUDCanvas = GameObject.Find("HUDPrefab");
        RectTransform hud = HUDCanvas.GetComponent<RectTransform>();
        GameObject canvas = GameObject.Find("Health");
        GameObject balance = GameObject.Find("Balance");
        GameObject nodes = GameObject.Find("NodesBalance");
        popupHelp = hud.Find("Help_Popup").gameObject;
        popupWeapon = hud.Find("Weapons_Popup").gameObject;
        popupStatus = hud.Find("Statuses_Popup").gameObject;
        dodgeSlider = hud.Find("Dodge Cooldown").GetComponent<Slider>();
        
        Button pistolButton = popupWeapon.GetComponent<RectTransform>().Find("Pistol").GetComponent<Button>();
        Button shotgunButton = popupWeapon.GetComponent<RectTransform>().Find("Shotgun").GetComponent<Button>();
        Button rifleButton = popupWeapon.GetComponent<RectTransform>().Find("Rifle").GetComponent<Button>();
        Button sniperButton = popupWeapon.GetComponent<RectTransform>().Find("Sniper").GetComponent<Button>();


        pistolButton.onClick.AddListener(() => ChangeWeapon(0));
        shotgunButton.onClick.AddListener(() => ChangeWeapon(1));
        rifleButton.onClick.AddListener(() => ChangeWeapon(2));
        sniperButton.onClick.AddListener(() => ChangeWeapon(3));

        Button healthyButton = popupStatus.GetComponent<RectTransform>().Find("Healthy").GetComponent<Button>();
        Button burntButton = popupStatus.GetComponent<RectTransform>().Find("Burnt").GetComponent<Button>();
        Button poisonedButton = popupStatus.GetComponent<RectTransform>().Find("Poisoned").GetComponent<Button>();
        Button invincibleButton = popupStatus.GetComponent<RectTransform>().Find("Invincible").GetComponent<Button>();
        Button paralyzedButton = popupStatus.GetComponent<RectTransform>().Find("Paralyzed").GetComponent<Button>();


        // healthyButton.onClick.AddListener(() => ChangeStatus(0));
        // burntButton.onClick.AddListener(() => ChangeStatus(1));
        // poisonedButton.onClick.AddListener(() => ChangeStatus(2));
        // invincibleButton.onClick.AddListener(() => ChangeStatus(3));
        // paralyzedButton.onClick.AddListener(() => ChangeStatus(4));


        HUDCanvas.SetActive(true);
        popupHelp.SetActive(false);
        popupWeapon.SetActive(false);
        popupStatus.SetActive(false);
        healthSlider = canvas.GetComponent<Slider>();
        coinCount = balance.GetComponent<Text>();
        coinCount.text = "0";
        nodeCount = nodes.GetComponent<Text>();
        nodeCount.text = "0";
        currentHealth = maxHealth;
        healthSlider.value = currentHealth / maxHealth;
    }

    void ChangeWeapon(int weaponNumber) {
        switch (weaponNumber) {
            case 0:
                this.currentWeapon = Weapons.instance.GetPistol();
                break;
            case 1:
                this.currentWeapon = Weapons.instance.GetShotgun();
                break;
            case 2:
                this.currentWeapon =  Weapons.instance.GetAssaultRifle();
                break;
            case 3:
                this.currentWeapon = Weapons.instance.GetSniper();
                break;
            default:
                Debug.Log("Incorrect weapon number in change weapon");
                break;
        }
    }

    public void takeDamage(float amount, string bulletId){
        currentHealth -= amount;
        if (currentHealth - amount > 0){
            healthSlider.value = currentHealth/maxHealth;
        } else {
            healthSlider.value = 0f;
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

    public void StatusCooldown(float timer){
        if(timer>currentStatus.GetDuration()){
            currentStatus = Statuses.instance.GetHealthy();
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

    public void PopupStatuses(bool active){
        if (active){
            popupStatus.SetActive(true);
        }
        else{
            popupStatus.SetActive(false);
        }
    }

    public float getCurrentHealth()
    {
        return currentHealth;
    }  

    public void UpdateDamageDealt(float damageDealt){
        this.damageDealt += damageDealt;
    }   

    public void RefreshHealth(){
        this.currentHealth = maxHealth;
    }
}
