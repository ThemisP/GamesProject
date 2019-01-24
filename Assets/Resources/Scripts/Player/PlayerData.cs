using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerData : Photon.MonoBehaviour {

	[SerializeField] private float maxHealth = 100f;
    [SerializeField] private float dodgeWaitTime = 0.5f; // time required to wait to dodge again after dodging

    
	private Slider healthSlider;
    private Slider dodgeSlider;
   // playerColour = GetComponent<Color>();
    private Text coinCount;
    private Text nodeCount;
    //timings related to undamagable sprite
    //is active when the sprite is either damaged or dodging
    private float currentHealth;
    private float dodgeCooldown;
    //player collectables
    private int kills = 0, assists = 0, deaths = 0;
	private int coins = 0;
    private int nodePoints = 0;
    //fields relating to dodge mechanic
    private bool dodging = false;
    private Color dodgeColor = new Color(1.0f,0.0f,1.0f,1.0f); //purple
    private Image dodgeImage;

	void Start()
	{
            GameObject canvas = GameObject.Find("Health");
            GameObject balance = GameObject.Find("Balance");
            GameObject nodes = GameObject.Find("NodesBalance");
            GameObject dodgeCool = GameObject.Find("Dodge Cooldown");
            healthSlider = canvas.GetComponent<Slider>();
            dodgeSlider = dodgeCool.GetComponent<Slider>();
            coinCount = balance.GetComponent<Text>();
            coinCount.text = "0";
            nodeCount = nodes.GetComponent<Text>();
            nodeCount.text = "0";
            currentHealth = maxHealth;
            healthSlider.value = currentHealth/maxHealth;
        if (true){
        } 
        else {
            healthSlider =  transform.GetChild(0).gameObject.GetComponentInChildren<Slider>();

            healthSlider.gameObject.SetActive(true);
            currentHealth = maxHealth;
            healthSlider.value = currentHealth/maxHealth;
       }
	}

    //void FixedUpdate()
    //{
    //    bool dodge = Input.GetButton("Dodge"); //pressed f key
    //    Dodge(dodge);
    
    //}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
	
	}

    [PunRPC]
    public void takeDamage(float amount){
        if (photonView.isMine)
        {
            dodging = isDodging();
            if (dodging)
            {
                if (currentHealth - amount > 0)
                {
                    currentHealth -= amount;
                    healthSlider.value = currentHealth / maxHealth;
                }
                else
                {
                    currentHealth = 0;
                    healthSlider.value = currentHealth / maxHealth;
                }
            }
            else
            {
                healthSlider.value = currentHealth / maxHealth;
            }
        }
       
    }

    [PunRPC]
    public void Dodge(bool dodging) {
        if (isDodging())
        {
                dodgeCooldown = 0f;
                dodgeSlider.value = dodgeCooldown / dodgeWaitTime;

        }
        else
        {
            if (dodgeCooldown <= dodgeWaitTime)
            {
                dodgeCooldown += Time.deltaTime;
                dodgeSlider.value = dodgeCooldown / dodgeWaitTime;

                //regulates dodgeCooldown value due to precision mismatching of Time.deltaTime 
                // i.e: dodgeCooldown will equal 0.5000001 instead of 0.5; crashing the functionality of the dodge function call
                if(dodgeCooldown > dodgeWaitTime)
                {
                    dodgeCooldown = dodgeWaitTime;
                }
            }
        }

        }
    

    //checks whether the player is currently dodging
    //done by checking if the cooldown time has elapsed
    public bool isDodging()
    {
       // Debug.Log(dodgeCooldown);
       // Debug.Log(dodgeWaitTime);
        return (Input.GetButton("Dodge") && (dodgeCooldown == dodgeWaitTime));
    }


    //finds the amount of skill points held by the user
    public int getSkillPoints()
    {
        return coins;
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

    
}
