using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System.IO.Ports;

public class playerController : MonoBehaviour {
	
	public float jumpPower = 10F;
	public float gravityScale = 1F;
	public float speed = 5F;
	
	private CharacterController	cc;
	private Animator			anim;
	private Vector3				inputVelocity;

	//////////////////////////////////////////////
	SerialPort	sp = new SerialPort("COM4", 115200);
	//////////////////////////////////////////////

	// Use this for initialization
	void Start () {
		cc = GetComponent< CharacterController >();
		anim = GetComponent< Animator >();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public string ReadFromArduino (int timeout = 0) {
		sp.ReadTimeout = timeout;        
		try {
			return sp.ReadLine();
		}
		catch (System.Exception) {
			return null;
		}
	}
	
	void arduinoController() {
		Debug.Log("arduino datas: " + ReadFromArduino());
	}
	
	void FixedUpdate() {
		inputVelocity.z = CrossPlatformInputManager.GetAxis("Horizontal") * speed;

		if (cc.isGrounded) {
			if (CrossPlatformInputManager.GetButtonDown("Jump")) {
				inputVelocity.y += jumpPower;
				anim.SetBool("jump", true);
			} else {
				anim.SetBool("jump", false);
				inputVelocity.y = 0;
			}
		}
		if (inputVelocity.z < 0)
			transform.localRotation = Quaternion.Euler(0, 180, 0);
		else if (inputVelocity.z > 0)
			transform.localRotation = Quaternion.Euler(0, 0, 0);
		
		inputVelocity.y += Physics.gravity.y * gravityScale * Time.deltaTime * 5;
		if (inputVelocity != Vector3.zero)
			cc.Move(inputVelocity * Time.deltaTime);
		anim.SetFloat("speed", Mathf.Abs(cc.velocity.z));
	}
}
