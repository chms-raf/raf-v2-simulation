/// |-----------------------------------------Simulated Mouse-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the mouse on the GUI screen in order to line it with the middle of the screen
///              and interact with buttons.
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class SimulatedMouse : MonoBehaviour {
    // Accessible Properties
    [SerializeField] GameObject screen;
    [SerializeField] GameObject mouse;
    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject rendText;
    [SerializeField] float duration = 2f; // The amount of duration until it interacts with the component

    // Private variables
    private RaycasterWorld _raycaster;
    private PointerEventData _PointerEventData;
    private EventSystem _EventSystem;
    private RectTransform mouseRect;
    private Button selectedButton = null;
    private Button previousButton = null;
    private float timer = 0f;    // Timer variable to see how much time has passed
    private bool buttonHover = false;
    private bool selecting = false;
    private Image cursor = null;
    private SpriteRenderer selectCursor = null;

    // Called at the start when script becomes active
    void Start() {
        // Fetch the raycaster from the object
        _raycaster = screen.GetComponent<RaycasterWorld>();
        
        //Fetch the Event System from the Scene
        _EventSystem = GetComponent<EventSystem>();

        // Get the rect of the mouse
        mouseRect = mouse.GetComponent<RectTransform>();

        // Get the cursor image
        cursor = mouse.GetComponent<Image>();

        // Get the select cursor image
        selectCursor = mouse.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        //Set up the new Pointer Event
        _PointerEventData = new PointerEventData(_EventSystem);
        
        // Create a list of raycast results
        List<RaycastResult> results = new List<RaycastResult>();

        // Create a ray that shoots from the middle of the screen
        _raycaster.Raycast(_PointerEventData, results);
        
        // If there are any results from the raycast
        if (results.Any()) {
            // Get the world position and clamp it to within the screen limits
            Vector3 pos = results[0].worldPosition;
            // pos.x = Mathf.Clamp(pos.x, 0.45f, 0.73f);
            // pos.y = Mathf.Clamp(pos.y, 0.27f, 0.45f);
            // pos.z = -Mathf.Clamp(-pos.z, 0.10f, 0.32f);
    
            // Set the mouse to that position
            mouse.transform.position = pos;

            // See if the mouse is overlapping any buttons
            foreach (Transform button in buttonParent.transform) {
                if (getSelectedButton(button)) { break; }
            }

            // See if the mouse is interacting with any object selection buttons
            foreach (Transform selectButton in rendText.transform) {
                if (selecting) {
                    if (getSelectedButton(selectButton)) { break; }
                } else if (selectButton.CompareTag("ActionPrompt")) {
                    if (getSelectedButton(selectButton)) { break; }
                }  
            }

            // Wait to press the button
            if (buttonHover && previousButton != selectedButton && selectedButton.interactable == true) {
                // Add the time change since the last frame
                timer += Time.deltaTime;

                // Change to the select icon
                mouse.GetComponent<RectTransform>().localScale = new Vector3(0.015f, 0.015f, 1f);
                cursor.enabled = false;
                selectCursor.enabled = true;

                // Lerp the radial mask on the select icon
                float degVal = Mathf.Lerp(0f, 360f, timer/duration);
                selectCursor.material.SetFloat("_Arc1", degVal);
                
                
                // Check if its longer than the duration
                if (timer > duration) {
                    // If it is then reset the timer and invoke the onClick() method on the button
                    timer = 0f;
                    selectedButton.onClick.Invoke();

                    // If the "Start Selection" button is active
                    if (selectedButton.name == "ToggleSelection" && selectedButton.GetComponent<toggleButton>().selected == true) {
                        this.selecting = true;
                    } else if (selectedButton.name == "ToggleSelection" && selectedButton.GetComponent<toggleButton>().selected == false) {
                        this.selecting = false;
                    }

                    // Set previous button to selected so it can't redo it
                    previousButton = selectedButton;

                    // Reset cursor color to white
                    cursor.color = Color.white;
                }
            } else {
                // Change to the normal mouse icon
                mouse.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                cursor.enabled = true;
                selectCursor.enabled = false;

                // Reset the radial mask on the select icon
                selectCursor.material.SetFloat("_Arc1", 0f);
            }
        }
    }

    // Checks whether the button contains the mouse cursor or not
    bool getSelectedButton(Transform button) {
        // Get the rect transform of the button
        RectTransform bRect = button.GetComponent<RectTransform>();

        // Create a corners variable
        Vector3[] corners = new Vector3[4];

        // Get the world corners of the button rect and create a rect out of it
        bRect.GetWorldCorners(corners);
        Rect rec = new Rect(corners[0].x, corners[0].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y);

        // Get the world corners of the mouse rect
        mouseRect.GetWorldCorners(corners);

        // Check if the button rec contains the mouse
        if (rec.Contains(new Vector2(corners[0].x, corners[0].y)) && rec.Contains(new Vector2(corners[2].x, corners[2].y))) {
            // If they do not equal then the button has changed
            if (selectedButton != button.GetComponent<Button>()) {
                selectedButton = button.GetComponent<Button>();
                timer = 0f;
            }
            
            // Keep button hover true
            buttonHover = true;
            return true;
        } else {
            // If it no longer contains then reset the timer and hover set to false, set previous button to null since it left
            if (selectedButton == button.GetComponent<Button>()) {
                buttonHover = false;
                timer = 0f;
                previousButton = null;
            }
            return false;
        }
    }

    // Setter for the selecting value to determine whether it should select a target button or not
    public void setSelecting(bool val) {
        selecting = val;
    }
}