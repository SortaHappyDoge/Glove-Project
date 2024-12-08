using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
//using System.Numerics;


public class ProjectLandmarks : MonoBehaviour
{
    // Takes all values as floats
    public struct Landmark
    {
        public float handNo;
        public float landmarkId;
        public float landmarkX;
        public float landmarkY;
        public float landmarkZ;
        public GameObject landmarkObject;

        // Function to assign variables
        public Landmark(float handNo, float landmarkId, float landmarkX, float landmarkY, float landmarkZ, GameObject landmarkObject)
        {
            this.handNo = handNo;
            this.landmarkId = landmarkId;
            this.landmarkX = landmarkX;
            this.landmarkY = landmarkY;
            this.landmarkZ = landmarkZ;
            this.landmarkObject = landmarkObject;
        }

        // Print stored info
        public override string ToString()
        {
            return $"Hand: {handNo}, LandmarkId: {landmarkId}, X: {landmarkX}, Y: {landmarkY}, Z: {landmarkZ}, LandmarkObject:{landmarkObject}";
        }
    }


    public bool isDrawLines = true;
    public ReceiveLandmarks receiveLandmarks;
    public GameObject LandmarkPrefab;
    private string landmarkString;
    private List<Landmark> handLandmarks = new();
    private List<Landmark> handPositions = new();
    private Dictionary<(int, int), LineRenderer> lines = new Dictionary<(int, int), LineRenderer>();

    // Define specific pairs of indexes to connect with lines
    private List<(int, int)> indexPairs = new()
    {
        // Landmarks from left hand
        (0, 1), (1, 2), (2, 3), (3, 4), (1, 5), (5, 6), 
        (6, 7), (7, 8), (5, 9), (9, 10), (10, 11), 
        (11, 12), (9, 13), (13, 14), (14, 15), (15, 16), 
        (0, 17), (13, 17), (17, 18), (18, 19), (19, 20),
        // Landmarks from right hand
        (21, 22), (22, 23), (23, 24), (24, 25), (22, 26), 
        (26, 27), (27, 28), (28, 29), (26, 30), (30, 31), 
        (31, 32), (32, 33), (30, 34), (34, 35), (35, 36), 
        (36, 37), (21, 38), (34, 38), (38, 39), (39, 40), (40, 41)
    };

    // Start is called before the first frame update
    void Start()
    {
        landmarkString = receiveLandmarks.receivedMessage;
    }


    // Update is called once per frame
    void Update()
    {
        if (receiveLandmarks.hasReceivedMessage)
        {
            landmarkString = receiveLandmarks.receivedMessage;
            ParseMessage(landmarkString);
            AssignVectors(1000);
            if (isDrawLines)
            {
                UpdateLines();
                RemoveInvalidLines();
            }
            receiveLandmarks.hasReceivedMessage = false;
        }
    }

    // Parse the received landmark string into landmark structs
    public void ParseMessage(string message)
    {
        // Clean the message
        message = message.Replace(" ", string.Empty).Replace("],[", "|").Replace("[", string.Empty).Replace("]", string.Empty)
        .Replace("),(", "|").Replace("(", string.Empty).Replace(")", string.Empty).Replace("||", "|");
        // Removes any extra characters left from .Replace("],[", "|") 
        if (message.EndsWith("|")) { message = message.Substring(0, message.Length - 1); }
        if (message.StartsWith("|")) { message = message.Substring(1); }

        // Delete everything when no data is received
        if (receiveLandmarks.hasReceivedMessage && string.IsNullOrEmpty(message)) 
        { 
            /*Debug.Log("No data received");*/ 
            foreach (var landmark in handLandmarks)
            {
            if (landmark.landmarkObject != null)
            {
                Destroy(landmark.landmarkObject);
            }
            }
            handLandmarks.Clear();
            return; 
        }

        string[] parsedMessage = message.Split("|");
        string[] handString = parsedMessage[..^2];
        string[] positionString = parsedMessage[^2..];
        Debug.Log(handString.Length);

        ParseToLandmark(handString, handLandmarks);
        ParseToLandmark(positionString, handPositions);

        // Remove extra objects
        if (handString.Length < handLandmarks.Count)
        {
            for (int i = handString.Length; i < handLandmarks.Count; i++)
            {
                Destroy(handLandmarks[i].landmarkObject);
            }
            handLandmarks.RemoveRange(handString.Length, handLandmarks.Count - handString.Length);
        }
    }

