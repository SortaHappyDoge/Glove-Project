using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;
using UnityEditor.VersionControl;
using System.Net.WebSockets;
public class SocketRecieverNEW : MonoBehaviour
{
    Socket server;
    Thread SocketThread;
    public int messageId;
    public float[] receivedData;
    public bool isReceivedMessage;  

    public ProjectESP projectESP;

    void Start(){
        Application.runInBackground = true;
        InitiateSocket();

    }

    void Update(){
        if (messageId == 0)
        {
        
        }
    }

    void OnApplicationQuit()
    {
        // Clean up on application quit
        if (SocketThread != null && SocketThread.IsAlive) 
            SocketThread.Abort();
            server.Close();
    }
    void OnDisable()
    {
        // Ensure the thread is stopped when the object is disabled or destroyed
        if (SocketThread != null && SocketThread.IsAlive)
        {
            SocketThread.Abort();
            server.Close();
        }
    }


    void InitiateSocket(){
        SocketThread = new Thread(SocketRun);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

    void SocketRun(){
        Debug.Log("Thread Initiated...");

        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //var buffer = new MemoryStream();        

        IPAddress ip = getIPAddress();
        IPEndPoint localEndPoint = new IPEndPoint(ip, 8000);
        //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 8000);

        server.Bind(localEndPoint);

        Debug.Log("Server Initiated, IP: " + ip);
        while(true){

            //data = null;
            var bytes = new byte[1024];
            int bytesReceived = server.Receive(bytes);
            //data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

            byte[] dataBytes = new byte[bytesReceived - 1];
            
            Array.Copy(bytes, 1, dataBytes, 0, bytesReceived - 1);

            messageId = bytes[0];
            Debug.Log("Message Received" + " " + bytesReceived);
            if(messageId == 0)
            {
                var data = new float[dataBytes.Length / sizeof(float)];
                Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);

                projectESP.FetchData(data);
            }

            if(messageId == 2)
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
