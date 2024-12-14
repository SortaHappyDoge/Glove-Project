using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class TouchDetection : MonoBehaviour
{
    public ProjectLandmarks projectLandmarks;
    private List<GameObject> objects;
    // These are temporary
    private bool isLeftIndex = false;
    private bool isLeftMiddle = false;
    private bool isLeftRing = false;
    private bool isLeftPinky = false;
    private bool isRightIndex = false;
    private bool isRightMiddle = false;
    private bool isRightRing = false;
    private bool isRightPinky = false;

    // Start is called before the first frame update
    void Start()
    {
        objects = projectLandmarks.objects;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(PrintDistance(objects[0], objects[1]));
    }
    
    void PlaySounds()
    {
        
    }

    bool IsCheckDistance(GameObject obj1, GameObject obj2, float maxDistance)
    {
        float distance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
        return distance <= maxDistance;
    }
    
    float PrintDistance(GameObject obj1, GameObject obj2)
    {
        float distance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
        return distance;
    }
}
