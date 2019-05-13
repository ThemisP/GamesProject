using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPosition : MonoBehaviour
{
    private Rigidbody rig;

    void Start() {
        rig = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < 0) {
            rig.MovePosition(new Vector3(transform.position.x, 1, transform.position.z));
        }
    }
}
