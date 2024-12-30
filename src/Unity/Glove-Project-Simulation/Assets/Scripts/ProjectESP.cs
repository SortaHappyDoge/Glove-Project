using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectESP : MonoBehaviour
{
    public Transform[] joints = new Transform[22];
    Quaternion[] mpu_rotations = new Quaternion[12];
    
    void FixedUpdate(){
        RotateJoints();
    }
    
    public void FetchData(float[] data){
        for(int i = mpu_rotations.Length; i>0; i--){ //x^2, ebem, eben
            mpu_rotations[mpu_rotations.Length-i] = 
            new Quaternion(data[data.Length-(i*4)+3],
                           data[data.Length-(i*4)], 
                           data[data.Length-(i*4)+1], 
                           data[data.Length-(i*4)+2]
                           );
        }
    }

    void RotateJoints(){
        //joints[1].rotation = mpu_rotations[10];
        joints[2].localRotation = mpu_rotations[1];
        joints[3].localRotation = mpu_rotations[6];
        
        joints[5].localRotation = mpu_rotations[2];
        joints[6].localRotation = mpu_rotations[7];
        joints[7].localRotation = mpu_rotations[7];
        
        joints[9].localRotation = mpu_rotations[3];
        joints[10].localRotation = mpu_rotations[8];
        joints[11].localRotation = mpu_rotations[8];
        
        joints[13].localRotation = mpu_rotations[4];
        joints[14].localRotation = mpu_rotations[9];
        joints[15].localRotation = mpu_rotations[9];
        
        joints[17].localRotation = mpu_rotations[5];
        joints[18].localRotation = mpu_rotations[10];
        joints[19].localRotation = mpu_rotations[10];

        joints[0].localRotation = mpu_rotations[0];
        //joints[21].localRotation = mpu_rotations[11];
    }
}
