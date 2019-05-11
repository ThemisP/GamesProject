using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleRotator : MonoBehaviour
{

    public GameObject destroyParticle;

    // Update is called once per frame
    void Update(){
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    void OnDisable() {
        GameObject particle = Instantiate(destroyParticle, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(particle, 1f);
    }
}
