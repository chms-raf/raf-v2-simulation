/// |-------------------------------------------Gripper Collision-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the gripper when it hits a target object so it stops.
/// |-------------------------------------------------------------------------------------------------------------|

using System.Security.Cryptography;
using UnityEngine;
using UrdfControlRobot = Unity.Robotics.UrdfImporter.Control;

public class GripperCollision : MonoBehaviour {
	// Private variables
	private RobotController robotController;
	private GameObject gripperBase;
	private ArticulationBody[] articulationChain;
	private GripperController gripControl;

	void Start() {
		// Get the Gripper Base according to where the script is located
		gripperBase = this.transform.parent.parent.parent.parent.gameObject;

		// Get the chain of articulation bodies for the gripper
		articulationChain = gripperBase.GetComponentsInChildren<ArticulationBody>();

		// Get the gripper controller
		gripControl = GameObject.FindFirstObjectByType<GripperController>();
	}

	// Gets called at the start of the collision
	void OnCollisionEnter(Collision collision) {
		// Check if the object is on the left gripper and not colliding with another object on the left gripper
		if (this.gameObject.CompareTag("LeftGrip") && !collision.gameObject.CompareTag("LeftGrip")) {
			// Iterate through each gripper joint
			foreach (ArticulationBody joint in articulationChain) {
				// Check if it is apart of the Left Gripper
				if (joint.gameObject.CompareTag("LeftGrip")) {
				
					// Get the current joint control object that we are working on
					JointControl current = joint.GetComponent<JointControl>();
					
					// Apply no direction to the specified joint on left gripper
					current.direction = UrdfControlRobot.RotationDirection.None;
					
					// Stop gripper
					if (collision.gameObject.CompareTag("Target") || collision.gameObject.CompareTag("Utensil")) {
						gripControl.stopLeftGripper(collision.gameObject.GetComponent<TargetCollisionHandler>().stopOffset);
					} else {
						gripControl.stopLeftGripper();
					}	
				}
			}
		// Check if the object is on the right gripper and not colliding with another object on the right gripper
		} else if (this.gameObject.CompareTag("RightGrip") && !collision.gameObject.CompareTag("RightGrip")) {
			// Iterate through each gripper joint
			foreach (ArticulationBody joint in articulationChain) {
				// Check if it is apart of the Right Gripper
				if (joint.gameObject.CompareTag("RightGrip")) {
					// Get the current joint control object that we are working on
					JointControl current = joint.GetComponent<JointControl>();
					
					// Apply no direction to the specified joint on right gripper
					current.direction = UrdfControlRobot.RotationDirection.None;
					
					// Stop gripper
					if (collision.gameObject.CompareTag("Target") || collision.gameObject.CompareTag("Utensil")) {
						gripControl.stopRightGripper(collision.gameObject.GetComponent<TargetCollisionHandler>().stopOffset);
					} else {
						gripControl.stopRightGripper();
					}
				}
			}
		}
		// Debug.Log(this.gameObject.name + ": Entered collision with " + collision.gameObject.name);
	}


	void OnCollisionStay (Collision collision) {

	}

	// Gets called when the object exits the collision
	void OnCollisionExit(Collision collision) {

	}
}