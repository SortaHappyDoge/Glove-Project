using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class ReceiveLandmarks : MonoBehaviour
{
    public bool receiveLandmarks = true;
    public bool hasReceivedMessage = false;
    public string receivedMessage;
    private UdpClient udpClient;  // UDP client to receive packets
    public Thread receiveThread; // Separate thread for receiving data
    public int listenPort = 8000; // The port to listen on

    // Start is called before the first frame update
    void Start()
    {
        if(receiveLandmarks){
            // Start the receiver on a new thread
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Debug.Log($"UDP receiver started, listening on port {listenPort}.");
        }
    }

    public void ReceiveData()
    {
        udpClient = new UdpClient(listenPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(/*IPAddress.Parse("27.0.0.1")*/IPAddress.Any, listenPort);

        try
        {
            while (true)
            {
                // Receive data from any IP address
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                hasReceivedMessage = true;
                //Debug.Log($"Received: {receivedMessage} from {remoteEndPoint.Address}:{remoteEndPoint.Port}");
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }
        finally
        {
            udpClient.Close();
        }
    }

    private void OnApplicationQuit()
    {
        // Clean up on application quit
        if (udpClient != null) 
            udpClient.Close();
        if (receiveThread != null && receiveThread.IsAlive) 
            receiveThread.Abort();
    }
    void OnDisable()
    {
        // Ensure the thread is stopped when the object is disabled or destroyed
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }


// Update is called once per frame
void Update()
    {
        
    }
}
