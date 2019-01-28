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
        GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.EulerAngles(rot));
        bullet.GetComponent<Rigidbody>().velocity = Vector3.forward*speed;
        bullet.GetComponent<BulletScript>().lifeTime = lifetime;
        Bullets.Add(bulletId, bullet);
    }

}