    // Assingns values in given string array to the specified landmark list  
    public void ParseToLandmark(string[] stringLandmarks, List<Landmark> landmarks)
    {
        // Parse the message into structs
        for (int i = 0; i < stringLandmarks.Length; i++)
        {

            string[] parsedLandmark = stringLandmarks[i].Split(",");
            // Create new landmarks if there arent enough doesnt exist
            if (i >= landmarks.Count)
            {
                landmarks.Add(new Landmark(
                    float.Parse(parsedLandmark[0], CultureInfo.InvariantCulture.NumberFormat), 
                    float.Parse(parsedLandmark[1], CultureInfo.InvariantCulture.NumberFormat), 
                    float.Parse(parsedLandmark[2], CultureInfo.InvariantCulture.NumberFormat), 
                    float.Parse(parsedLandmark[3], CultureInfo.InvariantCulture.NumberFormat), 
                    float.Parse(parsedLandmark[4], CultureInfo.InvariantCulture.NumberFormat), 
                    Instantiate(LandmarkPrefab)));
            }

            // Change the xyz of the landmark by replacing the struct with a new one
            else
            {
                Landmark landmark = landmarks[i]; 
                landmark.landmarkX = float.Parse(parsedLandmark[2], CultureInfo.InvariantCulture.NumberFormat);
                landmark.landmarkY = float.Parse(parsedLandmark[3], CultureInfo.InvariantCulture.NumberFormat);
                landmark.landmarkZ = float.Parse(parsedLandmark[4], CultureInfo.InvariantCulture.NumberFormat);
                landmarks[i] = landmark;
            }
        }
    }

    // Create and assign coordintes to objects
    // Takes an integer to scale the size of the coordinates
    public void AssignVectors(int coordinateScale)
    {
        Vector3 leftHand = new Vector3(
            handPositions[0].landmarkX * coordinateScale,
            -handPositions[0].landmarkY * coordinateScale,
            handPositions[0].landmarkZ * coordinateScale);
        Vector3 rightHand = new Vector3(
            handPositions[1].landmarkX * coordinateScale,
            -handPositions[1].landmarkY * coordinateScale,
            handPositions[1].landmarkZ * coordinateScale);

        // Assign coordinates to objects
        for (int i = 0; i < handLandmarks.Count; i++)
        {
            if (handLandmarks[i].handNo == 0)
            {
                handLandmarks[i].landmarkObject.transform.position = new Vector3(
                    handLandmarks[i].landmarkX * coordinateScale,
                    -handLandmarks[i].landmarkY * coordinateScale,
                    handLandmarks[i].landmarkZ * coordinateScale)
                    + leftHand;
            }
            if (handLandmarks[i].handNo == 1)
            {
                handLandmarks[i].landmarkObject.transform.position = new Vector3(
                    handLandmarks[i].landmarkX * coordinateScale,
                    -handLandmarks[i].landmarkY * coordinateScale,
                    handLandmarks[i].landmarkZ * coordinateScale)
                    + rightHand;
            }
        }
    }

    // Draws lines between landmarks to distinguish them
    void UpdateLines()
    {
        foreach (var pair in indexPairs)
        {
            int indexA = pair.Item1;
            int indexB = pair.Item2;

            // Ensure both indexes are within the bounds of the objects list
            if (indexA < handLandmarks.Count && indexB < handLandmarks.Count)
            {
                GameObject objA = handLandmarks[indexA].landmarkObject;
                GameObject objB = handLandmarks[indexB].landmarkObject;

                if (objA != null && objB != null)
                {
                    // Create a new line if it doesn’t exist for this pair
                    if (!lines.ContainsKey(pair))
                    {
                        LineRenderer line = CreateLineRenderer();
                        lines[pair] = line;
                    }

                    // Update line position
                    lines[pair].SetPosition(0, objA.transform.position);
                    lines[pair].SetPosition(1, objB.transform.position);
                }
            }
        }
    }

    void RemoveInvalidLines()
    {
        // Find and remove lines where one or both objects are null or out of range
        var keysToRemove = new List<(int, int)>();

        foreach (var pair in lines)
        {
            int indexA = pair.Key.Item1;
            int indexB = pair.Key.Item2;

            // Check if objects are null or indexes are out of bound
            if (indexA >= handLandmarks.Count || indexB >= handLandmarks.Count ||
                handLandmarks[indexA].landmarkObject == null || handLandmarks[indexB].landmarkObject == null)
            {
                Destroy(pair.Value.gameObject);  // Destroy LineRenderer
                keysToRemove.Add(pair.Key);
            }
        }

        // Remove invalid keys from dictionary
        foreach (var key in keysToRemove)
        {
            lines.Remove(key);
        }
    }

    LineRenderer CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        // Customize the line appearance here
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        return lineRenderer;
    }
}

