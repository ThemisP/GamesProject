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
    }