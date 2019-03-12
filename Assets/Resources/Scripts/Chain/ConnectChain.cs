using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Scripts.Chain
{


    public class ConnectChain : MonoBehaviour
    {

        void Awake()
        {
            GetComponent<CharacterJoint>().connectedBody = transform.parent.GetComponent<Rigidbody>();
        }
    }
}
