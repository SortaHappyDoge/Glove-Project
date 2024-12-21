using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintStates : MonoBehaviour
{
    [SerializeField] private CheckTouchState checkTouchState;
    public bool[] L;
    public bool[] R;
    // Start is called before the first frame update
    void Start()
    {
        bool[] L = checkTouchState.leftStates;
        bool[] R = checkTouchState.rightStates;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"id 0: {L[0]}, ");   
    }
    int BoolToIntConvert(bool value)
    {
        return value ? 1 : 0;
    }
}
