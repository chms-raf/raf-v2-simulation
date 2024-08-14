/// |------------------------------------------Target Button------------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the button shown on the screen to be interacted with for motion planning.              
/// |-------------------------------------------------------------------------------------------------------------|

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetButton : MonoBehaviour {
    // Accessible Properties
    [SerializeField] GameObject mouse;
    [SerializeField] GameObject actionButton;
    [SerializeField] GameObject selectButton;
    [SerializeField] GameObject targetBody;
    [SerializeField] float radius = 0.001f;                     // The radius away from the center
    [SerializeField] List<string> actions = new List<string>(); // List of actions that the target object can do

    // Private Variables
    private List<GameObject> buttons = new List<GameObject>();
    private ControlSystem controlSystem;
    private SimulatedMouse simMouseScript;
    private Vector3 positionOffset;
    
    // Start is called before the first frame update
    void Start() {
        // Get the necessary variables
        controlSystem = GameObject.FindAnyObjectByType<ControlSystem>();
        simMouseScript = Camera.main.GetComponent<SimulatedMouse>();

        // Add the cancel action to the list
        actions.Add("Cancel");

        // Create the buttons
        for (int i = 0; i < actions.Count; i++) {
            // Clone the action button
            var button = GameObject.Instantiate(actionButton);
            var action = actions[i];

            // Add the necessary properties
            button.transform.SetParent(this.transform, false);                  // Set the button to be parented to this button            
            button.SetActive(false);                                            // Set it to be inactive
            button.name = action;                                           // Set the name to be the action
            button.GetComponentInChildren<TextMeshProUGUI>().text = actions[i]; // Set the text on the button to be the action
            buttons.Add(button);                                                // Add the button to the List

            // Get the position and angle of the buttons
            var angle = i * Mathf.PI * 2 / actions.Count;
            button.transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3 (Mathf.Sin(angle) - (button.transform.GetComponent<RectTransform>().rect.width / 2), 
                                                                                             Mathf.Cos(angle) - (button.transform.GetComponent<RectTransform>().rect.height / 2), 
                                                                                             0) * radius;
            button.transform.localRotation = Quaternion.Euler(Vector3.zero);

            // Add the onClick() method to the buttons
            if (action == "Cancel") {
                button.GetComponent<Button>().onClick.AddListener(delegate { this.onClick(false); });
                button.GetComponent<Button>().onClick.AddListener(delegate { simMouseScript.setSelecting(true); });
            } else {
                button.GetComponent<Button>().onClick.AddListener(delegate { controlSystem.changeMapAndPlay(action, targetBody.transform); });
                button.GetComponent<Button>().onClick.AddListener(delegate { this.onClick(false); });
                button.GetComponent<Button>().onClick.AddListener(delegate { selectButton.GetComponent<Button>().onClick.Invoke(); });
            }
        }
    }

    // Update is called once per frame
    void Update() {
        
        
    }

    public void onClick(bool pressed) {
        if (pressed) {
            // Make it so it can't interact with the behind button anymore
            this.transform.GetComponent<Button>().interactable = false;

            // Make the buttons show and be set to the render texture
            foreach (var button in buttons) {
                // Get the offset to the mouse
                positionOffset = this.transform.position - mouse.transform.position;
                positionOffset.z = 0f;
                button.transform.GetComponent<RectTransform>().anchoredPosition3D += positionOffset;
                
                // Set the button to active and set the parent to the render texture
                button.SetActive(true);
                button.transform.SetParent(this.transform.parent, true);
            }
        } else {
            // Make it so the behind button is interactable again
            this.transform.GetComponent<Button>().interactable = true;

            // Make the buttons hidden and set the parent back to the main button
            foreach (var button in buttons) {
                // Remove the mouseOffset
                button.transform.GetComponent<RectTransform>().anchoredPosition3D -= positionOffset;
                
                button.SetActive(false);
                button.transform.SetParent(this.transform);
            }
        }
    }
}
