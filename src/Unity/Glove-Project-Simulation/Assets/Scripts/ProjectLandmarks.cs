using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;

public class ProjectLandmarks : MonoBehaviour
{
    // Takes all values as floats converts handNo and landmarkId into integer
    public struct landmark
    {
        public float handNo;
        public float landmarkId;
        public float landmarkX;
        public float landmarkY;
        public float landmarkZ;

        public landmark(float handNo, float landmarkId, float landmarkX, float landmarkY, float landmarkZ){
            this.handNo = handNo;
            this.landmarkId = landmarkId;
            this.landmarkX = landmarkX;
            this.landmarkY = landmarkY;
            this.landmarkZ = landmarkZ;
        }

        public override string ToString()
        {
            return $"Hand: {handNo}, LandmarkId: {landmarkId}, X: {landmarkX}, Y: {landmarkY}, Z: {landmarkZ}";
        }
    }


    public ReceiveLandmarks receiveLandmarks;
    public string landmarkString;
    private List<landmark> landmarks = new();
    private List<GameObject> objects = new();
    public GameObject LandmarkPrefab;
    


    // Start is called before the first frame update
    void Start()
    {
        landmarkString = receiveLandmarks.receivedMessage;
        ParseMessage(landmarkString);
        AssignVectors(1000);
    }


    // Update is called once per frame
    void Update()
    {
        if(receiveLandmarks.hasReceivedMessage)
        {
            landmarkString = receiveLandmarks.receivedMessage;
            ParseMessage(landmarkString);
            AssignVectors(1000);
            receiveLandmarks.hasReceivedMessage = false;
        }

    }


    // Parse the received landmark string into landmark structs
    public void ParseMessage(string message)
    {
        landmarks.Clear();
        // Clean the message
        message = message.Replace(" ", string.Empty).Replace("],[", "|").Replace("[", string.Empty).Replace("]", string.Empty).Replace("),(", "|").Replace("(", string.Empty).Replace(")", string.Empty);
        if(message.EndsWith("|")){message = message.Remove(message.Length - 1); }
        if(message == string.Empty){/*Debug.Log("No data received");*/ return;}

        foreach(string parsedString in message.Split("|"))
        {
            // Parse the message into float values
            List<float> valueHolder = new();
            foreach(string value in parsedString.Split(",")){
                valueHolder.Add(float.Parse(value, CultureInfo.InvariantCulture.NumberFormat));
            }
            // Create and store landmarks in a list
            landmarks.Add(new landmark(valueHolder[0], valueHolder[1], valueHolder[2], valueHolder[3], valueHolder[4]));
        }
    }

    // Create and assign coordintes to objects
    // Takes an integer to scale the size of the coordinates
    public void AssignVectors(int coordinateScale){
        foreach(GameObject i in objects){
            Destroy(i);
        }
        objects.Clear();

        int numberOfObjects = objects.Count;
        int numberOfLandmarks = landmarks.Count;

        // Create objects if they are less than the amount of landmark structs
        if(numberOfObjects < numberOfLandmarks){
            for(int i = numberOfObjects; i < numberOfLandmarks; i++){
                GameObject newObject = Instantiate(LandmarkPrefab);
                objects.Add(newObject);
            }


        }
        // Remove extra objects if there are
        if(numberOfObjects > numberOfLandmarks){
            for(int i = numberOfObjects-1; i >= numberOfLandmarks; i--){
                Destroy(objects[i]);
                objects.RemoveAt(i);
            }
        }

        // Assign coordinates to objects
        for (int i = 0; i < numberOfLandmarks; i++)
        {
            float xOffset = (landmarks[i].handNo == 0) ? -0.1f*coordinateScale : 0.1f*coordinateScale;

            objects[i].transform.position = new Vector3(
                landmarks[i].landmarkX*coordinateScale + xOffset,
                -landmarks[i].landmarkY*coordinateScale,
                landmarks[i].landmarkZ*coordinateScale
            );
            
        }
    }
}
