using System;
using UnityEngine;

public class AngleCalculation : MonoBehaviour
{
    [SerializeField] GameObject point_0;
    [SerializeField] GameObject point_1;
    [SerializeField] GameObject point_2;

    public Vector3 angleDirection = Vector3.one;
    GameObject middle_point;

    void Start(){
        middle_point = point_0;
    }
    void Update(){
        Vector3 point_1_to_mid = point_1.transform.position - middle_point.transform.position;
        Vector3 point_2_to_mid = point_2.transform.position - middle_point.transform.position;

        float angle = Vector3.SignedAngle(point_1_to_mid, point_2_to_mid, angleDirection);

        if(angle >= 0) angle = angle;
        else angle = 360 + angle;
        Debug.Log(angle);
    }
}
