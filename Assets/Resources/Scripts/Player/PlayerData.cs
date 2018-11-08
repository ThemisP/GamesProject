using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : Photon.MonoBehaviour {

	[SerializeField] private float maxHealth = 100f;
	private Slider healthSlider;
	private float currentHealth;

	private int kills = 0, assists = 0, deaths = 0;

	private int skillPoints = 0;

	private PhotonView photonView;
	void Start()
	{
		GameObject canvas = GameObject.Find("HUD");
		healthSlider = canvas.GetComponentInChildren<Slider>();
		currentHealth = maxHealth;
		photonView = GetComponent<PhotonView>();
		healthSlider.value = currentHealth/maxHealth;
	}

	private void FixedUpdate() {
		if(!photonView.isMine) return;
		
		

		//currentHealth -= 5 *Time.deltaTime; // This is just to check if the slider is working
	}
}
