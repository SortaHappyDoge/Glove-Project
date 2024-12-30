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
        bool[] L = checkTouchState.leftStates;
        bool[] R = checkTouchState.rightStates;
        Debug.Log($"id0:{L[0]}, id1:{L[1]}, id2 :{L[2]}, id3 :{L[3]}, id4 :{L[4]}, id5 :{L[5]}, id6 :{L[6]}, id7 :{L[7]}, id8 :{L[8]}, id9 :{L[9]}, id10 :{L[10]}, id11 :{L[11]}, id12 :{L[12]}, id13 :{L[13]}, id14 :{L[14]}, id15 :{L[15]}, id16 :{L[16]}, id17 :{L[17]}, id18 :{L[19]}, id20 :{L[20]}, \nid0:{R[0]}, id1:{R[1]}, id2 :{R[2]}, id3 :{R[3]}, id4 :{R[4]}, id5 :{R[5]}, id6 :{R[6]}, id7 :{R[7]}, id8 :{R[8]}, id9 :{R[9]}, id10 :{R[10]}, id11 :{R[11]}, id12 :{R[12]}, id13 :{R[13]}, id14 :{R[14]}, id15 :{R[15]}, id16 :{R[16]}, id17 :{R[17]}, id18 :{R[19]}, id20 :{R[20]},");   
    }
    int BoolToIntConvert(bool value)
    {
        return value ? 1 : 0;
    }
}
