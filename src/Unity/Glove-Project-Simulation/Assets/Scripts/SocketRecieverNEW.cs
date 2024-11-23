using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;
public class SocketRecieverNEW : MonoBehaviour
{
    Socket server;
    Thread SocketThread;


    void Start(){
        Application.runInBackground = true;
        InitiateSocket();
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
            Debug.Log(bytesReceived);
            //data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

            byte[] dataByte = new byte[bytesReceived - 1];
            
            Array.Copy(bytes, 1, dataByte, 0, 16);

            var data = new int[dataByte.Length / sizeof(int)];
            Buffer.BlockCopy(dataByte, 0, data, 0, dataByte.Length);
            

            /*foreach(var b in bytes){
                Debug.Log(b);
            }
            foreach(var b in dataByte){
                Debug.Log(b);
            }*/
            /*foreach(float f in data){
                Debug.Log(f);
            }*/
            Thread.Sleep(1);
        }
    }

    void OnDisable(){
        server.Close();
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
