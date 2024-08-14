/// |----------------------------------------Gripper Controller---------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the gripper for use by other classes. It allows for control by percentage or
///              to close until it hits a target object.
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class GripperController : MonoBehaviour {
    // Accessible Properties
    [SerializeField] float deltaAngle = 1f; // The amount of angle it changes by in an update
    
    // Private Variables
    private Transform robot;
    private ArticulationBody[] articulationChain;
    private ArticulationBody[] gripperChain;
    private ArticulationBody[] leftChain;
    private ArticulationBody[] rightChain;
    private float[] gripperAngles;
    private float lPerc = 0f;      // Left Percent
    private float rPerc = 0f;      // Right Percent
    private bool gripperRightStopped = false;
    private bool gripperLeftStopped = false;
    


    // Called on first startup
    void Awake() {
        // Get the robot object and find the articulation chain
        robot = GameObject.FindWithTag("robot").transform;
        articulationChain = robot.GetComponentsInChildren<ArticulationBody>();

        // Get the joints for the gripper (Right + Left Inner/Outer Knuckles + Inner Fingers)
        gripperChain = articulationChain[9..11].Concat(articulationChain[12..13].Concat(articulationChain[14..16].Concat(articulationChain[17..18]))).ToArray();

        // Get the Joint Controls for left and right
        leftChain = gripperChain[0..3];
        rightChain = gripperChain[3..6];

        // Set up the float array
        gripperAngles = new float[gripperChain.Length];
    }
    
    // Called at a specific interval
    void FixedUpdate () {
        // Get the left angles
        gripperAngles[0] = Mathf.MoveTowards(leftChain[0].xDrive.target, lPerc * leftChain[0].xDrive.upperLimit, deltaAngle);
        gripperAngles[1] = Mathf.MoveTowards(leftChain[1].xDrive.target, lPerc * leftChain[1].xDrive.upperLimit, deltaAngle);
        gripperAngles[2] = Mathf.MoveTowards(leftChain[2].xDrive.target, lPerc * leftChain[2].xDrive.upperLimit * -1, deltaAngle);

        // Get the right angles
        gripperAngles[3] = Mathf.MoveTowards(rightChain[0].xDrive.target, rPerc * rightChain[0].xDrive.upperLimit, deltaAngle);
        gripperAngles[4] = Mathf.MoveTowards(rightChain[1].xDrive.target, rPerc * rightChain[1].xDrive.upperLimit, deltaAngle);
        gripperAngles[5] = Mathf.MoveTowards(rightChain[2].xDrive.target, rPerc * rightChain[2].xDrive.upperLimit * -1, deltaAngle);

        for (int i = 0; i < gripperChain.Length; i++) {
            setTargetAngle(gripperChain[i], gripperAngles[i]);
        }
    }

    // Set the target angle for the joint control on the articulation body
    private void setTargetAngle(ArticulationBody joint, float angle) {
        var drive = joint.xDrive;
        drive.target = angle;
        joint.xDrive = drive;
    }

    // Open the gripper all the way to 0% closed
    public bool openGripper () {
        // Set the percents to zero
        lPerc = 0f; rPerc = 0f;

        gripperLeftStopped = false;
        gripperRightStopped = false;
        
        // Return whether it has reached it or not
        if (FloatEquality(leftChain[0].xDrive.target, leftChain[0].xDrive.upperLimit * lPerc)) { return true; }
        else { return false; }
    }

    // Close the gripper all the way to 100% closed
    // It will stop if it hits something because of the collisionHandler on the the finger pads
    public bool closeGripper () {
        // Set the percents to 100
        if (!gripperLeftStopped) { lPerc = 1f; }
        else { gripperLeftStopped = false; }
        if (!gripperRightStopped) { rPerc = 1f; }
        else { gripperRightStopped = false; }
        
        // Return whether it has reached it or not
        if (FloatEquality(leftChain[0].xDrive.target, leftChain[0].xDrive.upperLimit * lPerc)) { return true; }
        else { return false; }
    }
    
    // Close the gripper all the way to a certain percentage closed
    // It will stop if it hits something because of the collisionHandler on the the finger pads
    public bool closeGripper (float percent) {
        // Set both to a clamp of the percentage
        if (!gripperLeftStopped || percent < lPerc) { lPerc = Mathf.Clamp(percent, 0f, 1f); }
        else { gripperLeftStopped = false; }
        if (!gripperRightStopped || percent < rPerc) { rPerc = Mathf.Clamp(percent, 0f, 1f); }
        else { gripperRightStopped = false; }
        
        // Return whether it has reached it or not
        if (FloatEquality(leftChain[0].xDrive.target, leftChain[0].xDrive.upperLimit * lPerc)) { return true; }
        else { return false; }
    }

    // Close the gripper all the way to a certain percentage closed on each finger
    // It will stop if it hits something because of the collisionHandler on the the finger pads
    public bool closeGripper (float percentL, float percentR) {
        // Set to a clamp of the provided percentage
        if (!gripperLeftStopped || percentL < lPerc) { lPerc = Mathf.Clamp(percentL, 0f, 1f); }
        else { gripperLeftStopped = false; }
        if (!gripperRightStopped || percentR < rPerc) { rPerc = Mathf.Clamp(percentR, 0f, 1f); }
        else { gripperRightStopped = false; }
        
        // Return whether it has reached it or not
        if (FloatEquality(leftChain[0].xDrive.target, leftChain[0].xDrive.upperLimit * lPerc)) { return true; }
        else { return false; }
    }

    // Stop the gripper where at the percentage it is currently at
    public void stopGripper(float stopOffset = 0f) {
        if (!gripperLeftStopped) {
            lPerc = stopOffset + (leftChain[0].xDrive.target /  leftChain[0].xDrive.upperLimit);
            gripperLeftStopped = true;
        }
        if (!gripperRightStopped) {
            rPerc = stopOffset + (rightChain[0].xDrive.target /  rightChain[0].xDrive.upperLimit);
            gripperRightStopped = true;
        }
    }

    // Stop the right part of the gripper at the percentage it is currently at
    public void stopLeftGripper(float stopOffset = 0f) {
        if (!gripperLeftStopped) {
            lPerc = stopOffset + (leftChain[0].xDrive.target /  leftChain[0].xDrive.upperLimit);
            gripperLeftStopped = true;
        }
    }

    // Stop the right part of the gripper at the percentage it is currently at
    public void stopRightGripper(float stopOffset = 0f) {
        if (!gripperRightStopped) {
            rPerc = stopOffset + (rightChain[0].xDrive.target /  rightChain[0].xDrive.upperLimit);
            gripperRightStopped = true;
        }
    }

    // Check if the two provided floats are equal
    private static bool FloatEquality(float a, float b) {
        if (float.IsNaN(a) || float.IsNaN(b)) 
            return false;
        if (float.IsInfinity(a) || float.IsInfinity(b)) 
            return a == b;
        return Mathf.Abs(a - b) < 1e-5 * 0.5;
    }
}