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
    private Text weaponName;
    private Text ammoRemaining;
    private Text userName;
    private Slider dodgeSlider;
    private int kills = 0, assists = 0, deaths = 0;

    private GameObject HUDCanvas;
    private GameObject popupHelp;
    private GameObject popupWeapon;
    private GameObject holdToRevive;
    private GameObject ReviveSlider;
    private GameObject activeWeapon;
    private GameObject currentMagazine;
    // private GameObject popupStatus;
    private PlayerController playerController;

	private int coins = 0;
    private int nodePoints = 0;
    private float damageDealt = 0;
    public int bulletsLeft;

    public Weapon currentWeapon = Weapons.instance.GetPistol();
    public Status currentStatus = Statuses.instance.GetHealthy();
    public bool upgradeFlag = false;

    //Change back to Start when fixed HUD
    void Start() {
        playerController = GetComponent<PlayerController>();
        HUDCanvas = GameObject.Find("HUDPrefab");
        RectTransform hud = HUDCanvas.GetComponent<RectTransform>();
        GameObject canvas = GameObject.Find("Health");
        GameObject balance = GameObject.Find("Balance");
        GameObject nodes = GameObject.Find("NodesBalance");
        activeWeapon = GameObject.Find("Weapon Type");
        currentMagazine = hud.Find("Magazine").gameObject;
        popupHelp = hud.Find("Help_Popup").gameObject;
        popupWeapon = hud.Find("Weapons_Popup").gameObject;
        // popupStatus = hud.Find("Statuses_Popup").gameObject;
        dodgeSlider = hud.Find("Dodge Cooldown").GetComponent<Slider>();
        holdToRevive = hud.Find("Revive_Button").gameObject;
        ReviveSlider = hud.Find("Revive_Slider").gameObject; 

        Button pistolButton = popupWeapon.GetComponent<RectTransform>().Find("Pistol").GetComponent<Button>();
        Button shotgunButton = popupWeapon.GetComponent<RectTransform>().Find("Shotgun").GetComponent<Button>();
        Button rifleButton = popupWeapon.GetComponent<RectTransform>().Find("Rifle").GetComponent<Button>();
        Button sniperButton = popupWeapon.GetComponent<RectTransform>().Find("Sniper").GetComponent<Button>();
        Button upgradeRangeButton = popupWeapon.GetComponent<RectTransform>().Find("+Range").GetComponent<Button>();
        Button upgradeDamageButton = popupWeapon.GetComponent<RectTransform>().Find("+Power").GetComponent<Button>();
        Button upgradeCapacityButton = popupWeapon.GetComponent<RectTransform>().Find("+Capacity").GetComponent<Button>();


        pistolButton.onClick.AddListener(() => UpgradeWeapon("Pistol"));
        shotgunButton.onClick.AddListener(() => UpgradeWeapon("Shotgun"));
        rifleButton.onClick.AddListener(() => UpgradeWeapon("Assault Rifle"));
        sniperButton.onClick.AddListener(() => UpgradeWeapon("Sniper"));
        upgradeCapacityButton.onClick.AddListener(() => UpgradeWeapon("+Capacity"));
        upgradeDamageButton.onClick.AddListener(() => UpgradeWeapon("+Power"));
        upgradeRangeButton.onClick.AddListener(() => UpgradeWeapon("+Range"));

        // Button healthyButton = popupStatus.GetComponent<RectTransform>().Find("Healthy").GetComponent<Button>();
        // Button burntButton = popupStatus.GetComponent<RectTransform>().Find("Burnt").GetComponent<Button>();
        // Button poisonedButton = popupStatus.GetComponent<RectTransform>().Find("Poisoned").GetComponent<Button>();
        // Button invincibleButton = popupStatus.GetComponent<RectTransform>().Find("Invincible").GetComponent<Button>();
        // Button paralyzedButton = popupStatus.GetComponent<RectTransform>().Find("Paralyzed").GetComponent<Button>();


        // healthyButton.onClick.AddListener(() => ChangeStatus(0));
        // burntButton.onClick.AddListener(() => ChangeStatus(1));
        // poisonedButton.onClick.AddListener(() => ChangeStatus(2));
        // invincibleButton.onClick.AddListener(() => ChangeStatus(3));
        // paralyzedButton.onClick.AddListener(() => ChangeStatus(4));


        HUDCanvas.SetActive(true);
        popupHelp.SetActive(false);
        popupWeapon.SetActive(false);
        holdToRevive.SetActive(false);
        ReviveSlider.SetActive(false);
        // popupStatus.SetActive(false);
        healthSlider = canvas.GetComponent<Slider>();
        coinCount = balance.GetComponent<Text>();
        coinCount.text = "0";
        nodeCount = nodes.GetComponent<Text>();
        nodeCount.text = "0";
        weaponName = activeWeapon.GetComponentInChildren<Text>();
        weaponName.text = "Pistol";

        ammoRemaining = currentMagazine.GetComponentInChildren<Text>();
        ammoRemaining.text = "10";
        currentHealth = maxHealth;
        healthSlider.value = currentHealth / maxHealth;
    }

    void UpgradeWeapon(string newWeapon) {
        Debug.Log(newWeapon);
        

        /* Will eventually be where currency check will be in the future */ 
        if (currentWeapon.GetWeaponName().Equals("Pistol"))
        {
            /*Stage 1:  Upgrade from pistol to either Shotgun, Assault_Rifle or Sniper */

            switch (newWeapon)
            {
                case "Shotgun":
                    upgradeFlag = true;
                    this.currentWeapon = Weapons.instance.GetShotgun();
                    weaponName.text = "Shotgun";
                    ammoRemaining.text = bulletsLeft.ToString();

                    break;
                case "Assault Rifle":
                    upgradeFlag = true;
                    this.currentWeapon = Weapons.instance.GetAssaultRifle();
                    weaponName.text = "Assault Rifle";
                    ammoRemaining.text = bulletsLeft.ToString();
                    

                    break;
                case "Sniper":
                    upgradeFlag = true;
                    this.currentWeapon = Weapons.instance.GetSniper();
                    weaponName.text = "Sniper";
                    ammoRemaining.text = bulletsLeft.ToString();
                   

                    break;
                default:
                    Debug.Log("Invalid Action. ");
                    break;
            }
        }
        else
        {
            switch (newWeapon)
            {
                case "+Range":
                    upgradeFlag = true;
                    this.currentWeapon.SetRange(currentWeapon);
                    break;

                case "+Power":
                    upgradeFlag = true;
                    this.currentWeapon.SetDamage(currentWeapon);
                    break;

                case "+Capacity":
                    upgradeFlag = true;
                    this.currentWeapon.SetCapacity(currentWeapon);
                    break;

                default:
                    Debug.Log("Invalid upgrade");
                    break;
            }
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
            GameObject balance = GameObject.Find("Balance");
            coinCount = balance.GetComponent<Text>();
            coinCount.text = coins.ToString();
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

    public void ReviveButton(bool active) {
        if(active) {
            holdToRevive.SetActive(true);
        } else {
            holdToRevive.SetActive(false);
        }
    }

    public void Reviving(float value) {
        ReviveSlider.SetActive(true);
        ReviveSlider.GetComponent<Slider>().value = value;
    }
    public void StopReviving() {
        ReviveSlider.GetComponent<Slider>().value = 0f;
        ReviveSlider.SetActive(false);
    }

    // public void PopupStatuses(bool active){
    //     if (active){
    //         popupStatus.SetActive(true);
    //     }
    //     else{
    //         popupStatus.SetActive(false);
    //     }
    // }

    public float getCurrentHealth()
    {
        return currentHealth;
    } 
    
    public void UpdateDamageDealt(float damageDealt){
        this.damageDealt += damageDealt;
    }   

    public void RefreshHealth(){
        this.currentHealth = maxHealth;
        healthSlider.value = currentHealth / maxHealth;
    }

    public void UpdateMagazine() {
        int currentClip = bulletsLeft;
        ammoRemaining.text = currentClip.ToString();
    }

    public void SethealthFromRevive() {
        this.currentHealth = 30f;
    }

    //public Weapon changeWeapon(Weapon oldWeapon)
    //    {
    //        string item = oldWeapon.GetWeaponName();
    //        /*sets potential value to new weapon */

    //        Weapon acquiredWeapon = new Weapon(10f, 0.7f, 1f, 2f, 8f, 10, 1, "Pistol");

    //        switch (item)
    //        {
    //            case "Assault Rifle":
    //                acquiredWeapon = Weapons.instance.GetAssaultRifle();
    //                break;
    //            case "Shotgun":
    //                acquiredWeapon = Weapons.instance.GetShotgun();
    //                break;
    //            case "Sniper":
    //                acquiredWeapon = Weapons.instance.GetSniper();
    //                break;
    //            default:
    //                Debug.Log("Invalid Action. ");
    //                break;
    //        }
    //        return acquiredWeapon;
    //    }
}
