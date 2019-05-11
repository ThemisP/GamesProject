using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectHandler : MonoBehaviour {
    public static ObjectHandler instance;

 	private Queue<Action> RunOnMainThread = new Queue<Action>();
    private Dictionary<string, GameObject> Bullets;
    public GameObject bulletPrefab;
    public GameObject bulletEnemyPrefab;
    private Dictionary<string, GameObject> Coins;
    private Dictionary<string, GameObject> Pills;
    // Use this for initialization
    void Start () {
        instance = this;
        Bullets = new Dictionary<string, GameObject>();
        Coins = new Dictionary<string, GameObject>();
        Pills = new Dictionary<string, GameObject>();
        Transform coinGroup = GameObject.Find("CoinSpawns").transform;
        int i = 1;
        bool loopRun = true;
        while (loopRun) {
            Transform coinX = coinGroup.Find("Coin (" + i + ")");
            if (coinX != null) {
                Coins.Add("Coin (" + i + ")", coinX.gameObject);
                i++;
            } else
                loopRun = false;
            
        }
        Transform pillGroup = GameObject.Find("PillSpawns").transform;
        i = 1;
        loopRun = true;
        while (loopRun) {
            Transform pillX = pillGroup.Find("Pill (" + i + ")");
            if (pillX != null) {
                Pills.Add("Pill (" + i + ")", pillX.gameObject);
                i++;
            } else
                loopRun = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (RunOnMainThread.Count > 0) {
            	lock (RunOnMainThread) {
			Action s = RunOnMainThread.Dequeue();
			s();
            }            
        }
	}

    public void CallFunctionFromAnotherThread(Action functionName) {
        lock (RunOnMainThread) {
            RunOnMainThread.Enqueue(functionName);
        }
    }

    public void DestroyBullet(string bulletId) {
        GameObject bullet;
        if(Bullets.TryGetValue(bulletId, out bullet)) {
            Bullets.Remove(bulletId);
            if(bullet!= null) {
                Destroy(bullet, 0f);
            }
        }      
    }

    public void InstantiateBullet(Vector3 pos, Vector3 rot, float speed, float lifetime, string bulletId, float damage, int bulletTeam){
        GameObject bullet;
        if (bulletTeam == Network.instance.player.GetTeamNumber() )
            bullet = GameObject.Instantiate(this.bulletPrefab, pos, Quaternion.Euler(rot));
        else
            bullet = GameObject.Instantiate(this.bulletEnemyPrefab, pos, Quaternion.Euler(rot));
        //Rigidbody rigbod = bullet.GetComponent<Rigidbody>();
        //if (rigbod != null) rigbod.velocity = Vector3.forward*speed;
        //else Debug.LogError("Rigid body for bullet prefab spawn not found!");
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null) {
            bulletScript.SetSpeed(speed);
            bulletScript.lifeTime = lifetime;
            bulletScript.SetBulletId(bulletId);
            bulletScript.SetBulletDamage(damage);
            bulletScript.SetBulletTeam(bulletTeam);
        } else Debug.LogError("BulletScript not found for bullet prefab!");

        if(bulletScript!=null)Bullets.Add(bulletId, bullet);
    }

    public void InstantiateCollectibles() {
        foreach (GameObject coin in Coins.Values) {
            coin.SetActive(true);
        }
        foreach (GameObject pill in Pills.Values) {
            pill.SetActive(true);
        }
    }

    public void DisableCoin(string id) {
        GameObject coin;
        if(Coins.TryGetValue(id, out coin)) {
            coin.SetActive(false);            
        }
    }

    public void DisablePill(string id) {
        GameObject pill;
        if (Pills.TryGetValue(id, out pill)) {
            pill.SetActive(false);
        }
    }

    public void DestroyAll() {
        foreach(KeyValuePair<string, GameObject> bullet in Bullets) {
            GameObject.Destroy(bullet.Value);
        }
    }

}
