using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectHandler : MonoBehaviour {
 	private Queue<Action> RunOnMainThread = new Queue<Action>();
    private Dictionary<string, GameObject> Bullets;
    public GameObject bulletPrefab;
	// Use this for initialization
	void Start () {
		
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

    public void InstantiateBullet(Vector3 pos, Vector3 rot, float speed, float lifetime, string bulletId){
        GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.Euler(rot));
        Rigidbody rigbod = bullet.GetComponent<Rigidbody>();
        if (rigbod != null) rigbod.velocity = Vector3.forward * speed;
        else Debug.LogError("Rigid body for bullet prefab spawn not found!");
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null) bulletScript..lifeTime = lifetime;
        else Debug.LogError("BulletScritp not found for bullet prefab!");

        if(rigbod!=null && bulletScript!=null)Bullets.Add(bulletId, bullet);
    }

}
