using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;
using UnityEditor.VersionControl;
using System.Net.WebSockets;
//using System.Diagnostics;
public class SocketRecieverNEW : MonoBehaviour
{
    //public Vector3 demoRotation;
    //public GameObject demoCube;
    //float PI = 3.14f;
    Socket server;
    Thread SocketThread;
    public int messageId;
    public float[] receivedData;
    public bool isReceivedMessage;  
    void Start(){
        Application.runInBackground = true;
        InitiateSocket();

    }

    void Update(){
        /*if (messageId == 1)
        {
            demoCube.transform.eulerAngles = demoRotation;
        }*/
    }

    void OnApplicationQuit()
    {
        // Clean up on application quit
        if (SocketThread != null && SocketThread.IsAlive) 
            SocketThread.Abort();
    }
    void OnDisable()
    {
        // Ensure the thread is stopped when the object is disabled or destroyed
        if (SocketThread != null && SocketThread.IsAlive)
        {
            SocketThread.Abort();
        }
    }


    void InitiateSocket(){
        SocketThread = new Thread(SocketRun);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

    void SocketRun(){
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //var buffer = new MemoryStream();        

        IPAddress ip = getIPAddress();
        IPEndPoint localEndPoint = new IPEndPoint(ip, 8000);
        //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 8000);
        Debug.Log(ip);


        server.Bind(localEndPoint);

        while(true){

            //data = null;
            var bytes = new byte[1024];
            int bytesReceived = server.Receive(bytes);
            //data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

            byte[] dataBytes = new byte[bytesReceived - 1];
            
            Array.Copy(bytes, 1, dataBytes, 0, bytesReceived - 1);

            messageId = bytes[0];

            if(bytes[0] == 1)
            {
                var data = new float[dataBytes.Length / sizeof(float)];
                Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);  

                /* Demo to see the output values */
                //demoRotation = new Vector3(data[3], 0, data[4]) / PI*180;  
            }

            if(bytes[0] == 2)
            {
                var data = new float[dataBytes.Length / 4];
                Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);
                receivedData = data;

            }
            isReceivedMessage = true;
            Thread.Sleep(1);
        }
    }

    private IPAddress getIPAddress()
    {
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }

        }
        return null;
    }
}
