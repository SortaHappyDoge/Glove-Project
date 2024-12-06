using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;
//using System.Diagnostics;
public class SocketRecieverNEW : MonoBehaviour
{
    Socket server;
    Thread SocketThread;



    public GameObject demoCube;
    public Vector3 demoRotation; 
    float PI = 3.14f;
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
            //data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

            byte[] dataBytes = new byte[bytesReceived - 1];
            
            Array.Copy(bytes, 1, dataBytes, 0, bytesReceived - 1);

            if(bytes[0] == 1){
                var data = new float[dataBytes.Length / sizeof(float)];
                Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);  

                /* Demo to see the output values */
                demoRotation = new Vector3(data[3], 0, data[4]) / PI*180;  
            }

            if(bytes[0] == 2){
                float[] data = new float[dataBytes.Length / sizeof(float)];
                Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);

                /*  Attempt at turning float into landmarks :pray:
                List<> landmarks = new();
                float[] landmark = new();
                int j = 5;
                
                foreach(float value in data){
                    landmark.Add(value);
                    --j;
                    if(j == 0){
                        landmarks.Add(landmark);
                        j == 5;
                        landmark = new();
                    }
                }

                foreach(float value in data){
                    Debug.Log(value);
                }*/
            }

            Debug.Log(bytes[0]);

            /*
            foreach(var b in bytes){
                Debug.Log(b);
            }

            foreach(var b in dataByte){
                Debug.Log(b);
            }

            foreach(float f in data){
                Debug.Log(f);
            }
            */
            Thread.Sleep(1);
        }
    }
    
    void Update(){
        
        //demoCube.transform.eulerAngles = demoRotation; 
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
