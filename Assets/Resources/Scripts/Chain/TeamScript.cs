using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class TeamScript : MonoBehaviour {

    private Rigidbody player1;
    private Rigidbody player2;

    [Header("Chain Prefabs")]
    public GameObject SpringJoint;
    public GameObject ChainLink;

    private int chainPoints = 5;

    [SerializeField] private float jointRange = 5f;
    private float separationRatio;

    public void SetPlayers(Transform player1, Transform player2) {
        Rigidbody player1Rigid = player1.GetComponent<Rigidbody>();
        Rigidbody player2Rigid = player2.GetComponent<Rigidbody>();

        this.player1 = player1Rigid;
        this.player2 = player2Rigid;
        CreateChain();
    }

    private void CreateChain() {
        Vector3 differenceVecStep = getDifference() * 1 / (float)chainPoints;
        CreateChain(player1, differenceVecStep, 0);        
    }
    private void CreateChain(Rigidbody link1, Vector3 differenceVecStep, int count) {
        if (count >= chainPoints) return;
        GameObject link2;
        if (count == chainPoints-1) {
            link2 = player2.gameObject;
        } else {
            link2 = Instantiate(ChainLink, player1.position + (count * differenceVecStep), player1.rotation);
        }
        GameObject joint = Instantiate(SpringJoint);
        link1.transform.parent = gameObject.transform;
        link2.transform.parent = gameObject.transform;
        joint.transform.parent = gameObject.transform;

        Rigidbody link1Rigid = link1.GetComponent<Rigidbody>();
        Rigidbody link2Rigid = link2.GetComponent<Rigidbody>();
        if (link1Rigid == null || link2Rigid == null) Debug.LogError("rigidbodies not found in chain links");
        else {
            SpringJoint script = joint.GetComponent<SpringJoint>();
            if (script == null) Debug.Log("spring joint script not found");
            else {
                script.SetHandles(link1Rigid, link2Rigid);
                CreateChain(link2Rigid, differenceVecStep, count + 1);
            }
        }
    }

    public float getJointRange() {
        return jointRange;
    }

    public Vector3 getDifference() {
        return (player1.position - player2.position);
    }

    // TODO: Smoothing of movement circle edge 
    //public Vector3 movementModifier(Vector3 velocity) {
    //    Vector3 differenceVec = getDifference();
    //    if (differenceVec.magnitude >= jointRange) {
    //        /* At this point the connection is at max tension, so we must dampen 
    //         * the appropriate components of the movement of the player in the 
    //         * direction in which the force of tension is applied
    //        */
    //        float displacement = differenceVec.magnitude - jointRange;
    //        float force = -tensionConstant * displacement - dampeningConstant*velocity.magnitude;
    //        Vector3 forceVec = differenceVec.normalized * force;
    //        return forceVec;
    //    }
    //    return Vector3.zero;
    //}
}