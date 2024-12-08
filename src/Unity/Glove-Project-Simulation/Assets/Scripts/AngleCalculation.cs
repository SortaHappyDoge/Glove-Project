using System;
using UnityEngine;

public class AngleCalculation : MonoBehaviour
{
    [SerializeField] GameObject point_0;
    [SerializeField] GameObject point_1;
    [SerializeField] GameObject point_2;

    public string angleName;

    public Vector3 angleDirection = Vector3.one;
    GameObject middle_point;

    void Start(){
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
