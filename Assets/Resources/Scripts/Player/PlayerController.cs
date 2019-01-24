using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Photon.MonoBehaviour {

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform bulletSpawn;
	[SerializeField] private float speed = 6f;
	Vector3 movement;
	Rigidbody playerRigidbody;
   /* Color playerColour = new Color();*/ //a player's colour usually
    Color dodgeColour; //a player's colour when dodging
    Color revertCol; //placeholder to revert colour back to normal once time has elapsed
    int floorMask;
	float camRayLength = 100f;
	float lastShootTime = 0;
	[SerializeField] private float fireRate = 2f;
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private Quaternion realRotation;

	private PlayerData playerData;

	// Use this for initialization
	void Awake () {
		floorMask = LayerMask.GetMask("Floor");
		playerRigidbody = GetComponent<Rigidbody>();
		playerRigidbody.freezeRotation = true;
		playerData = GetComponent<PlayerData>();
	}
	
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		playerRigidbody.MovePosition(Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay));
		playerRigidbody.rotation = Quaternion.Lerp(playerRigidbody.rotation, realRotation, .3f);
	}

	// Update is called once per frame
	void FixedUpdate () {
		if(photonView.isMine){
			float h = Input.GetAxisRaw("Horizontal");// a and d keys
			float v = Input.GetAxisRaw("Vertical"); // w and s keys
			bool fire = Input.GetMouseButton(0);//pressed primary mouse button
            bool dodge = Input.GetButton("Dodge"); //pressed f key
			Move(h,v);
			Turning();
            playerData.Dodge(dodge);
            Fire(fire);
        } else {
			SyncedMovement();
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(playerRigidbody.position);
			stream.SendNext(playerRigidbody.velocity);
			stream.SendNext(playerRigidbody.rotation);
		}
    	else {
        	Vector3 syncPosition = (Vector3)stream.ReceiveNext();
        	Vector3 syncVelocity = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
 
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
	
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = playerRigidbody.position;
    	}
	}

	void Move(float h, float v){
		movement.Set(h, 0f, v);
		//Normalise the movement vector to make it proportional to the speed per second
		//Deltatime is the step for the game timer
		movement = movement.normalized * speed * Time.deltaTime;
		playerRigidbody.MovePosition(transform.position + movement);
	}

	void Turning(){
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		RaycastHit floorHit;

		if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask)){
			Vector3 playerToMouse = floorHit.point - transform.position;

			playerToMouse.y = 0f;

			Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

			playerRigidbody.MoveRotation(newRotation);
		}
	}

    void Fire(bool fire){
		if(fire){
            if(lastShootTime+fireRate<Time.fixedTime){
				GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, bulletSpawn.position, bulletSpawn.rotation, 0);
				lastShootTime = Time.fixedTime;
			}
		} 
	}

    //void Dodge(bool dodging)
    //{
    //    if (dodging)
    //    {
    //        Debug.Log("Successful dodge ready to use");
    //        playerData.dodgeUsed();
    //    }
    //    //if(dodging && !playerData.canDodge())
    //    //{
    //    //    Debug.Log("Cooldown not complete!");
    //    //    //notify the player that you cannot dodge yet
    //    //}
    //    else
    //    {
    //        //default case when f is not pressed
    //    }
    //}




    //[PunRPC]
    //public IEnumerator Dodge(bool dodge)
    //{
    //    dodgeColour = Color.magenta;
    //    if (dodge)
    //    {
    //     for(int i = 0; i < 5; i++)
    //        {
    //            //creates a flashing-effect when the dodging key is pressed
    //            playerColour = dodgeColour;
    //            yield return new WaitForSeconds(.1f);
    //            playerColour = revertCol;
    //            yield return new WaitForSeconds(.1f);
    //        }
    //    }
    //}

}
