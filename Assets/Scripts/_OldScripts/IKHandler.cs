using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Robotics;
using UrdfControlRobot = Unity.Robotics.UrdfImporter.Control;

public class IKHandler : MonoBehaviour
{
    // Public Variables
    [Header("Robot Properties")]

    [Tooltip("Total number of joints for IK")]
    public int chainLength = 0;

    [Tooltip("Target the robot should go to")]
    public Transform _target;


    // Private variables
    private ArticulationBody[] joints;
    private RobotJoint[] jointInfo;
    private List<float> dofAngles = new List<float>();
    // private float[] _angles;
    private List<float> _angles = new List<float>();
    private float[] _zangles;
    private List<int> indexes = new List<int>();
    private Vector3 endEffPos;
    private Transform root;
    private FKScript fkScript;

    // Constants
    private const float SamplingDistance = 0.0001f;
    private const float LearningRate = 200f;
    private const float DistanceThreshold = 0.01f;

    // Initialize the variables
    void Start() {
        // Initialize the root
        root = this.transform;

        // Get the FK Handler script
        fkScript = root.gameObject.GetComponent<FKScript>();

        // Initialize the articulation chain
        var entireChain = root.gameObject.GetComponentsInChildren<ArticulationBody>();
        joints = entireChain[1..chainLength];
        foreach (var joint in joints) {
            Debug.Log(joint.transform.name);
        }
        joints[0].GetDofStartIndices(indexes);

        // Initialize the variables
        //_angles = new float[joints.Length];
        _zangles = new float[joints.Length];
        jointInfo = new RobotJoint[joints.Length];

        // Get the current angles of each joint
        updateAngles();

        // Go to a starting position
        RotateTo(0f,joints[0]);
        _angles[0] = 0;
        RotateTo(45f,joints[1]);
        _angles[1] = 45;
        RotateTo(-45f,joints[2]);
        _angles[2] = -45;
        RotateTo(0f,joints[3]);
        _angles[3] = 0;
        RotateTo(0f,joints[4]);
        _angles[4] = 0;
        RotateTo(-90f,joints[5]);
        _angles[5] = -90;
    }

    // Update is called once per frame
    void FixedUpdate() {
        // Debug.Log(_target.position);
        // Debug.Log(joints[joints.Length - 1].transform.position);
        // Debug.Log(ForwardKinematics2(_angles));
        // var newAngles = new List<float>();
        // foreach (var angle in _angles) {
        //     newAngles.Add(angle * Mathf.Deg2Rad);
        // }
        // Debug.Log(fkScript.FK(newAngles));
        //Debug.Log(DistanceFromTarget(_target.position, _angles));
        // updateAngles();
        InverseKinematics(_target.position, _target.rotation.eulerAngles, _angles);

        for (int i = 0; i < joints.Length; i++) {
            RotateTo(_angles[i],joints[i]);
        }
    }

    private void updateAngles() {
        for (int i = 0; i < joints.Length; i++) {
            // Get the joint info
            jointInfo[i] = joints[i].transform.gameObject.GetComponent<RobotJoint>();
            
            if (jointInfo[i].Axis ==  new Vector3(1,0,0)) {        // X
                //_angles[i] = joints[i].transform.localRotation.eulerAngles.x * Mathf.Deg2Rad;
                _angles.Add(joints[i].transform.localRotation.eulerAngles.x);
            } else if (jointInfo[i].Axis == new Vector3(0,1,0)) { // Y
                //_angles[i] = joints[i].transform.localRotation.eulerAngles.y * Mathf.Deg2Rad;
                _angles.Add(joints[i].transform.localRotation.eulerAngles.y);
            } else if (jointInfo[i].Axis == new Vector3(0,0,1)) { // Z
                //_angles[i] = joints[i].transform.localRotation.eulerAngles.z * Mathf.Deg2Rad;
                _angles.Add(joints[i].transform.localRotation.eulerAngles.z);
            }

            _zangles[i] = joints[i].transform.localRotation.eulerAngles.z;
        }
    }

