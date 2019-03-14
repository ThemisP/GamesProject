using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringJoint : MonoBehaviour
{
    [Header("Spring Settings")]
    public float range = 0.5f;
    public float tensionConstant = 3f;
    public float dampeningConstant = 2f;

    private Rigidbody handle1;
    private Rigidbody handle2;

    public void SetHandles(Rigidbody handle1, Rigidbody handle2) {
        this.handle1 = handle1;
        this.handle2 = handle2;
    }

    // Update is called once per frame
    void Update()
    {
        if (handle1 != null && handle2 != null) {
            Vector3 differenceVec = handle1.position - handle2.position;
            if (differenceVec.magnitude >= range) {
                float displacement = differenceVec.magnitude - range;
                float force = -tensionConstant * displacement - dampeningConstant * handle1.velocity.magnitude;
                Vector3 forceVec = differenceVec.normalized * force;
                handle1.AddForce(forceVec);
                handle2.AddForce(-forceVec);
            }
        }
    }
}
