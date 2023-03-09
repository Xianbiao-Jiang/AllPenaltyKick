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


public class MessagesManager : MonoBehaviour
{
    [HideInInspector] public bool isTxStarted = false;

    string IP = "192.168.3.5"; // local host
    [SerializeField] int rxPort = 8000; // port to receive data from Python on
    [SerializeField] int txPort = 8001; // port to send data to Python on

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    public Transform ball;
    BallPosition ballPosition;
    BallPosition receiveBallPosition;
    ShotTrigger shotTrigger;
    HandPosition leftHandPosition;
    HandPosition rightHandPosition;
    public Transform leftHand;
    public Transform rightHand;
    // udp variable
    [HideInInspector] public bool isBallStarted = false;
    [HideInInspector] public bool isLeftHandStarted = false;
    [HideInInspector] public bool isRightHandStarted = false;

    Rigidbody ballRigidbody;
    Vector3 ballDefaultPosition;

    public bool isRegister = false;

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

    void Awake()
    {
        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);
        //        client2 = new UdpClient(txPort);
        ballPosition = new BallPosition();
        receiveBallPosition = new BallPosition();
        shotTrigger = new ShotTrigger();
        leftHandPosition = new HandPosition();
        rightHandPosition = new HandPosition();

        ballRigidbody = ball.gameObject.GetComponent<Rigidbody>();
        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");

        RegisterServer();

        ballDefaultPosition = ball.transform.position;



        //       StartCoroutine(SendDataCoroutine()); // DELETE THIS: Added to show sending data from Unity to Python via UDP
    }

    void Update()
    {
        RegisterServer();

#if UNITY_STANDALONE_WIN
        if (isRegister)
        {
            if (isBallStarted)
            {
                ball.position = receiveBallPosition.ballPosition;
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
#endif


        /*
        if (leftHand.position != leftHandPosition.handPosition || leftHand.rotation.eulerAngles != leftHandPosition.handRotation || leftHand.localScale != leftHandPosition.handScale)
        {
            leftHandPosition.keyword = "LeftHandPosition";
            leftHandPosition.handPosition = leftHand.position;
            leftHandPosition.handRotation = leftHand.rotation.eulerAngles;
            leftHandPosition.handScale = leftHand.localScale;
            SendData(JsonNet.Serialize(leftHandPosition));
        }

        if (rightHand.position != rightHandPosition.handPosition || rightHand.rotation.eulerAngles != rightHandPosition.handRotation || rightHand.localScale != rightHandPosition.handScale)
        {
            rightHandPosition.keyword = "RightHandPosition";
            rightHandPosition.handPosition = rightHand.position;
            rightHandPosition.handRotation = rightHand.rotation.eulerAngles;
            rightHandPosition.handScale = rightHand.localScale;
            SendData(JsonNet.Serialize(rightHandPosition));
        }

        if (ball.position != ballPosition.ballPosition)
        {
            ballPosition.ballPosition = ball.position;
            SendData(JsonNet.Serialize(ballPosition));
        }
        */

#if UNITY_ANDROID

        if (isRegister)
        {


            if (leftHand.position != leftHandPosition.handPosition || leftHand.rotation.eulerAngles != leftHandPosition.handRotation)
            {
                leftHandPosition.keyword = "LeftHandPosition";
                leftHandPosition.handPosition = leftHand.position;
                leftHandPosition.handRotation = leftHand.rotation.eulerAngles;
                SendData(JsonNet.Serialize(leftHandPosition));
            }

            if (rightHand.position != rightHandPosition.handPosition || rightHand.rotation.eulerAngles != rightHandPosition.handRotation)
            {
                rightHandPosition.keyword = "RightHandPosition";
                rightHandPosition.handPosition = rightHand.position;
                rightHandPosition.handRotation = rightHand.rotation.eulerAngles;
                SendData(JsonNet.Serialize(rightHandPosition));
            }

            if (ball.position != ballPosition.ballPosition)
            {
                ballPosition.ballPosition = ball.position;
                SendData(JsonNet.Serialize(ballPosition));
            }
        }
            
#endif

    }


    void InitialBallAndHand()
    {
        leftHandPosition.keyword = "LeftHandPosition";
        leftHandPosition.handPosition = leftHand.position;
        leftHandPosition.handRotation = leftHand.rotation.eulerAngles;
        SendData(JsonNet.Serialize(leftHandPosition));
        rightHandPosition.keyword = "RightHandPosition";
        rightHandPosition.handPosition = rightHand.position;
        rightHandPosition.handRotation = rightHand.rotation.eulerAngles;
        SendData(JsonNet.Serialize(rightHandPosition));
        ballPosition.ballPosition = ball.position;
        SendData(JsonNet.Serialize(ballPosition));
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

                //    Debug.Log(objectProperties);
                //   Debug.Log(objectProperties.objectRotation);
                //   Debug.Log(objectProperties.objectScale);
#if UNITY_ANDROID

#endif

#if UNITY_STANDALONE_WIN
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

#endif


                ProcessInput(text);
            }
            catch (Exception err)
            {
                string s = err.ToString();
            }
        }
    }

    private void ProcessInput(string input)
    {
        // PROCESS INPUT RECEIVED STRING HERE
        if (input.Contains("BallPosition"))
        {
            isTxStarted = true;
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