    public Vector3 ForwardKinematics (List<float> angles) {
        // Start at Joint 0
        Vector3 prevPoint = joints[0].transform.position;

        // Get the rotation identity
        Quaternion rotation = Quaternion.identity;// joints[0].transform.rotation;

        // Iterate through each joint
        for (int i = 1; i < joints.Length; i++) {
            rotation *= Quaternion.AngleAxis(angles[i-1], jointInfo[i-1].Axis) * Quaternion.AngleAxis(_zangles[i-1], new Vector3(0,0,1));
            Vector3 nextPoint = prevPoint + rotation * jointInfo[i].StartOffset; //joints[i].transform.localPosition;
            
            prevPoint = nextPoint;
        }
        return prevPoint;
    }

    public Vector3 ForwardKinematics2 (List<float> angles) {
        var radAngles = new List<float>();
        foreach (var angle in angles) {
            radAngles.Add(angle * Mathf.Deg2Rad);
        }

        // Get all of the homogeneous tranformation matrices
        Matrix4x4 T_B1 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[0]), -Mathf.Sin(radAngles[0]), 0f, 0f), 
                                        new Vector4(-Mathf.Sin(radAngles[0]), -Mathf.Cos(radAngles[0]), 0f, 0f), 
                                        new Vector4(0f, 0f, -1f, 0.1564f), 
                                        new Vector4(0f, 0f, 0f, 1f));
        Matrix4x4 T_12 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[1]), -Mathf.Sin(radAngles[1]), 0f, 0f), 
                                        new Vector4(0f, 0f, -1f, 0.005375f),
                                        new Vector4(Mathf.Sin(radAngles[1]), Mathf.Cos(radAngles[1]), 0f, -0.12838f),
                                        new Vector4(0f, 0f, 0f, 1f));
        Matrix4x4 T_23 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[2]), -Mathf.Sin(radAngles[2]), 0f, 0f), 
                                        new Vector4(-Mathf.Sin(radAngles[2]), -Mathf.Cos(radAngles[2]), 0f, -0.410f), 
                                        new Vector4(0f, 0f, -1f, 0f), 
                                        new Vector4(0f, 0f, 0f, 1f));
        Matrix4x4 T_34 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[3]), -Mathf.Sin(radAngles[3]), 0f, 0f), 
                                        new Vector4(0f, 0f, -1f, 0.2084301f),
                                        new Vector4(Mathf.Sin(radAngles[3]), Mathf.Cos(radAngles[3]), 0f, -0.006374974f),
                                        new Vector4(0f, 0f, 0f, 1f));
        Matrix4x4 T_45 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[4]), -Mathf.Sin(radAngles[4]), 0f, 0f), 
                                        new Vector4(0f, 0f, 1f, 0f),
                                        new Vector4(-Mathf.Sin(radAngles[4]), -Mathf.Cos(radAngles[4]), 0f, -0.10593f),
                                        new Vector4(0f, 0f, 0f, 1f));
        Matrix4x4 T_56 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[5]), -Mathf.Sin(radAngles[5]), 0f, 0f), 
                                        new Vector4(0f, 0f, -1f, 0.10593f),
                                        new Vector4(Mathf.Sin(radAngles[5]), Mathf.Cos(radAngles[5]), 0f, 0f),
                                        new Vector4(0f, 0f, 0f, 1f));

        // Get the tranformation matrix from B -> 6 (base to bracelet link)
        Matrix4x4 T_B6 = T_B1.transpose * T_12.transpose * T_23.transpose * T_34.transpose * T_45.transpose * T_56.transpose;

        // Get the value of the point relative to frame 6
        Matrix4x4 p6 = new Matrix4x4(new Vector4(0f,0f,0f,0f), new Vector4(0f,0f,0f,0f), new Vector4(0f,0f,0f,-0.2f), new Vector4(0f,0f,0f,1f));

        // Get the value of the point relative to base frame
        p6 = T_B6 * p6.transpose;

        return new Vector3(-p6[1,3],p6[2,3],p6[0,3]);
    }

    public float DistanceFromTarget(Vector3 target, List<float> angles) {
        // var newAngles = new List<float>();
        // foreach (var angle in angles) {
        //     newAngles.Add(angle * Mathf.Deg2Rad);
        // }
        Vector3 point = ForwardKinematics2(angles); // fkScript.FK(newAngles); // ForwardKinematics(angles);
        // Debug.Log(target);
        // Debug.Log(point);

        return Vector3.Distance(point, target);
    }

    public float PartialGradient (Vector3 target, List<float> angles, int i) {
        // Saves the angle, it will be restored later
        float angle = angles[i];

        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        float f_x = DistanceFromTarget(target, angles);
        angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(target, angles);
        float gradient = (f_x_plus_d - f_x) / SamplingDistance;

        // Restores the angle
        angles[i] = angle;
        return gradient;
    }

    public void InverseKinematics (Vector3 target, Vector3 targetRot, List<float> angles) {
        // Get the adjusted target rotation so the range is from [-180,180]
        targetRot.x = (targetRot.x < 180f) ? targetRot.x : targetRot.x - 360;
        targetRot.y = (targetRot.y < 180f) ? targetRot.y : targetRot.y - 360;
        targetRot.z = (targetRot.z < 180f) ? targetRot.z : targetRot.z - 360;

        //------------------------------ Keep orientation with end effector ------------------------------//
        angles[3] = -90;                                           // Spherical Wrist 1
        angles[4] = -angles[0] + targetRot.y + targetRot.x;                      // Spherical Wrist 2

        // if (angles[0] < -25) {
        //     angles[3] = angles[0] - (angles[1] - angles[2]) + 90 + targetRot.y;
        //     angles[4] = -angles[0] + (angles[1] - angles[2]) - 90 - targetRot.x;
        //     angles[5] = (angles[2] - angles[1]) / 2 - angles[0] - 135 + targetRot.z; // -angles[0] + 180 + (angles[2] / 2) + 45;
        // } else if (angles[0] > 25) {
        //     angles[3] = angles[0] + (angles[1] - angles[2]) - 90 + targetRot.y;
        //     angles[4] = angles[0] + (angles[1] - angles[2]) - 90 - targetRot.x;
        //     angles[5] = (-1 *(angles[2] - angles[1] + 90) / 2) - 90 - angles[0] - 90 + targetRot.z;
        // } else {
        //     angles[3] = targetRot.y + angles[0] * 2;
        //     angles[4] = angles[0] + (angles[1] - angles[2]) - 90 - targetRot.x;
        //     angles[5] = (-1 *(angles[2] - angles[1] + 90) / 2) - 90 - angles[0] - 90 + targetRot.z;
        // }



        // angles[3] = angles[0];                                           // Spherical Wrist 1
        // angles[4] = angles[0];
        // angles[5] = -angles[0];
        // angles[4] = -angles[0] + targetRot.y + targetRot.x;                      // Sperical Wrist 2

        // The bracelet link has to compensate with the bicep and forearm link to be parallel to the ground
        // The method used depends on  where the shoulder link rotation is
        if (angles[0] < 0) { angles[5] = angles[2] - angles[1] + targetRot.z; }  // Bracelet Link
        else if (angles[0] > 0) { angles[5] = (-1 *(angles[2] - angles[1] + 90)) - 90 + targetRot.z; }
        else { angles[5] = -90 + targetRot.z; }


        //--------------------------------- Start of Inverse Kinematics ----------------------------------//
        // If the target is within the threshold, return
        if (DistanceFromTarget(target, angles) < DistanceThreshold) { return; }

        for (int i = joints.Length - 4; i >= 0; i --) {
            // Gradient descent
            // Update : Solution -= LearningRate * Gradient
            float gradient = PartialGradient(target, angles, i);
            angles[i] -= LearningRate * gradient;

            // Clamp
            if (joints[i].linearLockX == ArticulationDofLock.LimitedMotion || joints[i].linearLockY == ArticulationDofLock.LimitedMotion) {
                angles[i] = Mathf.Clamp(angles[i], joints[i].xDrive.lowerLimit, joints[i].xDrive.upperLimit);
            }

            // Early termination
            if (DistanceFromTarget(target, angles) < DistanceThreshold) { return; }

            // Debug.Log(DistanceFromTarget(target));
        }
    }

    void RotateTo(float angle, ArticulationBody joint) {
        // angle = angle * Mathf.Rad2Deg;
        //var currentAngle = (float) joint.jointPosition[0] * Mathf.Rad2Deg;
        var drive = joint.xDrive;
        drive.target = angle;
        joint.xDrive = drive;

        //  // force position
        // float rotationRads = Mathf.Deg2Rad * angle;
        // ArticulationReducedSpace newPosition = new ArticulationReducedSpace(rotationRads);
        // joint.jointPosition = newPosition;
        // // force velocity to zero
        // ArticulationReducedSpace newVelocity = new ArticulationReducedSpace(0.0f);
        // joint.jointVelocity = newVelocity;
    }
}
