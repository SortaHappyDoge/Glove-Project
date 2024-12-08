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
    public bool isDrawLines = true;
    public SocketRecieverNEW socketReciever;
    public GameObject LandmarkPrefab;
    private float[] receivedData;
    private List<float[]> handLandmarks = new();
    private List<float[]> handPositions = new();
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
        receivedData = socketReciever.receivedData;
    }


    // Update is called once per frame
    void Update()
    {
        if (socketReciever.isReceivedMessage)
        {
            receivedData = socketReciever.receivedData;
            List<float[]> landmarks = FormatMessage(receivedData);
            /*AssignVectors(1000);
            if (isDrawLines)
            {
                UpdateLines();
                RemoveInvalidLines();
            }*/
            socketReciever.isReceivedMessage = false;
        }
    }

    // Parse the received landmark string into landmark structs
    public List<float[]> FormatMessage(float[] message)
    {
        int counter = 0;
        List<float[]> landmarks = new();
        float[] landmark = new float[5]; 
        foreach (float value in message)
        {
            landmark[counter++] = value;
            if (counter == 5)
            {
                landmarks.Add(landmark);
                counter = 0;
            }
        }
        return landmarks;
    }
/*
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
    private void UpdateLines()
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

    private void RemoveInvalidLines()
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

    private LineRenderer CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        // Customize the line appearance here
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        return lineRenderer;
    }*/
}