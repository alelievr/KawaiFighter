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
    SerialPort sp = new SerialPort("/dev/cu.usbmodem1431", 115200);
    //////////////////////////////////////////////

    // Use this for initialization
    void Start()
    {
        try
        {
            sp.Open();
            StartCoroutine
            (AsynchronousReadFromArduino
                ((string s) => Debug.Log(s),     // Callback
                    () => Debug.LogError("Error!"), // Error callback
                    10f                             // Timeout (seconds)
                )
            );
        }
        catch (System.Exception e)
        {
            Debug.Log("failed to open arduino serial port !" + e);
        }
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
    }

    public string ReadFromArduino(int timeout = 0)
    {
        sp.ReadTimeout = timeout;
        try
        {
            return sp.ReadLine();
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
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
                yield return null;
            }
            else
                yield return new WaitForSeconds(0.05f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
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
