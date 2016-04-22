using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class playerController : MonoBehaviour {
	
	public float jumpPower = 2.5F;
	public float gravityScale = 1F;
	
	private CharacterController	cc;

	// Use this for initialization
	void Start () {
		cc = GetComponent< CharacterController >();
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void FixedUpdate() {
		Vector3 mouvement = new Vector3(0, CrossPlatformInputManager.GetAxis("Vertical"), CrossPlatformInputManager.GetAxis("Horizontal"));

		if (cc.isGrounded) {
			mouvement.y = 0;
			if (CrossPlatformInputManager.GetButtonDown("Jump"))
				mouvement.y = jumpPower;
		}
		mouvement.y -= gravityScale * Physics.gravity.y * Time.deltaTime;
		if (mouvement != Vector3.zero)
			cc.Move(mouvement);
	}
}
