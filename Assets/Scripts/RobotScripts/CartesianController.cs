/// |---------------------------------------Cartesian Controller--------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the robot using cartesian mode so the end effector will follow its goal
///              position and orientation and the inputs control the position and orientation based on the Xbox
///              mode provided by Kinova.
/// |-------------------------------------------------------------------------------------------------------------|

using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CartesianController : MonoBehaviour {
    // Accessible Properties
    [Header("Properties")]
    [SerializeField] private Transform endEffGoal;
    [SerializeField] private float moveSpeed = 0.05f;  // The amount of meters its allowed to move by in an update
    [SerializeField] private float rotateSpeed = 0.5f; // The amount of degrees its allowed to rotate by in an update

    // The control map properties
    [Header("Control Map")]
    [SerializeField] private InputActionProperty moveHoriz;
    [SerializeField] private InputActionProperty moveVert;
    [SerializeField] private InputActionProperty homePos;
    [SerializeField] private InputActionProperty retractPos;
    [SerializeField] private InputActionProperty changeSpeed;
    [SerializeField] private InputActionProperty clearFault;
    [SerializeField] private InputActionProperty applyEStop;
    [SerializeField] private InputActionProperty openGrip;
    [SerializeField] private InputActionProperty closeGrip;

    // Private Variables
    [HideInInspector]
    private Rigidbody rigBod;
    private Vector3 homePosition;
    private Quaternion homeRotation;
    private float maxDist = 0.001f;
    private bool eStop = false;
    private bool running = true;
    private GripperController gripController;

    void OnEnable() {
        // Enable to read new inputs
        moveHoriz.action.Enable();
        moveVert.action.Enable();
        homePos.action.Enable();
        retractPos.action.Enable();
        changeSpeed.action.Enable();
        clearFault.action.Enable();
        applyEStop.action.Enable();
        openGrip.action.Enable();
        closeGrip.action.Enable();
    }

    void OnDisable() {
        // Disable to no longer read inputs
        moveHoriz.action.Disable();
        moveVert.action.Disable();
        homePos.action.Disable();
        retractPos.action.Disable();
        changeSpeed.action.Disable();
        clearFault.action.Disable();
        applyEStop.action.Disable();
        openGrip.action.Disable();
        closeGrip.action.Disable();
    }

    // Called at the very start of the program
    void Start() {
        // Get the necessary variables
        rigBod = endEffGoal.GetComponent<Rigidbody>();
        homePosition = endEffGoal.position;
        homeRotation = endEffGoal.rotation;
        gripController = this.GetComponent<GripperController>();
    }

    // Called at a fixed interval
    // Any movement done is done in fixed update so it is not based on frame rate
    void FixedUpdate() {
        if (running && !eStop) {
            // Create a movement vector
            Vector3 movement = new Vector3(0,0,0);

            // If moving horizontally
            if (moveHoriz.action.IsPressed()) { 
                // Read the value:
                Vector2 inputValue = moveHoriz.action.ReadValue<Vector2>();
                
                // Add x and z movement
                movement += Vector3.right * inputValue[0] * moveSpeed;
                movement += Vector3.forward * inputValue[1] * moveSpeed;
            }
            // If moving vertically
            if (moveVert.action.IsPressed()) { 
                // Read the value:
                float inputValue = moveVert.action.ReadValue<float>();
                
                // Add y movement
                movement += Vector3.up * inputValue * moveSpeed;
            }

            // Apply the movement to the rigidbody
            rigBod.velocity = movement;

            // Go to home position
            if (homePos.action.IsPressed()) {
                endEffGoal.position = Vector3.MoveTowards(endEffGoal.position, homePosition, maxDist);
                endEffGoal.rotation = Quaternion.RotateTowards(endEffGoal.rotation, homeRotation, rotateSpeed);
            }

            // Go to retract position
            if (retractPos.action.IsPressed()) {
                endEffGoal.position -= new Vector3(0, maxDist, maxDist);
                endEffGoal.rotation *= Quaternion.Euler(Vector3.right * rotateSpeed);
            }
        }
    }

    // Called every frame
    // Any buttons are checked in update so it can only do an action once a frame
    void Update() {
        // Change the speed
        if (changeSpeed.action.triggered) {
            // Read the value:
            float inputValue = changeSpeed.action.ReadValue<float>();

            // Adjust the movement speed
            moveSpeed += 0.01f * inputValue;
            moveSpeed = Mathf.Clamp(moveSpeed, 0f, 0.1f);
        }
        
        // Clear the fault
        if (clearFault.action.triggered) {
            eStop = false;
        }

        // Apply the E-Stop
        if (applyEStop.action.triggered) {
            eStop = true;
        }

        // Control the gripper
        if (openGrip.action.IsPressed()) {
            gripController.openGripper();
        } else if (closeGrip.action.IsPressed()) {
            gripController.closeGripper();
        } else {
            gripController.stopGripper();
        }
    }

    public void Run()  { running = true; }
    public void Stop() { running = false; }

    
}