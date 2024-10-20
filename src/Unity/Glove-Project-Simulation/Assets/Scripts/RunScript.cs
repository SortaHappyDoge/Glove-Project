using System.Diagnostics;
using UnityEngine;

public class RunScript : MonoBehaviour
{
    public bool runScript = true;
    string pythonPath = @"C:\Users\MrSykenro\AppData\Local\Programs\Python\Python312\python.exe";
    public string pythonParameter = @"C:\Users\MrSykenro\Desktop\Projects\glove-project\src\python\UDP_landmark_transmitter.py";  
    // Start is called before the first frame update
    void Start()
    {
        if(runScript){        
            Process.Start(pythonPath, pythonParameter);
        }
    }
}
