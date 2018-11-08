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

	private PhotonView photonView;
	void Start()
	{
		GameObject canvas = GameObject.Find("HUD");
        GameObject balance = GameObject.Find("Balance");
        GameObject nodes = GameObject.Find("Nodes");
		healthSlider = canvas.GetComponentInChildren<Slider>();
        coinCount = balance.GetComponent<Text>();
        nodeCount = nodes.GetComponent<Text>();
		currentHealth = maxHealth;
		photonView = GetComponent<PhotonView>();
		healthSlider.value = currentHealth/maxHealth;

	}

	private void FixedUpdate() {
     
		if(!photonView.isMine) return;
        coins += 1;
        nodePoints += 1;
        coinCount.text = coins.ToString();

		//currentHealth -= 5 *Time.deltaTime; // This is just to check if the slider is working
	}
    //finds the amount of skill points held by the user
    public int getSkillPoints()
    {
        return coins;
    }
    //updates the skill points held by the user
    public void addCoins(int newCoins)
    {
        coins += newCoins;
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
