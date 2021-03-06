﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class TeamScript : MonoBehaviour {

    private Rigidbody player1;
    private Rigidbody player2;

    [Header("Chain Prefabs")]
    public GameObject SpringJoint;
    public GameObject ChainLink;
    public GameObject LineLink;

    private int chainPoints = 5;
    private List<GameObject> ChainRelatedObjects = new List<GameObject>();

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
        player1.transform.parent = gameObject.transform;
        
        CreateChain(player1, differenceVecStep, 0);
    }
    private void CreateChain(Rigidbody link1, Vector3 differenceVecStep, int count) {
        if (count >= chainPoints) return;
        GameObject link2;
        if (count == chainPoints-1) {
            link2 = player2.gameObject;
        } else {
            link2 = Instantiate(ChainLink, player1.position + (count * differenceVecStep), player1.rotation);
            ChainRelatedObjects.Add(link2);
        }
        GameObject joint = Instantiate(SpringJoint);
        ChainRelatedObjects.Add(joint);
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
                GameObject line = Instantiate(LineLink, player1.position + (count * differenceVecStep), player1.rotation);
                ChainRelatedObjects.Add(line);
                LineLink lineScript = line.GetComponent<LineLink>();
                line.transform.parent = gameObject.transform;
                if (lineScript == null) Debug.LogError("Line script not found");
                else lineScript.SetLineEnds(link1.transform, link2.transform);
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

    public void DestroyChain() {
        foreach(GameObject item in ChainRelatedObjects) {
            Destroy(item);
        }
        ChainRelatedObjects.Clear();
        Destroy(this, 0.5f);
    }
}
