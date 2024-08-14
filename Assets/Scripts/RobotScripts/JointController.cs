/// |-----------------------------------------Joint Controller----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This controller controls the robot on a joint by joint basis.          
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using Unity.Robotics;
using UrdfControlRobot = Unity.Robotics.UrdfImporter.Control;
using UnityEngine.InputSystem;
using System.Linq;
using System.ComponentModel.Design;

public class JointController : MonoBehaviour {
    // Control Related Variables
    [Header("Robot Controls")]
    [SerializeField] private InputActionProperty moveJointInput;
    [SerializeField] private InputActionProperty selectJointInput;
    [SerializeField] private InputActionProperty openGrip;
    [SerializeField] private InputActionProperty closeGrip;

    // Script Settings
    [Header("Properties")]
    public Color highlightColor = new Color(1, 0, 0, 1);

    // Private Variables
    [HideInInspector]
    private Transform robot;
    private ArticulationBody[] entireChain;
    private ArticulationBody[] articulationChain;
    private Color[] prevColor;
    private int previousIndex;
    private int selectedIndex;
    private string selectedJoint;
    private bool running = true;
    private GripperController gripController;

    // OnEnable is called once the script is enabled
    private void OnEnable() {
        // Enable the input system
        moveJointInput.action.Enable();
        selectJointInput.action.Enable();
        openGrip.action.Enable();
        closeGrip.action.Enable();

        // Initialize the variables
        selectedIndex = previousIndex = 0;

        // Display the selected joint and store its colors
        DisplaySelectedJoint(selectedIndex);
        StoreJointColors(selectedIndex);

        // Highlight the currently selected joint
        Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(1).GetComponentsInChildren<Renderer>();
        foreach (var mesh in rendererList)
        {
            MaterialExtensions.SetMaterialColor(mesh.material, highlightColor);
        }
    }

    // OnDisable is called once the script is disabled
    private void OnDisable() {
        // Disable the input system
        moveJointInput.action.Disable();
        selectJointInput.action.Disable();
        openGrip.action.Disable();
        closeGrip.action.Disable();

        ResetJointColors(selectedIndex);
    }

    // Awake is the very first executed
    void Awake() {
        // Initialize the robot
        robot = GameObject.FindWithTag("robot").transform;
        entireChain = robot.GetComponentsInChildren<ArticulationBody>();

        // Get only the joints that should be moved using the controller
        articulationChain = entireChain[1..7];

        // Get the gripper controller
        gripController = this.GetComponent<GripperController>();
    }

    // Update is called once per frame
    void Update() {
        // Check if we should be running
        if (running) {
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

            // Update the direction after the index may have changed
            UpdateDirection(selectedIndex);

            // Open/Close the Gripper
            if (openGrip.action.IsPressed()) {
                gripController.openGripper();
            } else if (closeGrip.action.IsPressed()) {
                gripController.closeGripper();
            } else {
                gripController.stopGripper();
            }
        }
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

        // Set the material color to the chosen highlightColor
        foreach (var mesh in rendererList) {
            MaterialExtensions.SetMaterialColor(mesh.material, highlightColor);
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

        // Move Positive = (1.0, 0.0) (Up)
        if (inputValue.y > 0) {
            current.direction = UrdfControlRobot.RotationDirection.Positive;
        } else if (inputValue.y < 0) { // Move Negative = (-1.0, 0.0) (Down)
            current.direction = UrdfControlRobot.RotationDirection.Negative;
        } else { // Don't move at all
            current.direction = UrdfControlRobot.RotationDirection.None;
        }
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

    // Create a GUI to list information on how to operate and the current selected joint
    public void OnGUI() {
        GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontSize = 20;
        centeredStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(Screen.width / 2 - 300, 10, 600, 30), "Press A/D to select a robot joint. Press E/R to open/close gripper.", centeredStyle);
        GUI.Label(new Rect(Screen.width / 2 - 200, 40, 400, 30), "Press W/S to move " + selectedJoint + ".", centeredStyle);
    }

    public void Run()  { running = true; }
    public void Stop() { running = false; }
}
