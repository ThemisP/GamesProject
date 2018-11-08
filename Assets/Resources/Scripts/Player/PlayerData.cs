using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : Photon.MonoBehaviour {

	[SerializeField] private float maxHealth = 100f;
	public Slider healthSlider;
	private float currentHealth;

	private int kills = 0, assists = 0, deaths = 0;

	private int skillPoints = 0;

	private PhotonView photonView;
	void Start()
	{
		currentHealth = maxHealth;
		photonView = GetComponent<PhotonView>();
	}

	private void FixedUpdate() {
		if(!photonView.isMine) return;

		
	}
}
