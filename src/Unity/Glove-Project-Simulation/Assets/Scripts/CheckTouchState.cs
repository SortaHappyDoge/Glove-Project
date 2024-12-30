    using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class CheckTouchState : MonoBehaviour
{
    public int id = 0;
    public int hand = 0;
    public bool[] leftStates = new bool[21];
    public bool[] rightStates = new bool[21];

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision){
        collision.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        if (collision.transform.GetComponent<CheckTouchState>() != null)
        {
            switch (collision.transform.GetComponent<CheckTouchState>().hand)
            {
                case 0:
                    leftStates[collision.transform.GetComponent<CheckTouchState>().id] = true;
                    break;
                case 1:
                    rightStates[collision.transform.GetComponent<CheckTouchState>().id] = true;
                    break;
                default:
                    break;
            }
        }
    }
    void OnTriggerExit(Collider collision){
        collision.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        if (collision.transform.GetComponent<CheckTouchState>() != null)
        {
            switch (collision.transform.GetComponent<CheckTouchState>().hand)
            {
                case 0:
                    leftStates[collision.transform.GetComponent<CheckTouchState>().id] = false;
                    break;
                case 1:
                    rightStates[collision.transform.GetComponent<CheckTouchState>().id] = false;
                    break;
                default:
                    break;
            }
        }
    }
}
