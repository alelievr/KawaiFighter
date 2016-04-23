//#define USE_ARDUINO_CONTROLLER

using UnityEngine;
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
    private SerialPort sp = new SerialPort();
    
    float[] accel = new float[3];
    float[] gyro = new float[3];
    float[] gyroOffset = new float[3];
    //////////////////////////////////////////////

    void arduinoSerialTask()
    {
        gyroOffset[0] = -746;
        gyroOffset[1] = -355;
        gyroOffset[2] = 459;        sp.BaudRate = 38400;
        
        sp.PortName = "/dev/cu.usbmodem1411";
        sp.DataBits = 8;
        sp.StopBits = StopBits.One;
        sp.Parity = Parity.None;
        sp.Handshake = Handshake.RequestToSend;
        try
        {
            sp.Open();
            if (!sp.IsOpen)
                return ;
            sp.ReadTimeout = 100;
            sp.WriteTimeout = 100;
            while (true)
            {
                sp.Write("d");
                string accelgyroData = sp.ReadLine();

                string[] list = accelgyroData.Split('\t');

                for (int i = 0; i < 6; i++) {
                    if (i < 3)
                        accel[i] = Convert.ToSingle(list[i + 1]);
                    else
                        gyro[i - 3] = Convert.ToSingle(list[i + 1]);
                }

                for (int i = 0; i < 3; i++)
                    Debug.Log("gyro = " + gyro[i]);
                for (int i = 0; i < 3; i++)
                    Debug.Log("accel = " + accel[i]);
                
                System.Threading.Thread.Sleep(20);
                //				System.Threading.Thread.Sleep(2);
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
        #if USE_ARDUINO_CONTROLLER
            arduinoThread = new System.Threading.Thread(arduinoSerialTask);
            arduinoThread.Start();
        #endif

        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void OnDestroy()
    {
        sp.Close();
        //	arduinoThread.Abort();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r")) {
            gyroOffset = gyro;
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        gameObject.transform.Rotate((gyro[0] + gyroOffset[0]) / 2000, (gyro[1] + gyroOffset[1]) / 2000, (gyro[2] + gyroOffset[2]) / 2000);
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
