using UnityEngine;
using System.Collections.Generic;
using System;
public class FKScript : MonoBehaviour {
    // Public Variables
    public List<float> currentAngles;   // Shown for debugging purposes
    public Vector3 endEffectorPos;      // Will be in Unity's left hand coordinate system
    public Vector3 localEndEffectorPos = new Vector3(0f, 0f, -0.15f); // Has to be in the right-hand coordinate system
    public int numJoints = 6;           // Number of active joints
    public const string k_TagName = "robot";
    public List<ArticulationBody> jointChain;
    private List<float[]> dh;

    // Private Variables
    private GameObject robot;
    private static int prismaticJointVariable = 2;
    private static int revoluteJointVariable = 3;
    private Quaternion endEffectorRotation;
    public Matrix4x4 endEffectorPositionMatrix;
    

    // Start method is called once
    void Start() {
        // If DH does not exist, create a new list
        if (dh == null) { dh = new List<float[]>(); }
        
        // Create the chain of joints
        jointChain = new List<ArticulationBody>();

        // Find the main robot
        robot = FindRobotObject();
        
        // If the robot is not found, then return
        if (!robot) { return; }

        // Add the DH Parameters for your specific robot (alpha = 0, a = 1, d = 2, theta = 3)
        // dh.Add(new float[]{(float)Mathf.PI, 0.1564f, 0f, 0f});           // i = 1
        // dh.Add(new float[]{(float)-Mathf.PI/2, 0.1284f, 0.00538f, 0f});  // i = 2
        // dh.Add(new float[]{(float)Mathf.PI, 0.410f, 0f, 0f});            // i = 3
        // dh.Add(new float[]{(float)-Mathf.PI/2, 0.2084f, 0.00638f, 0f});  // i = 4
        // dh.Add(new float[]{(float)-Mathf.PI/2, 0.1059f, 0f, 0f});        // i = 5
        // dh.Add(new float[]{(float)-Mathf.PI/2, 0.1059f, 0f, 0f});        // i = 6

        // dh.Add(new float[]{0, 0f, 0.15643f, 0f});           // i = 1
        // dh.Add(new float[]{(float)-Mathf.PI/2, 0f, -0.005375f, 0f});  // i = 2
        // dh.Add(new float[]{(float)-Mathf.PI, 0.53838f, 0f, 0f});            // i = 3
        // dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.2084301f, 0f});  // i = 4
        // dh.Add(new float[]{(float)-Mathf.PI/2, 0f, -0.006374974f, 0f});        // i = 5
        // dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.21186f, 0f});        // i = 6

        dh.Add(new float[]{0, 0f, 0.28481f, 0f});           // i = 1
        dh.Add(new float[]{(float)-Mathf.PI/2, 0f, -0.005375f, 0f});  // i = 2
        dh.Add(new float[]{(float)Mathf.PI, 0.410f, -0.006374974f, 0f});            // i = 3
        dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.3142601f, 0f});  // i = 4
        dh.Add(new float[]{(float)-Mathf.PI/2, 0f, 0f, 0f});        // i = 5
        dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.10593f, 0f});        // i = 6

        // dh.Add(new float[]{(float)Mathf.PI, 0f, 0f, 0f});           // i = 1
        // dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.28481f, 0f});           // i = 1
        // dh.Add(new float[]{(float)Mathf.PI, 0.410f, -0.00538f, 0f});  // i = 2
        // dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.00638f, 0f});            // i = 3
        // dh.Add(new float[]{(float)Mathf.PI/2, 0f, -0.31436f, 0f});  // i = 4
        // dh.Add(new float[]{(float)Mathf.PI/2, 0f, 0f, 0f});        // i = 5
        // dh.Add(new float[]{(float)Mathf.PI, 0f, -0.16746f, 0f});        // i = 6

        // Add any joints to the joint chain if its not a fixed joint
        foreach (ArticulationBody joint in robot.GetComponentsInChildren<ArticulationBody>()) {
            if (joint.jointType != ArticulationJointType.FixedJoint) { jointChain.Add(joint); }
            //jointChain.Add(joint);
        }

        // Remove the unnecessary joints
        jointChain.RemoveRange(numJoints,jointChain.Count - numJoints);
    }

    // Find the robot object
    public static GameObject FindRobotObject() {
        try {
            GameObject robot = GameObject.FindWithTag(k_TagName);
            if (robot == null) {
                Debug.LogWarning($"No GameObject with tag '{k_TagName}' was found.");
            }
            return robot;
        }
        catch (Exception) {
            Debug.LogError($"Unable to find tag '{k_TagName}'. " + 
                            $"Add A tag '{k_TagName}' in the Project Settings in Unity Editor.");
        }
        return null;
    }

    // FixedUpdate gets called every second
    void FixedUpdate() {
        if (dh.Count == jointChain.Count) { FK(); }
    }

    /// <summary>
    /// Returns a Vector3 containing the end effector position in Unity's coordinate system
    /// </summary>
    /// <param name="angles">List of float numbers representing joint poistions of a robot in meters or radians.</param>
    /// <returns>Vector3 EndEffectorPos</returns>
    public Vector3 FK(List<float> angles = null) {
        // If the provided angles are null, get the current angles and popular the DH table
        if (angles == null) {
            currentAngles = currentJointParameters();
            PopulateDHparameters(currentAngles); 
        } else { 
            PopulateDHparameters(angles); 
        }

        // Create an identity matrix for the end effector matrix
        endEffectorPositionMatrix = Matrix4x4.identity;

        // Iterate through each joint to get the transformation matrix and multiply to get T from 0 to n
        for (int i = 0; i < dh.Count; i++) {
            endEffectorPositionMatrix = endEffectorPositionMatrix * (FWDMatrix(dh[i]).transpose);
        }

        // Calculate the end effector position in the right hand coordinate system
        endEffectorPositionMatrix = endEffectorPositionMatrix * new Matrix4x4(new Vector4(0f,0f,0f,0f), new Vector4(0f,0f,0f,0f), new Vector4(0f,0f,0f,0f), new Vector4(localEndEffectorPos.x, localEndEffectorPos.y, localEndEffectorPos.z,1f));
    
        // Convert it to the left-hand coordinate system (y-up - z-up)[x = x, y = z, z = -y]
        // endEffectorPos = new Vector3(-endEffectorPositionMatrix[1,3], endEffectorPositionMatrix[0,3], endEffectorPositionMatrix[2,3]);
        endEffectorPos = new Vector3(endEffectorPositionMatrix[1,3], endEffectorPositionMatrix[2,3], endEffectorPositionMatrix[0,3]);
        return endEffectorPos;
    }

    /// <summary>
    /// Modifies the DH parameters with the current joint positions from the robot in Unity Simulation
    /// JointPostition: 0-2: rotation along XYZ axis 3-5: Translation along XYZ co-ordinates
    /// https://docs.unity3d.com/2020.1/Documentation/ScriptReference/ArticulationBody-jointPosition.html
    /// </summary>
    /// <param name="angles">List of float numbers representing joint poistions of a robot in meters or radians</param>
    public void PopulateDHparameters(List<float> angles) {
        // This function may have to be changed if your angles have +- any radians
        // for (int i = 0; i < jointChain.Count; i++) {
        //     if (jointChain[i].jointType == ArticulationJointType.RevoluteJoint) {
        //         dh[i][revoluteJointVariable] = jointChain[i].jointPosition[0];
        //     }
        //     else if (jointChain[i].jointType == ArticulationJointType.PrismaticJoint) {
        //         dh[i][prismaticJointVariable] = jointChain[i].jointPosition[3];
        //     }
        //     else {
        //         Debug.LogError("Other joint types not supported");
        //     }
        // }
        for (int i = 0; i < jointChain.Count; i++) {
            if (jointChain[i].jointType == ArticulationJointType.RevoluteJoint) {
                if (i == 1 || i == 2) { dh[i][revoluteJointVariable] = angles[i] - (float)Mathf.PI/2; }
                else { dh[i][revoluteJointVariable] = angles[i]; }
                // if (i == 2 || i == 3) { dh[i][revoluteJointVariable] = angles[i - 1] - Mathf.PI/2; }
                // else if (i == 4 || i == 5 || i == 6) { dh[i][revoluteJointVariable] = angles[i - 1] + Mathf.PI;}
                // else { dh[i][revoluteJointVariable] = angles[i]; }
            }
            else if (jointChain[i].jointType == ArticulationJointType.PrismaticJoint) {
                dh[i][prismaticJointVariable] = angles[i];
            }
            else {
                Debug.LogError("Other joint types not supported");
            }
        }


    }

    // Calculates the current joint parameters
    public List<float> currentJointParameters() {
        List<float> angles = new List<float>();
        for (int i = 0; i < jointChain.Count; i++) {
            angles.Add((float)Math.Round(jointChain[i].jointPosition[0],2));
        }
        return angles;
    }

    /// <summary>
    /// Returns a homogenous transformatino matrix formed using a set of DH paramters of a joint in new DH convention.
    /// https://en.wikipedia.org/wiki/Denavit–Hartenberg_parameters
    /// </summary>
    /// <param name="DHparameters">Array of four float parameters (alpha = 0, a = 1, d = 2, theta = 3)</param>
    /// <returns>Transformation matrix, T, from i-1 to i</returns>
    private Matrix4x4 FWDMatrix(float[] DHparameters) {
        return new Matrix4x4(new Vector4(Mathf.Cos(DHparameters[3]), -Mathf.Sin(DHparameters[3]), 0, DHparameters[1]),
                            new Vector4(Mathf.Sin(DHparameters[3]) * Mathf.Cos(DHparameters[0]), Mathf.Cos(DHparameters[3]) * Mathf.Cos(DHparameters[0]), -Mathf.Sin(DHparameters[0]), Mathf.Sin(DHparameters[0]) * DHparameters[2] * -1f),
                            new Vector4(Mathf.Sin(DHparameters[3]) * Mathf.Sin(DHparameters[0]), Mathf.Cos(DHparameters[3]) * Mathf.Sin(DHparameters[0]), Mathf.Cos(DHparameters[0]), Mathf.Cos(DHparameters[0]) * DHparameters[2]),
                            new Vector4(0, 0, 0, 1));
    }

    /// <summary>
    /// Returns a homogenous transformatino matrix formed using a set of DH paramters of a joint in new DH convention.
    /// https://en.wikipedia.org/wiki/Denavit–Hartenberg_parameters
    /// </summary>
    /// <param name="DHparameters">Array of four float parameters</param>
    /// <returns></returns>
    private Matrix4x4 FWDMatrix2(float[] DHparameters) 
    {
        return new Matrix4x4(new Vector4(Mathf.Cos(DHparameters[3]), -Mathf.Sin(DHparameters[3]) * Mathf.Cos(DHparameters[0]), Mathf.Sin(DHparameters[3]) * Mathf.Sin(DHparameters[0]), DHparameters[1] * Mathf.Cos(DHparameters[3])),
                            new Vector4(Mathf.Sin(DHparameters[3]) , Mathf.Cos(DHparameters[3]) * Mathf.Cos(DHparameters[0]), -Mathf.Sin(DHparameters[0]) * Mathf.Cos(DHparameters[3]), Mathf.Sin(DHparameters[3]) * DHparameters[1]),
                            new Vector4(0, Mathf.Sin(DHparameters[0]), Mathf.Cos(DHparameters[0]), DHparameters[2]),
                            new Vector4(0, 0, 0, 1));
    }

}