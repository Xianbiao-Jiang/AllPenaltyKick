using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Json.Net;

public class TCPTestClient : MonoBehaviour {  	
	#region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread;
	public ShotTrigger receiveShotTrigger;
	public Transform ball;
	Rigidbody ballRigid;
	Vector3 defaultPosition;
	bool startShot = false;
	bool notReset = false;
	#endregion  	

	// Use this for initialization 	
	void Start () {
		defaultPosition = ball.position;
		ballRigid = ball.GetComponent<Rigidbody>();
		receiveShotTrigger = new ShotTrigger();
		ConnectToTcpServer();     
	}  	
	// Update is called once per frame
	void Update () 
	{
#if UNITY_ANDROID
		if (startShot)
        {
			notReset = true;
			ResetBall();
			ballRigid.AddForce(receiveShotTrigger.ShotDirection * float.Parse(receiveShotTrigger.force), ForceMode.VelocityChange);
			startShot = false;
		}

#endif

#if UNITY_STANDALONE_WIN

#endif

	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData() { 		
		try { 			
			socketConnection = new TcpClient("192.168.3.5", 25001);  			
//			socketConnection = new TcpClient("192.168.3.4", 25001);  			
			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						receiveShotTrigger = JsonNet.Deserialize<ShotTrigger>(serverMessage);

						Debug.Log("server message received as: " + serverMessage);
						startShot = true;

					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage() {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
//				string clientMessage = "This is a message from one of your clients."; 				
				string clientMessage = "This is a message from PC."; 				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     

	}
	private void ResetBall()
	{
		if (notReset)
		{
			ballRigid.velocity = new Vector3(0, 0, 0);
			ballRigid.rotation = Quaternion.identity;
			ball.position = defaultPosition;
			notReset = false;
		}
	}
	void OnDisable()
	{
		if (clientReceiveThread != null)
			clientReceiveThread.Abort();

		socketConnection.Close();
	}
}