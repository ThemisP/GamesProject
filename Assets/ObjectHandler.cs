using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectHandler : MonoBehaviour {
    public static ObjectHandler instance;

 	private Queue<Action> RunOnMainThread = new Queue<Action>();
    private Dictionary<string, GameObject> Bullets;
    public GameObject bulletPrefab;
	// Use this for initialization
	void Start () {
        instance = this;
        Bullets = new Dictionary<string, GameObject>();
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

    public void InstantiateBullet(Vector3 pos, Vector3 rot, float speed, float lifetime, string bulletId, float damage){
        GameObject bullet = GameObject.Instantiate(this.bulletPrefab, pos, Quaternion.Euler(rot));
        //Rigidbody rigbod = bullet.GetComponent<Rigidbody>();
        //if (rigbod != null) rigbod.velocity = Vector3.forward*speed;
        //else Debug.LogError("Rigid body for bullet prefab spawn not found!");
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null) {
            bulletScript.SetSpeed(speed);
            bulletScript.lifeTime = lifetime;
            bulletScript.SetBulletId(bulletId);
            bulletScript.SetBulletDamage(damage);
        } else Debug.LogError("BulletScript not found for bullet prefab!");

        if(bulletScript!=null)Bullets.Add(bulletId, bullet);
    }

}
