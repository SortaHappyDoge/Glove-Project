using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
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
    UdpClient client = new UdpClient();
    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
    
    void Start()
    {
        client.Connect(ep);
    }
    private void Update()
    {
        var recieved = client.Receive(ref ep);
        

        float[] split = Array.ConvertAll(recieved.ToString().Split(","), float.Parse);
        Recieved message; message.pitch = split[0]; message.roll = split[1];
    }
}
