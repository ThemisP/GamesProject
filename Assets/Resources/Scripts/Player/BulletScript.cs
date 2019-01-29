using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : Photon.PunBehaviour {
    string bullet_id;
	float bulletDamage = 10;
	public float lifeTime = 2f;

	Rigidbody rigidbody;
	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.velocity = this.transform.forward * 10f;
	}

	void Update(){
        if (bullet_id.StartsWith(Network.instance.ClientIndex.ToString())) {
            lifeTime -= Time.deltaTime;
            if (lifeTime < 0) {
                ObjectHandler.instance.DestroyBullet(this.bullet_id);
                Network.instance.SendDestroyBullet(bullet_id);
            }
        }
	}

    private void OnCollisionEnter(Collision collision) {
        GameObject obj = collision.gameObject;
        Debug.Log("triggered");
        if (obj.tag != "EnemyPlayer") {
            ObjectHandler.instance.DestroyBullet(this.bullet_id);
            Network.instance.SendDestroyBullet(this.bullet_id);
        }
    }

    #region "Setters"
    public void SetBulletId(string id) {
        this.bullet_id = id;
    }
    #endregion

    #region "Getters"
    public string GetBulletId() {
        return this.bullet_id;
    }
    public float GetBulletDamage() {
        return this.bulletDamage;
    }
    #endregion
}
