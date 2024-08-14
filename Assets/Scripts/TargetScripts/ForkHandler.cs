/// |-------------------------------------------Fork Handler------------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class handles the fork behavior to allow it to grab items and be held by the robot.
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using System.Collections;
public class ForkHandler : MonoBehaviour {
    // Private Variables
    private bool stuck;
    private Vector3 origPos;
    private Quaternion origRot;

    private void OnTriggerEnter(Collider collider) {
        // If the collider is a target
        if (collider.CompareTag("Target") && !stuck) {
            origPos = collider.transform.position;
            origRot = collider.transform.rotation;
            StartCoroutine(StickToFork(collider.gameObject));
        }
    }

    IEnumerator StickToFork(GameObject target) {
        // Disable sticking any other objects
        stuck = true;

        // Wait until fork stops moving
        yield return new WaitForSeconds(1f);

        // Created a fixed joint and stick it to the target
        if(target.GetComponent<FixedJoint>() != null) {
            target.GetComponent<FixedJoint>().connectedBody = this.transform.parent.GetComponent<Rigidbody>();
            target.GetComponent<FixedJoint>().enableCollision = false;
            
        } else {
            target.AddComponent<FixedJoint>();
            target.GetComponent<FixedJoint>().connectedBody = this.transform.parent.GetComponent<Rigidbody>();
            target.GetComponent<FixedJoint>().enableCollision = false;
        }

        // Disable collision on the target
        target.GetComponent<BoxCollider>().enabled = false;
        
        // Wait for 15 seconds to get to the face
        yield return new WaitForSeconds(15);
        
        // Destroy the fixed joint
        Destroy(target.GetComponent<FixedJoint>());

        // Enable collision on the target
        target.GetComponent<BoxCollider>().enabled = true;

        // Wait for 1 second then teleport back to the original spot
        yield return new WaitForSeconds(1);
        target.transform.position = origPos;
        target.transform.rotation = origRot;

        // Allow another object to stick
        stuck = false;
    }
}