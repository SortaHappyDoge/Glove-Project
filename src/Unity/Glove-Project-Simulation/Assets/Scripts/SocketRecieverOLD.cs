//OLD CODE

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;

public struct Recieved
{
    public float pitch;
    public float roll;
    public int id;
}

public class SocketRecieverOLD : MonoBehaviour
{
    public Recieved message;
    void Start(){
        
        Application.runInBackground = true;

        new Thread(() => {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1234);
            Socket newsock = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
            newsock.Bind(ipep);
            Console.WriteLine("Waiting for a client...");

            

            while(true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                byte[] data = new byte[1024];
                int recv;
                recv = newsock.ReceiveFrom(data, ref Remote);

                Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

                float[] split = Array.ConvertAll(Encoding.ASCII.GetString(data, 0, recv).Split(","), float.Parse);
                message.pitch = split[0]; message.roll = split[1];

            }
        }).Start();
    }


    void FixedUpdate(){
        transform.rotation = Quaternion.Euler(message.roll*-90, 0, message.pitch*90);
    }
}