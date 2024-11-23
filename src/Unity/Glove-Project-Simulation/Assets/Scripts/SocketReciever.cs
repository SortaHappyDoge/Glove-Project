using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
//using System.BitConverter;
using System;

public class SocketReciever : MonoBehaviour{
    private Thread socketThread;
    private UdpClient udpClient;  // UDP client to receive packets
    public const int socketPort = 8000;


    void Start(){
        
        socketThread = new Thread(new ThreadStart(RunServer));
        //socketThread.isBackground = true;
        socketThread.Start();
        Debug.Log("Started UDP server...");
    }

    void RunServer(){
        udpClient = new UdpClient(socketPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("27.0.0.1"), socketPort); //Receive from any address

        try{
            while(true){
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                Debug.Log(receivedBytes.Length);
                switch(/*receivedBytes[0]*/2){
                    case 0: 
                        break;
                    case 1:
                        Debug.Log(Encoding.UTF8.GetString(receivedBytes));
                        break;
                    case 2:
                        byte[] buff = {receivedBytes[1], receivedBytes[2], receivedBytes[3], receivedBytes[4]};
                        Debug.Log(BitConverter.ToSingle(buff, 0));
                        break;
                }

            }
        }catch(SocketException err){
            Debug.Log($"Socket Exception: {err.Message}");
        }finally{
            udpClient.Close();
        }
    }

    private void OnApplicationQuit()
    {
        // Clean up on application quit
        if (udpClient != null) 
            udpClient.Close();
        if (socketThread != null && socketThread.IsAlive) 
            socketThread.Abort();
    }
}