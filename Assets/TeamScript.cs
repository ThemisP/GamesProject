using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamScript : MonoBehaviour
{

    public Transform player1;
    public Transform player2;

    public Transform handle1;
    public Transform handle2;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player1 !=null && player2 != null) {
            Debug.Log(player1.transform);
            handle1.position = player1.position+Vector3.forward*1;
            handle2.position = player2.position+Vector3.forward*-1;
        }
    }

    public void SetPlayers(Transform player1, Transform player2) {
        this.player1 = player1;
        this.player2 = player2;
    }
}
