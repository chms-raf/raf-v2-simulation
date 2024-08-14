/// |-----------------------------------------Camera Control------------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls camera rotation.
/// |-------------------------------------------------------------------------------------------------------------|

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour {
    // Accessible Properties
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float maximumX =  360f;
    [SerializeField] float maximumY =  360f;
    [SerializeField] float minimumX = -360f;
    [SerializeField] float minimumY = -360f;
    
    // Private Variables
    private Transform _camera;


    void Update () {
        // Get the mouse data
        Vector2 mouseData = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime * Time.timeScale;

        // Get the rotation in the horizontal direction
        float rotationX = transform.localEulerAngles.y + (mouseData.x * sensitivity);
        if (rotationX > 180f) { rotationX -= 360f; }
        rotationX = Mathf.Clamp(rotationX, minimumX, maximumX);

        // Get the rotation in the vertical direction
        float rotationY = -transform.localEulerAngles.x + (mouseData.y * sensitivity);
        if (rotationY < -180f) { rotationY += 360f; }
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        // Get the Quaternions
        Quaternion quatY = Quaternion.AngleAxis(-rotationY, Vector3.right);
        Quaternion quatX = Quaternion.AngleAxis(rotationX, Vector3.up);

        // Debug the outputs
        // Debug.Log("DeltaX: " + mouseData.x + ", DeltaY: " + mouseData.y);
        // Debug.Log("RotX: " + mouseData.x * sensitivity + ", RotY: " + mouseData.y * sensitivity);
        // Debug.Log("X:" + rotationX + ", " + "Y: " + rotationY);

        // Set the rotation to the new rotation
        transform.localRotation = quatX * quatY;
        // transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
    }

    void Start () {
        // Set the cursor to invisble and locked
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;

        // Get the camera transform
        _camera = Camera.main.transform;
    }

    // Clamp the angle
    public void setSensitivity(float value) {
        this.sensitivity = Mathf.Clamp(value, 0f, 4f);
    }
}
