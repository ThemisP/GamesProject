﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineLink : MonoBehaviour
{
    private Transform StartPoint;
    private Transform EndPoint;

    private CapsuleCollider ColliderCapsule;
    private LineRenderer Line;
    private Rigidbody rig;

    public float LineWidth;

    void Start() {
        Line = gameObject.GetComponent<LineRenderer>();
        ColliderCapsule = gameObject.GetComponent<CapsuleCollider>();
        rig = gameObject.GetComponent<Rigidbody>();
        ColliderCapsule.radius = LineWidth / 2;
        ColliderCapsule.center = Vector3.zero;

    }
    public void SetLineEnds(Transform start, Transform end) {
        this.StartPoint = start;
        this.EndPoint = end;
    }

    // Update is called once per frame
    void Update()
    {
        if (StartPoint != null && EndPoint != null) {
            Line.SetPosition(0, StartPoint.position);
            Line.SetPosition(1, EndPoint.position);

            rig.MovePosition(StartPoint.position + (EndPoint.position - StartPoint.position) / 2);
            
            //ColliderCapsule.transform.position = StartPoint.position + (EndPoint.position - StartPoint.position) / 2;
            ColliderCapsule.transform.LookAt(EndPoint.position);
            ColliderCapsule.height = (EndPoint.position - StartPoint.position).magnitude - 0.2f;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        //Debug.Log(collision.gameObject.name);
    }
}
