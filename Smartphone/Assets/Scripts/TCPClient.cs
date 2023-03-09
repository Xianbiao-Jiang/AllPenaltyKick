using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Json.Net;
public class TCPClient : MonoBehaviour
{
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;

	public KickDirectionManager kickDirectionManager;
	public ShotTrigger shotTrigger;
	bool notReset = true;
	bool accumulateForce = false;
	float counter = 0;
	public Transform ball;
	Rigidbody ballRigidbody;
	Vector3 defaultPosition;
	#endregion
	// Use this for initialization 	
	void Start()
	{
		defaultPosition = ball.position;
		ballRigidbody = ball.GetComponent<Rigidbody>();
		ConnectToTcpServer();
		shotTrigger = new ShotTrigger();
	}
	// Update is called once per frame
	void Update()
	{
		if (Input.touchCount > 0)
		{
			notReset = true;
			ResetBall();
			accumulateForce = true;
			counter += Time.deltaTime;
		}
		else if (Input.touchCount == 0 && accumulateForce)
		{
			BallSetting((kickDirectionManager.aimPose - defaultPosition).normalized, 10.0f * counter);
			SendMessage();

			accumulateForce = false;
		}

		if (accumulateForce == false)
		{
			counter = 0;
		}

	}

	private void ResetBall()
	{
		if (notReset)
		{
			ballRigidbody.velocity = new Vector3(0, 0, 0);
			this.transform.position = defaultPosition;
			notReset = false;
		}
	}

	private void BallSetting(Vector3 direction, float forceScale)
	{
		shotTrigger.ShotDirection = direction;
		shotTrigger.force = forceScale.ToString();
	}

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			//			socketConnection = new TcpClient("192.168.3.5", 25001);  			
			socketConnection = new TcpClient("192.168.3.5", 25001);
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// string clientMessage = "This is a message from one of your clients."; 				
				string clientMessage = JsonNet.Serialize(shotTrigger);
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}