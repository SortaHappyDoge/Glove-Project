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

public class ProjectLandmarks : MonoBehaviour
{
    // Takes all values as floats converts handNo and landmarkId into integer
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
    private List<Landmark> landmarks = new();
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
        message = message.Replace(" ", string.Empty).Replace("],[", "|").Replace("[", string.Empty).Replace("]", string.Empty).Replace("),(", "|").Replace("(", string.Empty).Replace(")", string.Empty);
        // Removes any extra characters left from .Replace("],[", "|") 
        if (message.EndsWith("|")) { message = message.Substring(0, message.Length - 1); }
        if (message.StartsWith("|")) { message = message.Substring(1); }

        if (receiveLandmarks.hasReceivedMessage && string.IsNullOrEmpty(message)) 
        { 
            /*Debug.Log("No data received");*/ 
            foreach (var landmark in landmarks)
            {
            if (landmark.landmarkObject != null)
            {
                Destroy(landmark.landmarkObject);
            }
            }
            landmarks.Clear();
            return; 
        }

        // Parse the message into structs
        string[] stringLandmarks = message.Split("|");

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

            // Change the xyz of the landmark
            else
            {
                Landmark landmark = landmarks[i]; 
                landmark.landmarkX = float.Parse(parsedLandmark[2], CultureInfo.InvariantCulture.NumberFormat);
                landmark.landmarkY = float.Parse(parsedLandmark[3], CultureInfo.InvariantCulture.NumberFormat);
                landmark.landmarkZ = float.Parse(parsedLandmark[4], CultureInfo.InvariantCulture.NumberFormat);
                landmarks[i] = landmark;
            }
        } 
        // Remove extra objects
        if (stringLandmarks.Length < landmarks.Count)
        {
            for (int i = stringLandmarks.Length; i < landmarks.Count; i++)
            {
                Destroy(landmarks[i].landmarkObject);
            }
            landmarks.RemoveRange(stringLandmarks.Length, landmarks.Count - stringLandmarks.Length);
        }
    }

    // Create and assign coordintes to objects
    // Takes an integer to scale the size of the coordinates
    public void AssignVectors(int coordinateScale)
    {
        // Assign coordinates to objects
        for (int i = 0; i < landmarks.Count; i++)
        {
            float xOffset = (landmarks[i].handNo == 0) ? 0.1f * coordinateScale : -0.1f * coordinateScale;

            landmarks[i].landmarkObject.transform.position = new Vector3(
                landmarks[i].landmarkX * coordinateScale + xOffset,
                -landmarks[i].landmarkY * coordinateScale,
                landmarks[i].landmarkZ * coordinateScale
            );
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
            if (indexA < landmarks.Count && indexB < landmarks.Count)
            {
                GameObject objA = landmarks[indexA].landmarkObject;
                GameObject objB = landmarks[indexB].landmarkObject;

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
            if (indexA >= landmarks.Count || indexB >= landmarks.Count ||
                landmarks[indexA].landmarkObject == null || landmarks[indexB].landmarkObject == null)
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

