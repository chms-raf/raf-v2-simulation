/// |----------------------------------------Mapping Controller---------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the robot using motion maps by manipulating the end effector goal through the
///              map provided by the SequenceMaps class.
/// |-------------------------------------------------------------------------------------------------------------|

using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class MappingController : MonoBehaviour {
    // Accessible Properties
    [Header("Properties")]
    [SerializeField] private Transform endEffGoal;
    [SerializeField] private float moveSpeed = 0.18f; // In m/s
    [SerializeField] private float stateChangeDelay = 0.5f;
    [SerializeField] private float yAxisScale = 0.5f; // This is for the spherical motion away from the center of robot
    [SerializeField] private Transform toolFrame;

    // Private Variables
    [HideInInspector]
    private int motionIndex = 0;
    private Vector3 homePos;
    private Quaternion homeRot;
    private float timer = 0f;
    private float stateTimer = 0f;
    private bool playing = false;
    private bool goingHome = true;
    private SequenceMaps seqMaps;
    private SequenceMaps.MotionMap currentMotMap;
    private GripperController gripController;
    private Transform currentTarget;
    private Vector3 currentTargetPos;
    private Quaternion currentTargetRot;

    // Restart the script when it is enabled
    void OnEnable() {
        playing = false;
    }

    // Called at the very start of the program
    void Awake() {
        // Initialize all of the dependencies
        seqMaps = this.gameObject.AddComponent<SequenceMaps>();
        gripController = this.GetComponent<GripperController>();
    }

    void FixedUpdate() {
        // If the motion map is supposed to be playing and there is a motion map loaded and it is not currently tweening
        if (playing && currentMotMap != null && motionIndex < currentMotMap.keyFrames.Count && !goingHome && stateTimer < 0f && !DOTween.IsTweening(endEffGoal)) {
            // Get the current frame and next frame
            SequenceMaps.Frame currentFrame = currentMotMap.keyFrames[motionIndex];
            SequenceMaps.Frame nextFrame = null;

            // Protection for if it is at the end
            if (motionIndex < currentMotMap.keyFrames.Count - 1) {
                nextFrame = currentMotMap.keyFrames[motionIndex + 1];
            }

            // Check the current keyframe type
            switch(currentMotMap.keyFrameType[motionIndex]) {
                // If it is joint angles, move to the location it specifies
                case SequenceMaps.FrameType.JointAngles:
                    StartCoroutine(moveToLocation(currentFrame.position, currentFrame.orientation));

                    // This code partially works for dynamically adjusting the motion map, but the vector math did not work
                    // // If there is no target then follow the normal path
                    // if (currentTarget == null) {
                    //     StartCoroutine(moveToLocation(currentFrame.position, currentFrame.orientation));
                    // // Else, adjust the path to the current target
                    // } else {
                    //     // If its the target frame, go to the target instead
                    //     if (currentFrame.target == true) {
                    //         Debug.Log("Target Pos: " + currentTargetPos);
                    //         StartCoroutine(moveToLocation(currentTargetPos, currentTargetRot));
                    //     // If its the frame before the target frame, then adjust it to be in line with target
                    //     } else if (nextFrame != null) {
                    //         if (nextFrame.target == true) {
                    //             // Create a temporary object to get the necessary plane
                    //             GameObject copy = GameObject.Instantiate(endEffGoal.gameObject, currentFrame.position, currentFrame.orientation);
                    //             Vector3 plane = copy.transform.forward;
                    //             Destroy(copy);

                    //             // Calculate the adjusted position using Vector Math
                    //             // Project it on the plane according to origin, then move it to the plane of the currentFrame
                    //             Vector3 adjPos = Vector3.ProjectOnPlane(currentTargetPos, plane) + Vector3.Dot(currentFrame.position, plane) * plane;
                                
                    //             StartCoroutine(moveToLocation(adjPos, currentTargetRot));
                    //         } else {
                    //             StartCoroutine(moveToLocation(currentFrame.position, currentFrame.orientation));
                    //         }
                    //     // Else follow normal path
                    //     } else {
                    //         StartCoroutine(moveToLocation(currentFrame.position, currentFrame.orientation));
                    //     }
                    // }
                    break;

                // If it is the gripper, control the gripper
                case SequenceMaps.FrameType.Gripper:
                    if (controlGripper(currentFrame.gripperValue[0])) { incrementFrame(); }
                    break;

                // If it a delay, then delay by specified amount
                case SequenceMaps.FrameType.Delay:
                    timer += Time.deltaTime;
                    if (timer > currentFrame.delay) {
                        timer = 0f;
                        incrementFrame();
                    }
                    break;
            }
        
        // If it is playing, but not ready to transition
        } else if (playing) {
            stateTimer -= Time.deltaTime;
        }
    }

    // Changes the current motion map to the string provided
    // The map variable name is determined by the name in the JSON files
    public void changeMap(string map, Transform target = null) {
        // Turn off playing 
        playing = false;

        // Reset the motion index
        motionIndex = 0;

        // Grab the requested map from Sequence Maps
        currentMotMap = seqMaps.getMotionMap(map);

        // Open the gripper
        gripController.openGripper();

        // Set the current target to target
        currentTarget = target;
        if (currentTarget != null) {
            currentTargetPos = target.position;
            currentTargetRot = target.rotation;
        }

        // Grab the home position and rotation from map
        homePos = currentMotMap.keyFrames[motionIndex].position;
        homeRot = currentMotMap.keyFrames[motionIndex].orientation;

        // Move the end effector to the home position
        StartCoroutine(moveToHomePosition());
    }

    // Move to a location by Slerping between a rotation and position by a distance amount
    IEnumerator moveToLocation(Vector3 nextP, Quaternion nextR) {
        // Create a move tween
        Tween moveTween;

        // Get the current pos
        Vector3 currentPos = endEffGoal.position;

        // Get the duration
        float dur;
        
        // Scale the y axis if it is going near the center of the robot so it flips correctly 
        if ((Mathf.Abs(nextP.x) < 0.05 && Mathf.Abs(nextP.z) < 0.05) || (Mathf.Abs(currentPos.x) < 0.05 && Mathf.Abs(currentPos.z) < 0.05)) {
            // Get the midpoint
            Vector3 midP = ((currentPos + nextP) / 2) + new Vector3(0f, yAxisScale, 0f);
            
            // Create an array of the waypoints
            Vector3[] waypoints = new Vector3[] {midP, nextP};

            // Find the duration
            dur = ((currentPos - midP).magnitude / moveSpeed) + ((midP - nextP).magnitude / moveSpeed);

            // Create a bezier curve
            moveTween = endEffGoal.DOPath(waypoints, dur, pathType: PathType.CatmullRom).OnComplete(incrementFrame);
            //moveTween = endEffGoal.DOJump(nextP, yAxisScale, 1, dur).OnComplete(incrementFrame);
        } else {
            // Find the duration
            dur = (currentPos - nextP).magnitude / moveSpeed;

            // Do a normal move
            moveTween = endEffGoal.DOMove(nextP, dur).OnComplete(incrementFrame);
        }

        // Rotate the object
        if (Quaternion.Angle(endEffGoal.rotation, nextR) > 177) { // If the angle between is too large, flip it the other direction
            endEffGoal.DORotateQuaternion(Quaternion.Inverse(nextR), dur);
        } else {
            endEffGoal.DORotateQuaternion(nextR, dur);
        }
        

        // Wait until the tween is done
        yield return moveTween.WaitForCompletion();
    }

    // Move to the home position when this is called
    IEnumerator moveToHomePosition() {
        // Set going home to true
        goingHome = true;

        // Wait for it to move home
        if (Mathf.Abs(endEffGoal.position.x - homePos.x) > 0.05 || Mathf.Abs(endEffGoal.position.z - homePos.z) > 0.05) {
            yield return moveToLocation(homePos, homeRot); 
        }

        // Reset motion index (Assuming first frame is the home position)
        motionIndex = 1;

        // Set going home to false
        goingHome = false;
    }

    // Control the gripper by input the value that it should close
    bool controlGripper(float value) {
        // Set the gripper to the percentage closed and once its there, return that its there
        if (gripController.closeGripper(value)) { return true; }
        else { return false; }
    }

    // Increment to the next keyframe in the controller
    void incrementFrame() {
        // Increment the motion index
        motionIndex++;

        // Set the state change timer to the delay
        stateTimer = stateChangeDelay;
    }

    // Indicate to start playing the map
    public void playMap()     { playing = true; }

    // Indicate to stop playing the map
    public void stopPlaying() { playing = false; }
}