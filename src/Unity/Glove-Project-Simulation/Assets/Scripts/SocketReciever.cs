using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Recieved
{
    public float pitch;
    public float roll;
    public int id;
}

public class SocketReciever : MonoBehaviour
{
    System.Threading.Thread SocketThread;
    
    public void Start()
    {
        Application.runInBackground = true;
        StartServer();
        new Thread(() =>
        {
            /*SocketReciever client = new SocketReciever();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            client.Connect(ep);
            */
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 8001);
            Socket newsock = new Socket(AddressFamily.InterNetwork, socketType:SocketType.Dgram, protocolType:ProtocolType.Udp);
            newsock.Bind(ep);

            while (true)
            {
                byte[] data;
                newsock.Receive(data);
                print(recieved.ToString());

                float[] split = Array.ConvertAll(recieved.ToString().Split(","), float.Parse);
                Recieved message; message.pitch = split[0]; message.roll = split[1];
                transform.rotation = new Quaternion(split[0], 0, split[1], 0);
            }
        }).Start();
        
    }
    void StartServer()
    {
        SocketThread = new System.Threading.Thread(NetworkCode())
    }
    void NetworkCode()
    {
       
    }
}
