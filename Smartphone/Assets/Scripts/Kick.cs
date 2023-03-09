using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Json.Net;

public class Kick : MonoBehaviour
{
    public Transform ball;
    public Rigidbody ballRigidbody;
    public Transform kickDirection;
    public KickDirectionManager kickDirectionManager;
    bool notReset = true;
    private Vector3 defaultPosition;
    BallPosition ballPosition;
    BallPosition receiveBallPosition;

    // udp variable
    [HideInInspector] public bool isBallStarted = false;
    [HideInInspector] public bool isLeftHandStarted = false;
    [HideInInspector] public bool isRightHandStarted = false;


    string IP = "192.168.3.5"; // local host
    [SerializeField] int rxPort = 8000; // port to receive data from Python on
    [SerializeField] int txPort = 8001; // port to send data to Python on

    int i = 0; // DELETE THIS: Added to show sending data from Unity to Python via UDP
    // Create necessary UdpClient objects
    UdpClient client;
    UdpClient client2;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    public bool isRegister = false;

    ShotTrigger shotTrigger;


    HandPosition leftHandPosition;
    HandPosition rightHandPosition;
    public Transform leftHand;
    public Transform rightHand;


    IEnumerator SendDataCoroutine() // DELETE THIS: Added to show sending data from Unity to Python via UDP
    {
        while (true)
        {
            SendData("Sent from Unity: " + i.ToString());
            i++;

            yield return new WaitForSeconds(1f);
        }
    }

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
            //            client2.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        ballPosition = new BallPosition();
        receiveBallPosition = new BallPosition();
        ball = this.transform;
        defaultPosition = ball.position;
        ballRigidbody = ball.GetComponent<Rigidbody>();

        leftHandPosition = new HandPosition();
        rightHandPosition = new HandPosition();

        // udp setup
        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);
        // Create local client
        client = new UdpClient(rxPort);
        //        client2 = new UdpClient(txPort);


        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");

    }


    
    void Update()
    {

        RegisterServer();
        if (isRegister)
        {
            if (isBallStarted)
            {
                this.transform.position = receiveBallPosition.ballPosition;
            }

            if (isLeftHandStarted)
            {
                leftHand.position = leftHandPosition.handPosition;
                leftHand.eulerAngles = leftHandPosition.handRotation;
            }

            if (isRightHandStarted)
            {
                rightHand.position = rightHandPosition.handPosition;
                rightHand.eulerAngles = rightHandPosition.handRotation;
            }
        }

    }
    

    private void ResetBall()
    {
        if (notReset)
        {
            ballRigidbody.velocity = new Vector3(0, 0, 0);
            ballRigidbody.rotation = Quaternion.identity;
            this.transform.position = defaultPosition;
            notReset = false;
        }
    }

    private void BallSetting(Vector3 position, Vector3 direction, float forceScale, int index)
    {
        shotTrigger.ShotDirection = direction;
        shotTrigger.force = forceScale.ToString();
    }


    private void RegisterServer()
    {
        if (!isRegister)
        {
            SendData("");
        }
    }
    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                if (text.Contains("Accept"))
                {
                    isRegister = true;
                }

                if (text.Contains("BallPosition"))
                {
                    receiveBallPosition = JsonNet.Deserialize<BallPosition>(text);
                    isBallStarted = true;
                }

                if (text.Contains("LeftHandPosition"))
                {
                    leftHandPosition = JsonNet.Deserialize<HandPosition>(text);
                    isLeftHandStarted = true;
                }

                if (text.Contains("RightHandPosition"))
                {
                    rightHandPosition = JsonNet.Deserialize<HandPosition>(text);
                    isRightHandStarted = true;
                }
                //    Debug.Log(objectProperties);
                //   Debug.Log(objectProperties.objectRotation);
                //   Debug.Log(objectProperties.objectScale);




                //print(">> " + text);
           //     ProcessInput(text);
            }
            catch (Exception err)
            {
                     print(err.ToString());
            }
        }
    }

    private void ProcessInput(string input)
    {
        // PROCESS INPUT RECEIVED STRING HERE
        if (input.Contains("LeftHandPosition"))
        {

        }
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }




}

public class BallPosition
{
    public string keyword = "BallPosition";
    public Vector3 ballPosition;
}

public class HandPosition
{
    public string keyword = "HandPosition";
    public Vector3 handPosition;
    public Vector3 handRotation;
}

public class ShotTrigger
{
    public string keyword = "ShotTrigger";
    public Vector3 ShotDirection;
    public string force;
}