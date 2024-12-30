using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JointMonitoring : MonoBehaviour
{
    public List<AngleCalculation> leftHandJoints = new List<AngleCalculation>();
    public List<AngleCalculation> rightHandJoints = new List<AngleCalculation>();


    void Update(){
        if(!(SceneManager.GetActiveScene().name == "Glove Projection")){
            FetchAngles(0);
        }
        else{
            
        }
    }

    void FetchAngles(int options){
        if(options == 0){
            foreach(AngleCalculation joint in leftHandJoints){
                Debug.Log(joint.angleName+" "+joint.GetAngle());
            }
            foreach(AngleCalculation joint in rightHandJoints){
                Debug.Log(joint.angleName+" "+joint.GetAngle());
            }
        }
        else if(options == 1){
            foreach(AngleCalculation joint in leftHandJoints){
                Debug.Log(joint.angleName+" "+joint.GetAngle());
            }
        }
        else{
            foreach(AngleCalculation joint in rightHandJoints){
                Debug.Log(joint.angleName+" "+joint.GetAngle());
            }
        }
    }



}
