using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


    public class TeamScript : MonoBehaviour
    {

        public Transform player1;
        public Transform player2;

        public Transform handle1;
        public Transform handle2;

        public Transform block;

        public FixedJoint joint1;
        public FixedJoint joint2;
        [SerializeField] private float jointRange =5f;
        private float separationRatio;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (player1 != null && player2 != null)
            {
                //Debug.Log(player1.transform);
                block.position = 0.5f * (player1.position + player2.position);
            }
        }

        public void SetPlayers(Transform player1, Transform player2)
        {
            this.player1 = player1;
            this.player2 = player2;
        }

        public float getJointRange()
        {
            return jointRange;
        }

        public float getDifference(){
            return (player1.position-player2.position).magnitude;
        }
        
        //potentially vectorise this in the future
        public Vector3 movementModifier(float difference, Vector3 movement ){
        separationRatio = 1 - ((jointRange - difference) / jointRange);
        Debug.Log(separationRatio);
        //if (separationRatio > 0.4f) movement = movement * 0.5f;
        //else if (separationRatio > 0.55f) movement = movement * 0.25f;
        if (separationRatio > 0.6f) movement = movement * 0.1f;
        if (separationRatio > 0.75f) movement = movement * 0.5f;
        else if (separationRatio > 0.9f) movement = movement * 0.1f;
        else if (separationRatio > 1.01f) movement = movement * 10f;
        else if (separationRatio > 1.1f) movement = movement * 0f;
        return movement;
        }
    }