/// |-------------------------------------Target Collision Handler------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the fixed joint on the target for safe travel when the gripper picks it up.
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using System.Collections;

public class TargetCollisionHandler : MonoBehaviour {
    // Accessible Properties
	[SerializeField] Transform toolFrame;
    [SerializeField] bool resetPosition = false;
    [SerializeField] public float stopOffset;
    
    // Private variables
    bool contactLeft = false;
    bool contactRight = false;
    Vector3 originalPos;
    Quaternion originalRot;

    void Start() {
        originalPos = this.transform.position;
        originalRot = this.transform.rotation;
    }
    // Runs as one of the last scripts in the frame
	void FixedUpdate() {
        // If there is contact from the left gripper and right gripper
        if (contactLeft && contactRight) {
            // If the fixed joint exists, set the connected body to the tool frame
            if (this.transform.GetComponent<FixedJoint>() != null) {
                this.transform.GetComponent<FixedJoint>().connectedArticulationBody = toolFrame.GetComponent<ArticulationBody>();
            
            // If it doesn't exist, create it and connect it to the tool frame
            } else {
                this.transform.gameObject.AddComponent<FixedJoint>();
                this.transform.GetComponent<FixedJoint>().connectedArticulationBody = toolFrame.GetComponent<ArticulationBody>();
            }
        
        // If it isn't connected, then destroy the fixed joint
        } else {
            if (resetPosition && this.transform.GetComponent<FixedJoint>() != null) {
                Destroy(this.transform.GetComponent<FixedJoint>());
                StartCoroutine(goToOriginalPos());
            } else { 
                Destroy(this.transform.GetComponent<FixedJoint>());
            }
        }
	}

    IEnumerator goToOriginalPos() {
        yield return new WaitForSeconds(1);
        if (!contactLeft && !contactRight) {
            this.transform.position = originalPos;
            this.transform.rotation = originalRot;
        }
    }

	// Gets called at the start of the collision
    void OnCollisionEnter(Collision collision) { 
		if (collision.gameObject.CompareTag("LeftGrip")) { contactLeft = true; }
        if (collision.gameObject.CompareTag("RightGrip")) { contactRight = true; }
        // Debug.Log(this.gameObject.name + ": Entered collision with " + collision.gameObject.name);
	}

	void OnCollisionStay (Collision collision) {

	}

	// Gets called when the object exits the collision
	void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("LeftGrip") && collision.gameObject.name == "left_inner_finger_pad") { contactLeft = false; }
        if (collision.gameObject.CompareTag("RightGrip") && collision.gameObject.name == "right_inner_finger_pad") { contactRight = false; }
        // Debug.Log(this.gameObject.name + ": Exited collision with " + collision.gameObject.name);
	}
}