/// |-----------------------------------------Control System------------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the controllers and determine which one is active. This also ensures the
///              controller class that was modified in URDF importer is set since scripts from that library depend
///              on it. Any of the robot joint properties are also constantly updated via this script.
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using Unity.Robotics.UrdfImporter.Control;
using UnityEngine.InputSystem;

public class ControlSystem : MonoBehaviour {
    // Enums
    public enum ControlType { CartesianControl, JointControl, MappingControl };

    // Control Related Variables
    [Header("Robot Controls")]
    [SerializeField] private InputActionProperty selectControl;
    public ControlType control = ControlType.CartesianControl;

    // Property Related Variables
    [Header("Robot Properties")]
    public Transform toolFrame;
    public Transform endEffGoal;
    public float stiffness;
    public float damping;
    public float forceLimit;
    public float gripperForceLimit;
    public float speed = 5f; // Units: degree/s
    public float torque = 100f; // Units: Nm or N
    public float acceleration = 5f;// Units: m/s^2 / degree/s^2

    // Private Variables
    [HideInInspector]
    private ArticulationBody[] articulationChain;
    private Controller controller;
    private Transform robot;
    private CartesianController cartControl;
    private JointController jointControl;
    private MappingController mapControl;
    private BioIK.BioIK bioIK;
    bool running = true;


    // OnEnable is called once the script is enabled
    private void Start() {
        // Add the modified Controller component from the URDF Importer Library
        robot.gameObject.AddComponent(typeof(Controller));
        controller = robot.GetComponent<Controller>();
        SetControllerValues();

        // Enable the input system
        selectControl.action.Enable();

        // Set the initial control type
        changeControlType(control);
    }

    // OnDisable is called once the script is disabled
    private void OnDisable() {
        // Destroy the controller
        Destroy(controller);

        // Disable the input system
        selectControl.action.Disable();
    }

    // Awake is the very first executed function
    void Awake() {
        // Initialize the robot
        robot = GameObject.FindWithTag("robot").transform;
        articulationChain = robot.GetComponentsInChildren<ArticulationBody>();
        int defDyanmicVal = 10;

        // Go through the entire articulation chain to set the joint values
        foreach (ArticulationBody joint in articulationChain) {
            joint.gameObject.AddComponent<JointControl>();
            joint.jointFriction = defDyanmicVal;
            joint.angularDamping = defDyanmicVal;
            ArticulationDrive currentDrive = joint.xDrive;
            currentDrive.driveType = ArticulationDriveType.Target;
            currentDrive.stiffness = stiffness;
            currentDrive.damping = damping;
            // Change the force limit on the grippers
            if (joint.CompareTag("LeftGrip") || joint.CompareTag("RightGrip")) {
                currentDrive.forceLimit = gripperForceLimit;
            } else {
                currentDrive.forceLimit = forceLimit;
            }
            joint.xDrive = currentDrive;
        }

        // Get the other controller scripts
        cartControl = this.GetComponent<CartesianController>();
        jointControl = this.GetComponent<JointController>();
        mapControl = this.GetComponent<MappingController>();
        bioIK = robot.GetComponent<BioIK.BioIK>();
    }

    // Update is called once per frame
    void Update() {
        // If the select control button is pressed
        if (selectControl.action.triggered && running) {
            // Switch the current selected control type to the next
            switch (this.control) {
                case ControlType.CartesianControl:
                    control = ControlType.JointControl;
                    break;
                case ControlType.JointControl:
                    control = ControlType.MappingControl;
                    break;
                case ControlType.MappingControl:
                    control = ControlType.CartesianControl;
                    break;
                default:
                    control = ControlType.CartesianControl;
                    break;
            }
            
            // Change the control type
            changeControlType(control);
        }
    }

    // Switches the control type
    public void changeControlType(ControlType controlType) {
        switch (controlType) {
            case ControlType.CartesianControl:
                cartControl.enabled  = true;
                jointControl.enabled = false;
                mapControl.enabled = false;
                bioIK.enabled = true;
                break;
            case ControlType.JointControl:
                cartControl.enabled  = false;
                jointControl.enabled = true;
                mapControl.enabled = false;
                bioIK.enabled = false;
                break;
            case ControlType.MappingControl:
                // Update the end effector goal from joint control
                endEffGoal.position = toolFrame.position;
                endEffGoal.rotation = toolFrame.rotation;
                // Update the other scripts
                cartControl.enabled  = false;
                jointControl.enabled = false;
                mapControl.enabled = true;
                mapControl.changeMap("Sip"); // Set it to a default map
                //mapControl.playMap();
                bioIK.enabled = true;
                break;
        }
    }

    public void changeControlType(int controlType) {
        ControlType control = (ControlType) controlType;
        switch (control) {
            case ControlType.CartesianControl:
                cartControl.enabled  = true;
                jointControl.enabled = false;
                mapControl.enabled = false;
                bioIK.enabled = true;
                break;
            case ControlType.JointControl:
                cartControl.enabled  = false;
                jointControl.enabled = true;
                mapControl.enabled = false;
                bioIK.enabled = false;
                break;
            case ControlType.MappingControl:
                cartControl.enabled  = false;
                jointControl.enabled = false;
                mapControl.enabled = true;
                mapControl.changeMap("Sip"); // Set it to a default map
                bioIK.enabled = true;
                break;
        }   
    }

    // FixedUpdate is called at a fixed interval
    void FixedUpdate() {
        SetControllerValues();
    }

    // Set the values of the controller under the library
    void SetControllerValues() {
        controller.stiffness = stiffness;
        controller.damping = damping;
        controller.forceLimit = forceLimit;
        controller.speed = speed;
        controller.torque = torque;
        controller.acceleration = acceleration;
    }

    // Change the controller map and play
    public void changeMapAndPlay(string map) {
        mapControl.changeMap(map);
        mapControl.playMap();
    }

    public void changeMapAndPlay(string map, Transform target) {
        mapControl.changeMap(map, target);
        mapControl.playMap();
    }

    public void emergencyStop() {
        running = false;
        switch (this.control) {
            case ControlType.CartesianControl:
                cartControl.Stop();
                break;
            case ControlType.JointControl:
                jointControl.Stop();
                break;
            case ControlType.MappingControl:
                mapControl.stopPlaying();
                break;
        }
    }

    public void resumeControl() {
        running = true;
        switch (this.control) {
            case ControlType.CartesianControl:
                cartControl.Run();
                break;
            case ControlType.JointControl:
                jointControl.Run();
                break;
            case ControlType.MappingControl:
                mapControl.playMap();
                break;
        }
    }

    public void toggleStop() {
        running = !running;
        switch(running) {
            case true:
                resumeControl();
                break;
            case false:
                emergencyStop();
                break;
        }
    }

    // Create a GUI to list information on how to operate and the current control map
    public void OnGUI() {
        GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperLeft;
        centeredStyle.normal.textColor = Color.black;
        centeredStyle.fontSize = 20;
        GUI.Label(new Rect(10, Screen.height - 90, 400, 30), "Press O/RightStick to change Control.", centeredStyle);
        GUI.Label(new Rect(10, Screen.height - 60, 400, 30), "Current Control Map: " + "<color=red>" + control + "</color>", centeredStyle);
        GUI.Label(new Rect(10, Screen.height - 30, 400, 30), "Open settings menu with P/Start", centeredStyle);
    }

}
