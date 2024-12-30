using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AngleCalculation : MonoBehaviour
{
    [SerializeField] GameObject point_0;
    [SerializeField] GameObject point_1;
    [SerializeField] GameObject point_2;

    public string angleName;

    Vector3 angleDirection = new Vector3(1, 1, 1);
    GameObject middle_point;


    void Start(){
        if(SceneManager.GetActiveScene().name == "Glove Projection"){
            point_0 = transform.parent.gameObject;
            point_1 = transform.gameObject;
            point_2 = transform.GetChild(0).gameObject;
            angleName = transform.parent.name + " - " + transform.name + " - " + transform.GetChild(0).name;
        }
        middle_point = point_1;
    }

    public double GetAngle(){
        Vector3 point_0_to_mid = point_0.transform.position - middle_point.transform.position;
        Vector3 point_2_to_mid = point_2.transform.position - middle_point.transform.position;

        float angle = Vector3.SignedAngle(point_0_to_mid, point_2_to_mid, angleDirection);

        if(angle >= 0) angle = angle;
        else angle = 360 + angle;
        return angle;
    }
}
