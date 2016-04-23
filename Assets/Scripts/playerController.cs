using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System.IO.Ports;
using System;

public class playerController : MonoBehaviour
{

    public float jumpPower = 10F;
    public float gravityScale = 1F;
    public float speed = 5F;

    private CharacterController cc;
    private Animator anim;
    private Vector3 inputVelocity;

    //////////////////////////////////////////////
    private System.Threading.Thread arduinoThread = null;
    private SerialPort sp = new SerialPort("/dev/cu.usbmodem1431", 38400, Parity.None, 8, StopBits.One);

    //////////////////////////////////////////////

    void arduinoSerialTask()
    {
		char[]	buff = new char[1024];
        try
        {
            sp.Open();
            while (true)
            {
				Debug.Log("readline from serial: ");
				Debug.Log(sp.Read(buff, 0, buff.Length));
				System.Threading.Thread.Sleep(20);
/*                ReadFromArduino(
                    (string s) => Debug.Log(s),     // Callback
                        () => Debug.LogError("Error!"), // Error callback
                        10f                             // Timeout (seconds)
                    );
                System.Threading.Thread.Sleep(30);*/
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("failed to open arduino serial port !" + e);
        }
		Debug.Log("out !");
    }

    // Use this for initialization
    void Start()
    {
//        arduinoThread = new System.Threading.Thread(arduinoSerialTask);
//        arduinoThread.Start();

		sp.Open();
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

    }
	
	void OnDestroy() {
		sp.Close();
		arduinoThread.Abort();
	}

    // Update is called once per frame
    void Update()
    {
/*		if (sp.IsOpen) {
			Debug.Log(""+ sp.ReadByte());
		}*/
    }

    public void ReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            Debug.Log("reading from arduino !");
            try
            {
                dataString = sp.ReadLine();
                Debug.Log("dataString: " + dataString);
            }
            catch (TimeoutException e)
            {
                Debug.Log("timeout: " + e);
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                return;
            }

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;
        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
    }

    void FixedUpdate()
    {
        inputVelocity.z = CrossPlatformInputManager.GetAxis("Horizontal") * speed;

        if (cc.isGrounded)
        {
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                inputVelocity.y += jumpPower;
                anim.SetBool("jump", true);
            }
            else
            {
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
