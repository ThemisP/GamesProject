using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    Quaternion rotation = Quaternion.Euler(new Vector3(45, 0, 0));
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = rotation;
    }
}
