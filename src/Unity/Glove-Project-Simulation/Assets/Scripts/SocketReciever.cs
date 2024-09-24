using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MyStruct
{
    public float Field1;
    public float Field2;
    public int Field3;
}

public class SocketReciever : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isReceiving = true;
    public int port = 8000;

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        while (isReceiving)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("Received: " + message);
                
                // You can now process the message, such as parsing it into a format you need
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving data: " + e.Message);
            }
        }
    }

    public static MyStruct ByteArrayToStruct(byte[] byteArray)
    {
        // Pin the array to a fixed memory address.
        GCHandle handle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);

        try
        {
            // Get the pointer to the data in the byte array.
            IntPtr ptr = handle.AddrOfPinnedObject();

            // Convert the byte array (pointer) into the struct.
            return Marshal.PtrToStructure<MyStruct>(ptr);
        }
        finally
        {
            // Release the pinned memory handle.
            handle.Free();
        }
    }


    void OnApplicationQuit()
    {
        isReceiving = false;
        if (receiveThread != null) receiveThread.Abort();
        udpClient.Close();
    }
}
