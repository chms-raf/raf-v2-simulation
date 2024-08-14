using UnityEngine;
using System.Collections.Generic;
using Unity.Robotics;
using Unity.Robotics.UrdfImporter.Control;
using UrdfControlRobot = Unity.Robotics.UrdfImporter.Control;
using UnityEngine.InputSystem;
using System.Linq;
public class RobotController : MonoBehaviour {
    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public enum ControlType { PositionControl };

    //Fields for inputs
    [Header("Robot Control References")]
    [SerializeField] private InputActionProperty moveJointInput;
    [SerializeField] private InputActionProperty selectJointInput;
    [SerializeField] private InputActionProperty toggleGripperInput;


    //Robot properties
    private ArticulationBody[] entireChain;
    private ArticulationBody[] articulationChain;
    private ArticulationBody[] gripperChain;
    private Color[] prevColor;
    private int previousIndex;
    [InspectorReadOnly(hideInEditMode: true)]
    public string selectedJoint;
    [HideInInspector] public int selectedIndex;
    [Header("Robot properties")]
    public ControlType control = ControlType.PositionControl;
    public float stiffness;
    public float damping;
    public float forceLimit;
    public float speed = 5f; // Units: degree/s
    public float torque = 100f; // Units: Nm or N
    public float acceleration = 5f;// Units: m/s^2 / degree/s^2
    [Tooltip("Color to highlight the currently selected Join")]
    public Color highLightColor = new Color(1, 0, 0, 1);
    [HideInInspector] public bool gripperClosed = false;
    
    //The old controller
    private Controller controller;

    private void OnEnable()
    {
        this.gameObject.AddComponent(typeof(Controller));
        controller = GetComponent<Controller>();
        SetControllerValues();
    }
    void Start()
    {
        // Initialize the robot
        previousIndex = selectedIndex = 0;
        this.gameObject.AddComponent<FKScript>();
        entireChain = this.GetComponentsInChildren<ArticulationBody>();
        int defDyanmicVal = 10;

        // Go through the entire articulation chain to set the joint values
        foreach (ArticulationBody joint in entireChain) {
            joint.gameObject.AddComponent<JointControl>();
            joint.jointFriction = defDyanmicVal;
            joint.angularDamping = defDyanmicVal;
            ArticulationDrive currentDrive = joint.xDrive;
            currentDrive.forceLimit = forceLimit;
            joint.xDrive = currentDrive;
        }

        // Get only the joints that should be moved using the controller
        articulationChain = entireChain[1..7];

        // Get the joints for the gripper (Right + Left Inner/Outer Knuckles)
        gripperChain = entireChain[9..11].Concat(entireChain[14..16]).ToArray();

        // Display the selected joint and store its colors
        DisplaySelectedJoint(selectedIndex);
        StoreJointColors(selectedIndex);

        // Highlight the currently selected joint
        Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(1).GetComponentsInChildren<Renderer>();
        foreach (var mesh in rendererList)
        {
            MaterialExtensions.SetMaterialColor(mesh.material, highLightColor);
        }

        // Enable to read new inputs
        selectJointInput.action.Enable();
        moveJointInput.action.Enable();
        toggleGripperInput.action.Enable();
    }

    // Updates the joints every scan
    private void Update() {
        // Check to see if the selected index is valid
        SetSelectedJointIndex(selectedIndex);

        // Update the direction based on the selection index
        UpdateDirection(selectedIndex);

        // If one key has been triggered to selectJointInput
        if (selectJointInput.action.triggered)
        {
            //Read the value: Right =  (0.0, 1.0) // Left = (0.0, -1.0)
            Vector2 inputValue = selectJointInput.action.ReadValue<Vector2>();

            // Increment the selected joint
            if (inputValue.x > 0) {
                SetSelectedJointIndex(selectedIndex + 1);
                Highlight(selectedIndex);
            } else if (inputValue.x < 0) { // Decrement the selected joint
                SetSelectedJointIndex(selectedIndex - 1);
                Highlight(selectedIndex);
            }
        }

        // If the gripper button has been triggered to toggleGripperInput
        if (toggleGripperInput.action.triggered) {
            toggleGripper();
        }
        
        // Update the direction after the index may have changed
        UpdateDirection(selectedIndex);
    }

    // Protection for setting the selected joint index
    private void SetSelectedJointIndex(int index) {
        // If the articulation chain isn't empty
        if (articulationChain.Length > 0) {
            // Update the selected Index
            selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
        }
    }

    // Highlight the currently selected joint
    private void Highlight(int selectedIndex) {
        // If the joint index is outside the proper range
        if (selectedIndex == previousIndex || selectedIndex < 0 || selectedIndex >= articulationChain.Length) { return; }
        
        // Run the helper functions
        ResetJointColors(previousIndex);
        StoreJointColors(selectedIndex);
        DisplaySelectedJoint(selectedIndex);

        // Get the renderer
        Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(1).GetComponentsInChildren<Renderer>();

        // Set the material color to the chosen highLightColor
        foreach (var mesh in rendererList) {
            MaterialExtensions.SetMaterialColor(mesh.material, highLightColor);
        }
    }

