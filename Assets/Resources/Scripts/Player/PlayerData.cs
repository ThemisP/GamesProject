using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : Photon.MonoBehaviour {

	[SerializeField] private float maxHealth = 100f;
	private Slider healthSlider;
	private float currentHealth;
    private Text coinCount;
    private Text nodeCount;

	private int kills = 0, assists = 0, deaths = 0;

	private int coins = 0;
    private int nodePoints = 0;
	void Start()
	{
		GameObject canvas = GameObject.Find("Health");
        GameObject balance = GameObject.Find("Balance");
        GameObject nodes = GameObject.Find("NodesBalance");
		healthSlider = canvas.GetComponent<Slider>();
        coinCount = balance.GetComponent<Text>();
        coinCount.text = "0";
        nodeCount = nodes.GetComponent<Text>();
        nodeCount.text = "0";
		currentHealth = maxHealth;
		healthSlider.value = currentHealth/maxHealth;
	}

	// private void FixedUpdate() {
	// 	if(!photonView.isMine) return;
        
    //     // healthSlider.value = currentHealth/maxHealth;
    //     // coinCount.text = coins.ToString();
    //     // nodeCount.text = nodePoints.ToString();
	// }

    [PunRPC]
    public void takeDamage(float amount){
        if(photonView.isMine){
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
