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
    private Text shotgunCost;
    private Text rifleCost;
    private Text sniperCost;
    private Text rangeCost;
    private Text powerCost;
    private Text capacityCost;
    private Text assist_text;
    private Slider dodgeSlider;
    private int kills = 0, assists = 0, deaths = 0;

    private GameObject HUDCanvas;
    private GameObject popupHelp;
    private GameObject popupWeapon;
    private GameObject holdToRevive;
    private GameObject ReviveSlider;
    private GameObject DamageBloodHUD;
    private GameObject activeWeapon;
    private GameObject currentMagazine;
    private GameObject balance;
    private GameObject reload;


    private Transform TextAssist;
    private Transform shotgunRef;
    private Transform rifleRef;
    private Transform sniperRef;
    private Transform rangeRef;
    private Transform powerRef;
    private Transform capacityRef;

    private Button pistolButton;
    private Button shotgunButton;
    private Button rifleButton;
    private Button sniperButton;
    private Button upgradeRangeButton;
    private Button upgradeDamageButton;
    private Button upgradeCapacityButton;

    private GameObject upgradeCover;


    // private GameObject popupStatus;
    private PlayerData startingData;
    public PlayerController playerController;

	private int coins = 0;
    private int nodePoints = 0;
    private float damageDealt = 0;
    private int upgradeCount = 0;

    public Weapon currentWeapon = Weapons.instance.GetPistol();
    public Status currentStatus = Statuses.instance.GetHealthy();
    private float circleTimer = 0f;

    public bool upgradeFlag = false;

    //Change back to Start when fixed HUD
    void Start() {
        startingData = this;
        HUDCanvas = GameObject.Find("HUDPrefab");
        RectTransform hud = HUDCanvas.GetComponent<RectTransform>();
        GameObject canvas = GameObject.Find("Health");
        balance = GameObject.Find("Balance");
        GameObject nodes = GameObject.Find("NodesBalance");
        DamageBloodHUD = hud.Find("BloodDamage").gameObject;
        DamageBloodHUD.SetActive(false);
        currentMagazine = hud.Find("Magazine").gameObject;
        popupHelp = hud.Find("Help_Popup").gameObject;
        popupWeapon = hud.Find("Weapons_Popup").gameObject;
        activeWeapon = hud.Find("WeaponName").gameObject;
        TextAssist = hud.GetComponentInChildren<RectTransform>().Find("Text_Assist");
        // popupStatus = hud.Find("Statuses_Popup").gameObject;
        dodgeSlider = hud.Find("Dodge Cooldown").GetComponent<Slider>();
        holdToRevive = hud.Find("Revive_Button").gameObject;
        ReviveSlider = hud.Find("Revive_Slider").gameObject;
        reload = hud.Find("Reload").gameObject;
        reload.SetActive(false);
        pistolButton = popupWeapon.GetComponent<RectTransform>().Find("Pistol").GetComponent<Button>();
        shotgunButton = popupWeapon.GetComponent<RectTransform>().Find("Shotgun").GetComponent<Button>();
        rifleButton = popupWeapon.GetComponent<RectTransform>().Find("Rifle").GetComponent<Button>();
        sniperButton = popupWeapon.GetComponent<RectTransform>().Find("Sniper").GetComponent<Button>();
        upgradeRangeButton = popupWeapon.GetComponent<RectTransform>().Find("+Range").GetComponent<Button>();
        upgradeDamageButton = popupWeapon.GetComponent<RectTransform>().Find("+Power").GetComponent<Button>();
        upgradeCapacityButton = popupWeapon.GetComponent<RectTransform>().Find("+Capacity").GetComponent<Button>();
        upgradeCover = popupWeapon.GetComponent<RectTransform>().Find("Upgrade Cover").gameObject;

        pistolButton.onClick.AddListener(() => UpgradeWeapon("Pistol"));
        shotgunButton.onClick.AddListener(() => UpgradeWeapon("Shotgun"));
        rifleButton.onClick.AddListener(() => UpgradeWeapon("Assault Rifle"));
        sniperButton.onClick.AddListener(() => UpgradeWeapon("Sniper"));
        upgradeCapacityButton.onClick.AddListener(() => UpgradeWeapon("+Capacity"));
        upgradeDamageButton.onClick.AddListener(() => UpgradeWeapon("+Power"));
        upgradeRangeButton.onClick.AddListener(() => UpgradeWeapon("+Range"));

        shotgunRef = popupWeapon.GetComponentInChildren<RectTransform>().Find("Shotgun Cost");
        rifleRef = popupWeapon.GetComponentInChildren<RectTransform>().Find("Rifle Cost");
        sniperRef = popupWeapon.GetComponentInChildren<RectTransform>().Find("Sniper Cost");
        rangeRef = popupWeapon.GetComponentInChildren<RectTransform>().Find("Range Cost");
        powerRef = popupWeapon.GetComponentInChildren<RectTransform>().Find("Power Cost");
        capacityRef = popupWeapon.GetComponentInChildren<RectTransform>().Find("Capacity Cost");

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
        TextAssist.gameObject.SetActive(false);
        holdToRevive.SetActive(false);
        ReviveSlider.SetActive(false);
        TextAssist.gameObject.SetActive(false);
        // popupStatus.SetActive(false);
        healthSlider = canvas.GetComponent<Slider>();
        coinCount = balance.GetComponent<Text>();
        coinCount.text = "0";
        weaponName = activeWeapon.GetComponent<Text>();
        weaponName.text = "Pistol";

        ammoRemaining = currentMagazine.GetComponentInChildren<Text>();
        ammoRemaining.text = "10";
        currentHealth = maxHealth;
        healthSlider.value = currentHealth / maxHealth;

        assist_text = TextAssist.GetComponent<Text>();

        shotgunCost = shotgunRef.GetComponentInChildren<Text>();
        shotgunCost.text = "10";

        rifleCost = rifleRef.GetComponentInChildren<Text>();
        rifleCost.text = "10";
        sniperCost = sniperRef.GetComponentInChildren<Text>();
        sniperCost.text = "10";
        rangeCost = rangeRef.GetComponentInChildren<Text>();
        rangeCost.text = " ";
        powerCost = powerRef.GetComponentInChildren<Text>();
        powerCost.text = " ";
        capacityCost = capacityRef.GetComponentInChildren<Text>();
        capacityCost.text = " ";



        //assist_text.text = "You should not be able to see this...";
        upgradeCount = 0;   
    }
    private void Update() {
        circleTimer += Time.deltaTime;
        if (circleTimer > 2f) {
            circleTimer = 0f;
            if (!ShrinkCircle.instance.isInCircle(this.transform)) {
                takeDamage(5.0f, "");
                if(!playerController.isOffline()) Network.instance.SendPlayerDamage(5.0f, "");

            }
        }
        ammoRemaining.text = playerController.GetBulletsLeft().ToString() + " / " + currentWeapon.GetMagazine();

    }

    void UpgradeWeapon(string newWeapon) {

        if (currentWeapon.GetWeaponName().Equals("Pistol")) {
            /*Stage 1:  Upgrade from pistol to either Shotgun, Assault_Rifle or Sniper */
            TextAssist.gameObject.SetActive(true);

            switch (newWeapon) {

                case "Shotgun":
                    if (addCoinsIfAvailable(-10)) {
                        upgradeFlag = true;
                        this.currentWeapon = Weapons.instance.GetShotgun();
                        weaponName.text = "Shotgun";
                        playerController.Reload();
                        ammoRemaining.text = playerController.GetBulletsLeft().ToString() + " / " + currentWeapon.GetMagazine();
                        DisableGunButtons();
                        upgradeCount++;
                        assist_text.text = "Upgrade Sucessful";
                        sniperCost.text = " ";
                        rifleCost.text = " ";
                        shotgunCost.text = " ";
                        rangeCost.text = (10 * upgradeCount).ToString();
                        powerCost.text = (10 * upgradeCount).ToString();
                        capacityCost.text = (10 * upgradeCount).ToString();
                        upgradeCover.SetActive(false);
                    }
                    else
                    {
                        assist_text.text = "You don't have enough to upgrade your Pistol into a Shotgun";
                        Debug.Log("You don't have enough to upgrade your Pistol into a Shotgun");
                        
                    }

                    break;
                case "Assault Rifle":
                    if (addCoinsIfAvailable(-10)) {
                        upgradeFlag = true;
                        this.currentWeapon = Weapons.instance.GetAssaultRifle();
                        weaponName.text = "Assault Rifle";
                        playerController.Reload();
                        ammoRemaining.text = playerController.GetBulletsLeft().ToString() + " / " + currentWeapon.GetMagazine();
                        DisableGunButtons();
                        upgradeCount++;
                        assist_text.text = "Upgrade Sucessful";
                        sniperCost.text = " ";
                        rifleCost.text = " ";
                        shotgunCost.text = " ";
                        rangeCost.text = (10 * upgradeCount).ToString();
                        powerCost.text = (10 * upgradeCount).ToString();
                        capacityCost.text = (10 * upgradeCount).ToString();
                        upgradeCover.SetActive(false);

                    }
                    else
                    {
                        assist_text.text = "You don't have enough to upgrade your Pistol into an Assault Rifle";
                        Debug.Log("You don't have enough to upgrade your Pistol into an Assault Rifle");
                    }
                    break;
                case "Sniper":
                    if (addCoinsIfAvailable(-10)) {
                        upgradeFlag = true;
                        this.currentWeapon = Weapons.instance.GetSniper();
                        weaponName.text = "Sniper";
                        playerController.Reload();
                        ammoRemaining.text = playerController.GetBulletsLeft().ToString() + " / " + currentWeapon.GetMagazine();
                        DisableGunButtons();
                        upgradeCount++;
                        assist_text.text = "Upgrade Sucessful";
                        sniperCost.text = " ";
                        rifleCost.text = " ";
                        shotgunCost.text = " ";
                        rangeCost.text = (10 * upgradeCount).ToString();
                        powerCost.text = (10 * upgradeCount).ToString();
                        capacityCost.text = (10 * upgradeCount).ToString();
                        upgradeCover.SetActive(false);

                    }
                    else
                    {
                        assist_text.text = "You don't have enough to upgrade your Pistol into a Sniper";
                        Debug.Log("You don't have enough to upgrade your Pistol into a Sniper");
                    }
                    break;
                default:
                    Debug.Log("Invalid Action. ");
                    break;
            }
        } else {
            switch (newWeapon) {
                case "+Range":
                    if (addCoinsIfAvailable(-10 * upgradeCount))
                    {
                        upgradeFlag = true;
                        this.currentWeapon.SetRange(currentWeapon);
                        upgradeCount++;
                        assist_text.text = "Upgrade Sucessful";
                        rangeCost.text = (10 * upgradeCount).ToString();
                        powerCost.text = (10 * upgradeCount).ToString();
                        capacityCost.text = (10 * upgradeCount).ToString();
                    }
                    else
                    {
                        assist_text.text = "You don't have enough to upgrade your weapon's range!";
                        Debug.Log("You don't have enough to upgrade your weapon's range!");
                    }
                    break;

                case "+Power":
                    if (addCoinsIfAvailable(-10 * upgradeCount))
                    {
                        upgradeFlag = true;
                        this.currentWeapon.SetDamage(currentWeapon);
                        upgradeCount++;
                        assist_text.text = "Upgrade Sucessful";
                        rangeCost.text = (10 * upgradeCount).ToString();
                        powerCost.text = (10 * upgradeCount).ToString();
                        capacityCost.text = (10 * upgradeCount).ToString();

                    }
                    else
                    {
                        assist_text.text = "You don't have enough to upgrade your weapon's power!";
                        Debug.Log("You don't have enough to upgrade your weapon's power!");
                    }
                    break;

                case "+Capacity":
                    if (addCoinsIfAvailable(-10 * upgradeCount))
                    {
                        upgradeFlag = true;
                        this.currentWeapon.SetCapacity(currentWeapon);
                        upgradeCount++;
                        assist_text.text = "Upgrade Sucessful";
                        rangeCost.text = (10 * upgradeCount).ToString();
                        powerCost.text = (10 * upgradeCount).ToString();
                        capacityCost.text = (10 * upgradeCount).ToString();
                        playerController.Reload();
                    }
                    else
                    {
                        assist_text.text = "You don't have enough to upgrade your weapon's ammo capacity!";
                        Debug.Log("You don't have enough to upgrade your weapon's ammo capacity!");
                    }
                    break;

                default:
                    Debug.Log("Invalid upgrade");
                    break;
            }
        }
        StartCoroutine(Delay(2.5f));
        ammoRemaining.text = playerController.GetBulletsLeft().ToString() + " / " + currentWeapon.GetMagazine();
    }

    void DisableGunButtons() {
        pistolButton.interactable = false;
        shotgunButton.interactable = false;
        rifleButton.interactable = false;
        sniperButton.interactable = false;
    }

    public void takeDamage(float amount, string bulletId){
        if (currentHealth - amount > 0) {
            currentHealth -= amount;
            healthSlider.value = currentHealth/maxHealth;
        } else {
            currentHealth = 0;
            healthSlider.value = 0f;
        }
        CameraFollow script = Camera.main.GetComponent<CameraFollow>();
        if (script != null) script.ShakeCamera();
        else Debug.LogWarning("CameraFollow script not found");
        StartCoroutine(DamageBloodHUDReveal());
    }
    IEnumerator DamageBloodHUDReveal() {
        DamageBloodHUD.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);
        DamageBloodHUD.SetActive(false);
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
        if (this.coins + newCoins >= 0) {
            coins += newCoins;
            balance.GetComponent<Text>().text = coins.ToString();
            //balance.text = coins.ToString();
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

    public bool RefreshHealth(float amount) {
        if (currentHealth >= maxHealth) return false;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        healthSlider.value = currentHealth / maxHealth;
        if(!playerController.isOffline()) Network.instance.SendPlayerHealth(currentHealth);
        return true;
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

    public void SethealthFromRevive() {
        this.currentHealth = 30f;
        healthSlider.value = 30f;
    }

    public void ReloadPopupShow() {
        StartCoroutine(ReloadPopup(2f));
    }

    public void ReloadPopupStop() {
        reload.SetActive(false);
    }

    IEnumerator ReloadPopup(float time) {
        reload.SetActive(true);
        yield return new WaitForSecondsRealtime(time);
        reload.SetActive(false);
    }



    /* TODO: Fix and Generalise this */
    IEnumerator Delay(float x)
    {
        TextAssist.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(x);
        TextAssist.gameObject.SetActive(false);
    }


    public void EndGame()
    {
        pistolButton.interactable = true;
        shotgunButton.interactable = true;
        rifleButton.interactable = true;
        sniperButton.interactable = true;
        upgradeCover.SetActive(true);
    }
}
