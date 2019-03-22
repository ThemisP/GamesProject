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
        private Vector3 MaxConnectionTension;
        // Start is called before the first frame update
        void Start()
        {
            MaxConnectionTension = new Vector3(100000f, 0, 100000f);
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
        if (player1 == null || player2 == null) return 0f;
        return (player1.position-player2.position).magnitude;
        }
        
        // TODO: Integrate with new chain model
        public Vector3 movementModifier(float difference, Vector3 movement ){
            // if (difference >= jointRange) {
            //     /* At this point the connection is at max tension, so we must dampen 
            //      * the appropriate components of the movement of the player in the 
            //      * direction in which the force of tension is applied
            //     */
            //     Vector3 movementNormalized = movement.normalized;
            //     Vector3 tensionDirection = (player1.position - player2.position).normalized;
            //     if (movementNormalized - tensionDirection == Vector3.zero){
            //         return movement * 0f;
            //     }
            // }
            return movement;
        }
    }