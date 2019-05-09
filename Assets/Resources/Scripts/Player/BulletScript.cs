using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    [Header("Vfx particles")]
    public GameObject destroyParticle;

    string bullet_id;
	float bulletDamage = 10;
	public float lifeTime = 2f;
    private float speed = 1f;
    private int bulletTeam;

	new Rigidbody rigidbody;
	void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.velocity = this.transform.forward * speed;
	}

	void Update(){
        rigidbody.velocity = this.transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) {
            ObjectHandler.instance.DestroyBullet(this.bullet_id);
        }
	}

    void OnCollisionEnter(Collision collision) {        
        if (bullet_id.StartsWith(Network.instance.ClientIndex.ToString())) {            
            GameObject obj = collision.gameObject;
            if (obj.tag != "EnemyPlayer" && obj.tag!="Bullet" && obj.tag != "EnemyRevive") {
                ObjectHandler.instance.DestroyBullet(this.bullet_id);
            }
        }
    }

    void OnTriggerEnter(Collider collision) {
        GameObject obj = collision.gameObject;
        if (obj.tag != "EnemyPlayer" && obj.tag != "Bullet" && obj.tag != "EnemyRevive") {
            ObjectHandler.instance.DestroyBullet(this.bullet_id);
        }
    }

    void OnDestroy() {
        GameObject particle = Instantiate(destroyParticle, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(particle, 0.5f);
    }

    #region "Setters"
    public void SetBulletId(string id) {
        this.bullet_id = id;
    }
    public void SetBulletDamage(float damage) {
        this.bulletDamage = damage;
    }
    public void SetBulletLifeTime(float lifetime) {
        this.lifeTime = lifetime;
    }
    public void SetSpeed(float speed) {
        this.speed = speed;
    }
    public void SetBulletTeam(int number) {
        this.bulletTeam = number;
    }
    #endregion

    #region "Getters"
    public string GetBulletId() {
        return this.bullet_id;
    }
    public float GetBulletDamage() {
        return this.bulletDamage;
    }
    public int GetBulletTeam() {
        return this.bulletTeam;
    }
    #endregion
}
