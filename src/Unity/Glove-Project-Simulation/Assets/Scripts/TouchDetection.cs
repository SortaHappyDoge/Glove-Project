using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDetection : MonoBehaviour
{
    TouchManager touchManager;
    public int joint_id;
    bool[] jointContacts = 
    new bool[22]{
        false, false, false, false, false, 
        false, false, false, false, false,
        false, false, false, false, false, 
        false, false, false, false, false, 
        false, false
        };
    void Start()
    {
        touchManager = GameObject.FindGameObjectWithTag("Touch Manager").transform.GetComponent<TouchManager>();
        transform.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        jointContacts[joint_id] = true;
    }

    void OnTriggerEnter(Collider collision){
        TouchDetection other = collision.transform.GetComponent<TouchDetection>();
        jointContacts[other.joint_id] = true;
        collision.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

        touchManager.Invoke("Command"+"_"+joint_id+"_"+other.joint_id ,0f);
    }
    void OnTriggerExit(Collider collision){
        collision.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        jointContacts[collision.transform.GetComponent<TouchDetection>().joint_id] = false;
    }
}
