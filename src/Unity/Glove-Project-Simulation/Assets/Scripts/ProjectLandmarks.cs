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

        // Function to assign variables
        public Landmark(float handNo, float landmarkId, float landmarkX, float landmarkY, float landmarkZ)
        {
            this.handNo = handNo;
            this.landmarkId = landmarkId;
            this.landmarkX = landmarkX;
            this.landmarkY = landmarkY;
            this.landmarkZ = landmarkZ;
        }

        // Print stored info
        public override string ToString()
        {
            return $"Hand: {handNo}, LandmarkId: {landmarkId}, X: {landmarkX}, Y: {landmarkY}, Z: {landmarkZ}";
        }
    }


    public ReceiveLandmarks receiveLandmarks;
    public GameObject LandmarkPrefab;
    private string landmarkString;
    private List<Landmark> landmarks = new();
    private List<GameObject> objects = new();
    private Dictionary<(int, int), LineRenderer> lines = new Dictionary<(int, int), LineRenderer>();

    // Define specific pairs of indexes to connect with lines
    private List<(int, int)> indexPairs = new()
    {
        (0, 1), (1, 2), (2, 3), (3, 4), (1, 5), (5, 6), (6, 7), (7, 8), (5, 9), (9, 10), (10, 11), (11, 12), (9, 13), (13, 14), (14, 15), (15, 16), (0, 17), (13, 17), (17, 18), (18, 19), (19, 20),
        (21, 22), (22, 23), (23, 24), (24, 25), (22, 26), (26, 27), (27, 28), (28, 29), (26, 30), (30, 31), (31, 32), (32, 33), (30, 34), (34, 35), (35, 36), (36, 37), (21, 38), (34, 38), (38, 39), (39, 40), (40, 41)
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
            UpdateLines();
            RemoveInvalidLines();
            receiveLandmarks.hasReceivedMessage = false;
        }
    }

    // Parse the received landmark string into landmark structs
    public void ParseMessage(string message)
    {
        landmarks.Clear();
        // Clean the message
        message = message.Replace(" ", string.Empty).Replace("],[", "|").Replace("[", string.Empty).Replace("]", string.Empty).Replace("),(", "|").Replace("(", string.Empty).Replace(")", string.Empty);
        if (message.EndsWith("|")) { message = message.Remove(message.Length - 1); }
        if (message == string.Empty) {/*Debug.Log("No data received");*/ return; }

        foreach (string parsedString in message.Split("|"))
        {
            // Parse the message into float values
            List<float> valueHolder = new();
            foreach (string value in parsedString.Split(","))
            {
                valueHolder.Add(float.Parse(value, CultureInfo.InvariantCulture.NumberFormat));
            }
            // Create and store landmarks in a list
            landmarks.Add(new Landmark(valueHolder[0], valueHolder[1], valueHolder[2], valueHolder[3], valueHolder[4]));
        }
    }

    // Create and assign coordintes to objects
    // Takes an integer to scale the size of the coordinates
    public void AssignVectors(int coordinateScale)
    {
        // Create objects if they are less than the amount of landmark structs
        if (objects.Count < landmarks.Count)
        {
            for (int i = objects.Count; i < landmarks.Count; i++)
            {
                GameObject newObject = Instantiate(LandmarkPrefab);
                objects.Add(newObject);
            }
        }

        // Remove extra objects if there are
        if (objects.Count > landmarks.Count)
        {
            for (int i = objects.Count - 1; i >= landmarks.Count; i--)
            {
                Destroy(objects[i]);
                objects.RemoveAt(i);
            }
        }

        // Assign coordinates to objects
        for (int i = 0; i < landmarks.Count; i++)
        {
            float xOffset = (landmarks[i].handNo == 0) ? 0.1f * coordinateScale : -0.1f * coordinateScale;

            objects[i].transform.position = new Vector3(
                landmarks[i].landmarkX * coordinateScale + xOffset,
                -landmarks[i].landmarkY * coordinateScale,
                landmarks[i].landmarkZ * coordinateScale
            );
        }
    }

    void UpdateLines()
    {
        foreach (var pair in indexPairs)
        {
            int indexA = pair.Item1;
            int indexB = pair.Item2;

            // Ensure both indexes are within the bounds of the objects list
            if (indexA < objects.Count && indexB < objects.Count)
            {
                GameObject objA = objects[indexA];
                GameObject objB = objects[indexB];

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
            if (indexA >= objects.Count || indexB >= objects.Count ||
                objects[indexA] == null || objects[indexB] == null)
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