    // Update the direction the joint is supposed to move based on the input
    private void UpdateDirection(int jointIndex) {
        // If the joint index is outside the proper range
        if (jointIndex < 0 || jointIndex >= articulationChain.Length) { return; }

        // Read the current value from moveJointInput
        Vector2 inputValue = moveJointInput.action.ReadValue<Vector2>();

        // Get the current joint control object that we are working on
        JointControl current = articulationChain[jointIndex].GetComponent<JointControl>();

        // Update the previous joint to no longer rotate
        if (previousIndex != jointIndex) {
            JointControl previous = articulationChain[previousIndex].GetComponent<JointControl>();
            previous.direction = UrdfControlRobot.RotationDirection.None;
            previousIndex = jointIndex;
        }

        // If the control type is wrong, then change it
        if (current.controltype != UrdfControlRobot.ControlType.PositionControl) {
            UpdateControlType(current);
        }

        // Move Positive = (1.0, 0.0) (Up)
        if (inputValue.y > 0) {
            current.direction = UrdfControlRobot.RotationDirection.Positive;
        } else if (inputValue.y < 0) { // Move Negative = (-1.0, 0.0) (Down)
            current.direction = UrdfControlRobot.RotationDirection.Negative;
        } else { // Don't move at all
            current.direction = UrdfControlRobot.RotationDirection.None;
        }

        // List<float> positions = new List<float>();
        // List<int> indexes = new List<int>();

        // articulationChain[jointIndex].GetJointPositions(positions);
        // articulationChain[jointIndex].GetDofStartIndices(indexes);

        // for (float i = 0; i < 360; i++) {
        //     positions[indexes[articulationChain[jointIndex].index]] += i * Mathf.Deg2Rad;
        //     articulationChain[jointIndex].SetJointPositions(positions);
        // }
        
        // Debug.Log(positions[indexes[articulationChain[jointIndex].index]] * Mathf.Rad2Deg);

        // Debug.Log(articulationChain[jointIndex].jointPosition[0] * Mathf.Rad2Deg);
    }

    // Store the joint colors to remember for reseting
    private void StoreJointColors(int index) {
        // Get the renderer
        Renderer[] materialLists = articulationChain[index].transform.GetChild(1).GetComponentsInChildren<Renderer>();

        // Get the current color that is saved
        prevColor = new Color[materialLists.Length];

        // Store the material list in the prevColor variable
        for (int counter = 0; counter < materialLists.Length; counter++) {
            prevColor[counter] = MaterialExtensions.GetMaterialColor(materialLists[counter]);
        }
    }

    // Reset the joint color back to its previous state
    private void ResetJointColors(int index) {
        // Get the renderer
        Renderer[] previousRendererList = articulationChain[index].transform.GetChild(1).GetComponentsInChildren<Renderer>();

        // Reset the material list store to the prevColor stored
        for (int counter = 0; counter < previousRendererList.Length; counter++) {
            MaterialExtensions.SetMaterialColor(previousRendererList[counter].material, prevColor[counter]);
        }
    }

    // Update the GUI text of the selected joint
    void DisplaySelectedJoint(int selectedIndex) {
        // If the joint index is outside the proper range
        if (selectedIndex < 0 || selectedIndex >= articulationChain.Length) { return; }

        // The index has 1 added to it to index starting at 1 instead of 0 for less confusion
        selectedJoint = articulationChain[selectedIndex].name + " (" + (selectedIndex + 1) + ")";
    }

    // Update the control type of the specific joint
    public void UpdateControlType(JointControl joint) {
        joint.controltype = UrdfControlRobot.ControlType.PositionControl;
        if (control == ControlType.PositionControl)
        {
            ArticulationDrive drive = joint.joint.xDrive;
            drive.stiffness = stiffness;
            drive.damping = damping;
            joint.joint.xDrive = drive;
        }
    }

    // Set the values of the controller under the library
    private void SetControllerValues() {
        controller.stiffness = stiffness;
        controller.damping = damping;
        controller.forceLimit = forceLimit;
        controller.speed = speed;
        controller.torque = torque;
        controller.acceleration = acceleration;
    }
    

    private void FixedUpdate() {
        SetControllerValues();
    }

    // Create a GUI to list information on how to operate and the current selected joint
    public void OnGUI() {
        GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(Screen.width / 2 - 200, 10, 400, 20), "Press A/D to select a robot joint. Press E to open/close gripper.", centeredStyle);
        GUI.Label(new Rect(Screen.width / 2 - 200, 30, 400, 20), "Press W/S to move " + selectedJoint + ".", centeredStyle);
    }

    // Toggle the gripper if the button is pressed
    private void toggleGripper() {
        // If the gripper is not closed
        if (!gripperClosed) {
            // Iterate through each gripper joint
            foreach (ArticulationBody joint in gripperChain) {
                // Get the current joint control object that we are working on
                JointControl current = joint.GetComponent<JointControl>();

                // If the control type is wrong, then change it
                if (current.controltype != UrdfControlRobot.ControlType.PositionControl) {
                    UpdateControlType(current);
                }

                // Apply a positive direction to the specified joint
                current.direction = UrdfControlRobot.RotationDirection.Positive;

                // Set the bool to be closed
                gripperClosed = true;
            }
        } else { // If the gripper is closed
            // Iterate through each gripper joint
            foreach (ArticulationBody joint in gripperChain) {
                // Get the current joint control object that we are working on
                JointControl current = joint.GetComponent<JointControl>();

                // If the control type is wrong, then change it
                if (current.controltype != UrdfControlRobot.ControlType.PositionControl) {
                    UpdateControlType(current);
                }

                // Apply a positive direction to the specified joint
                current.direction = UrdfControlRobot.RotationDirection.Negative;
            }

            // Set the bool to be not closed
            gripperClosed = false;
        }
    }

    // public void OnCollisionEnter(Collision collision) {
    //     foreach (ArticulationBody joint in entireChain) {
    //         // Get the current joint control object that we are working on
    //         JointControl current = joint.GetComponent<JointControl>();

    //         // If the control type is wrong, then change it
    //         if (current.controltype != UrdfControlRobot.ControlType.PositionControl) {
    //             UpdateControlType(current);
    //         }

    //         // Apply no direction to the specified joint
    //         current.direction = UrdfControlRobot.RotationDirection.None;
    //     }
    // }
}