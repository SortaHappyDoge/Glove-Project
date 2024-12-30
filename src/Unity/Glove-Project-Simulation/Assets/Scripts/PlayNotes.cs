using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayNotes : MonoBehaviour
{
    public CheckTouchState checkTouchState;
    public bool[] fingerTips = new bool[4];
    
    // Start is called before the first frame update
    void Start()
    {
        switch (checkTouchState.hand)
        {
            case 0:
                fingerTips = new bool[4] {checkTouchState.leftStates[8], checkTouchState.leftStates[12], checkTouchState.leftStates[16], checkTouchState.leftStates[20]} ;
                break;
            case 1:
                fingerTips = new bool[4] {checkTouchState.leftStates[8], checkTouchState.leftStates[12], checkTouchState.leftStates[16], checkTouchState.leftStates[20]} ;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
