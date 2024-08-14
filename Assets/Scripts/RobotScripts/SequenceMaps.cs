/// |-------------------------------------------Sequence Maps-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class reads in the JSON sequences from the Kinova Web App program and transforms them into
///              a unity friendly class to access by the Mapping Controller.
/// |-------------------------------------------------------------------------------------------------------------|

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SequenceMaps : MonoBehaviour
{
    // JSON Interpreter Variables
    private List<Map> sequences = new List<Map>();

    // The list of generated motion maps
    private List<MotionMap> motionMaps = new List<MotionMap>();

    // Start is called before the first frame update
    void Start() {
        // Get all the JSON motion maps
        var motionMapFiles = Resources.LoadAll<TextAsset>("MotionMapJSON");

        // Create a variable for each motion map and store inside the list
        foreach (var map in motionMapFiles) {
            sequences.Add(JsonUtility.FromJson<Sequences>(map.ToString()).sequences.sequence[0]);
        }

        // Go through each sequence and add every frame to it
        foreach (var sequence in sequences) {
            // Create a new map for the sequence with its name
            motionMaps.Add(new MotionMap(sequence.name));
            var listIndex = motionMaps.Count - 1;

            // Search through the tasks and add them to the motion map
            foreach (var task in sequence.tasks) {
                // If the action is a joint angle command
                if (task.action.reachJointAngles.jointAngles != null) {
                    // Get the array of joint angles
                    var jointAngles = task.action.reachJointAngles.jointAngles.jointAngles;
                    
                    // Normalize the joints to -180 to 180
                    var jointAngle1 = (Mathf.Abs(jointAngles[0].value) > 180f) ? -(360 - Mathf.Abs(jointAngles[0].value)) : jointAngles[0].value;
                    var jointAngle2 = (Mathf.Abs(jointAngles[1].value) > 180f) ? -(360 - Mathf.Abs(jointAngles[1].value)) : jointAngles[1].value;
                    var jointAngle3 = (Mathf.Abs(jointAngles[2].value) > 180f) ? -(360 - Mathf.Abs(jointAngles[2].value)) : jointAngles[2].value;
                    var jointAngle4 = (Mathf.Abs(jointAngles[3].value) > 180f) ? -(360 - Mathf.Abs(jointAngles[3].value)) : jointAngles[3].value;
                    var jointAngle5 = (Mathf.Abs(jointAngles[4].value) > 180f) ? -(360 - Mathf.Abs(jointAngles[4].value)) : jointAngles[4].value;
                    var jointAngle6 = (Mathf.Abs(jointAngles[5].value) > 180f) ? -(360 - Mathf.Abs(jointAngles[5].value)) : jointAngles[5].value;
                    
                    // Adapt the joint angles to our system
                    jointAngle6 = jointAngle6 + 90;

                    // Add a frame for this command
                    motionMaps[listIndex].keyFrames.Add(new Frame(jointAngles: new float[] { jointAngle1, jointAngle2, jointAngle3, jointAngle4, jointAngle5, jointAngle6 }));

                    // Add the type
                    motionMaps[listIndex].keyFrameType.Add(FrameType.JointAngles);

                    // Add the target bool if applicable
                    if (task.action.reachJointAngles.jointAngles.target) { motionMaps[listIndex].keyFrames.Last().target = true; }
                
                // If the action is a gripper command
                } else if (task.action.sendGripperCommand.gripper != null) {
                    // Get the array of finger values
                    var sendGripperCommand = task.action.sendGripperCommand.gripper.finger;

                    // Go through the finger commands
                    List<float> values = new List<float>();
                    foreach (var finger in sendGripperCommand) {
                        values.Add(finger.value);
                    }
                
                    // Add a frame for this command
                    motionMaps[listIndex].keyFrames.Add(new Frame(gripperValue: values.ToArray()));

                    // Add the type
                    motionMaps[listIndex].keyFrameType.Add(FrameType.Gripper);

                    // Assume the previous joint angles is a target frame
                    // motionMaps[listIndex].keyFrames.last.target = true;
                
                // If the action is a delay
                } else if (task.action.delay != null) {
                    // Get the delay
                    var delay = task.action.delay;

                    // Add a frame for this command
                    motionMaps[listIndex].keyFrames.Add(new Frame(delay: delay.duration));

                    // Add the type
                    motionMaps[listIndex].keyFrameType.Add(FrameType.Delay);
                }
            }
        }

        // Print out the elements
        // foreach (var map in motionMaps[0].keyFrameType) {
        //     Debug.Log($"[{string.Join(", ", map)}]");
        // }
    }

    // Iterate through the list of motion maps and find the requested one and return it
    public MotionMap getMotionMap(string mapName) {
        foreach(var map in motionMaps) {
            if (map.name == mapName) { return map; }
        }
        return null;
    }

    // These classes are to make it into a friendly format for Unity
    public enum FrameType { JointAngles, Gripper, Delay }

    public class MotionMap {
        public string name;
        public List<Frame> keyFrames = new List<Frame>();
        public List<FrameType> keyFrameType = new List<FrameType>();

        public MotionMap (string name) {
            this.name = name;
        }
    }

    public class Frame {
        public float[] jointAngles;
        public Vector3 position;
        public Quaternion orientation;
        public float[] gripperValue;
        public float delay;
        public bool target;

        public Frame (float[] jointAngles = null, float[] gripperValue = null, float delay = 0f) {
            this.jointAngles = jointAngles;
            this.gripperValue = gripperValue;
            this.delay = delay;

            if (this.jointAngles != null) {
                ForwardKinematics(this.jointAngles, out position, out orientation);
            }

            this.target = false;
        }
        public void ForwardKinematics (float[] angles, out Vector3 position, out Quaternion orientation) {
            // Convert the angles into radians
            var radAngles = new List<float>();
            foreach (var angle in angles) { radAngles.Add(angle * Mathf.Deg2Rad); }

            // Get all of the homogeneous tranformation matrices
            Matrix4x4 T_B1 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[0]), -Mathf.Sin(radAngles[0]), 0f, 0f), 
                                            new Vector4(-Mathf.Sin(radAngles[0]), -Mathf.Cos(radAngles[0]), 0f, 0f), 
                                            new Vector4(0f, 0f, -1f, 0.1564f), 
                                            new Vector4(0f, 0f, 0f, 1f));
            Matrix4x4 T_12 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[1]), -Mathf.Sin(radAngles[1]), 0f, 0f), 
                                            new Vector4(0f, 0f, -1f, 0.005375f),
                                            new Vector4(Mathf.Sin(radAngles[1]), Mathf.Cos(radAngles[1]), 0f, -0.12838f),
                                            new Vector4(0f, 0f, 0f, 1f));
            Matrix4x4 T_23 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[2]), -Mathf.Sin(radAngles[2]), 0f, 0f), 
                                            new Vector4(-Mathf.Sin(radAngles[2]), -Mathf.Cos(radAngles[2]), 0f, -0.410f), 
                                            new Vector4(0f, 0f, -1f, 0f), 
                                            new Vector4(0f, 0f, 0f, 1f));
            Matrix4x4 T_34 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[3]), -Mathf.Sin(radAngles[3]), 0f, 0f), 
                                            new Vector4(0f, 0f, -1f, 0.2084301f),
                                            new Vector4(Mathf.Sin(radAngles[3]), Mathf.Cos(radAngles[3]), 0f, -0.006374974f),
                                            new Vector4(0f, 0f, 0f, 1f));
            Matrix4x4 T_45 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[4]), -Mathf.Sin(radAngles[4]), 0f, 0f), 
                                            new Vector4(0f, 0f, 1f, 0f),
                                            new Vector4(-Mathf.Sin(radAngles[4]), -Mathf.Cos(radAngles[4]), 0f, -0.10593f),
                                            new Vector4(0f, 0f, 0f, 1f));
            Matrix4x4 T_56 = new Matrix4x4(new Vector4(Mathf.Cos(radAngles[5]), -Mathf.Sin(radAngles[5]), 0f, 0f), 
                                            new Vector4(0f, 0f, -1f, 0.10593f),
                                            new Vector4(Mathf.Sin(radAngles[5]), Mathf.Cos(radAngles[5]), 0f, 0f),
                                            new Vector4(0f, 0f, 0f, 1f));

            // Get the tranformation matrix from B -> 6 (base to bracelet link)
            Matrix4x4 T_B6 = T_B1.transpose * T_12.transpose * T_23.transpose * T_34.transpose * T_45.transpose * T_56.transpose;

            // Get the value of the point relative to frame 6
            Matrix4x4 p6 = new Matrix4x4(new Vector4(0f,0f,0f,0f), new Vector4(0f,0f,0f,0f), new Vector4(0f,0f,0f,-0.2f), new Vector4(0f,0f,0f,1f));

            // Get the orientation of the point relative to the base frame
            var eulerAngles = T_B6.transpose.rotation.eulerAngles;
            orientation = Quaternion.Euler(new Vector3(-(eulerAngles[1] - 90), eulerAngles[2], eulerAngles[0]));

            // Get the value of the point relative to base frame
            p6 = T_B6 * p6.transpose;
            position = new Vector3(-p6[1,3],p6[2,3],p6[0,3]);
        }
    }

    /* All of the classes below are to read in the JSON data from Kinova Kortex Web API
     * If this changes in future updates, it will break this class structure which require a lot of editing to fix.
     * Here is a diagram on how the class structure is currently working:
     *          sequences
     *          └── sequence
     *              ├── [0]
     *              │   ├── applicationData (NU)
     *              │   ├── handle (NU)
     *              │   │   ├── identifier (NU)
     *              │   │   └── permission (NU)
     *              │   ├── name (sequence name)
     *              │   └── tasks
     *              │       ├── [0]
     *              │       │   ├── applicationData (NU)
     *              │       │   ├── action
     *              │       │   │   ├── applicationData (NU)
     *              │       │   │   ├── handle (NU)
     *              │       │   │   │   ├── identifier (NU)
     *              │       │   │   │   ├── permission (NU)
     *              │       │   │   │   └── actionType (NU)
     *              │       │   │   ├── reachJointAngles
     *              │       │   │   │   ├── jointAngles
     *              │       │   │   │   │   └── jointAngles
     *              │       │   │   │   │       ├── [0]
     *              │       │   │   │   │       │   ├── value
     *              │       │   │   │   │       │   └── jointIdentifier
     *              │       │   │   │   │       ├── ...
     *              │       │   │   │   │       └── [n]
     *              │       │   │   │   └── constraint (NU)
     *              │       │   │   │       ├── type (NU)
     *              │       │   │   │       └── value (NU)
     *              │       │   │   ├── sendGripperCommand
     *              │       │   │   │   ├── mode (NU)
     *              │       │   │   │   ├── gripper
     *              │       │   │   │   │   └── finger
     *              │       │   │   │   │       ├── [0]
     *              │       │   │   │   │       │   ├── value
     *              │       │   │   │   │       │   └── fingerIdentifier
     *              │       │   │   │   │       ├── ...
     *              │       │   │   │   │       └── [n]
     *              │       │   │   │   └── duration (NU)
     *              │       │   │   ├── delay
     *              │       │   │   │   └── duration
     *              │       │   │   └── name (NU)
     *              │       │   └── groupIdentifier (NU)
     *              │       ├── ...
     *              │       └── [n]
     *              ├── ...
     *              └── [n]
     * Anything marked with (NU) is not used.
     * If you ever have to change this, here is the link for how this was created:
     * https://tree.nathanfriend.io/?s=(%27opYs!(%27fancy!true~fullPathH~trailWgSlashH~rootDotH)~R(%27R%27Fsx2F383262handle-*i5-*V-2QXF%20Qz2tasks3*83O6OacY46**handleE2i5E2VE2acYTypeEreachJ72j7*j7OZ*joWtI54LconstraWtE*typeEKEsendGrippMCommand42modeE2grippM4w4OZwI54LbEdelay42b4Q-OgroupI5-*_3*93_39%27)~vMsB!%271%27)O2-XNUz2%20%203x*43**5dentifiM6applicaYData-7oWtAngles48%5B0%5D9%5Bn%5DBionE-**FsequenceH!falseK*valueLO_4O942MerO*2QnameRsource!VpMmissBWinX%20%7BYtBZ84*K4*_...bduraYw*fWgMx%5Cnz%7D3%01zxwb_ZYXWVRQOMLKHFEB98765432-*
     */
    [System.Serializable]
    public class Sequences {
        public Sequence sequences;
    }


    [System.Serializable]
    public class Sequence {
        public Map[] sequence;
    }

    [System.Serializable]
    public class Map {
        public string applicationData;
        public Handle handle;
        public string name;
        public Task[] tasks;
    }

    [System.Serializable]
    public class Handle {
        public int identifier;
        public int permission;
        public int actionType;
    }
    [System.Serializable]
    public class Task {
        public string applicationData;
        public Action action;
        public int groupIdentifier;
    }

    [System.Serializable]
    public class Action {
        public string applicationData;
        public Handle handle;
        public JointAngles1 reachJointAngles;
        public GripperCommand sendGripperCommand;
        public Delay delay;
        public string name;
    }

    // Joint Angles Portion
    [System.Serializable]
    public class JointAngles1 {
        public JointAngles2 jointAngles;
    }

    [System.Serializable]
    public class JointAngles2 {
        public JointAngle[] jointAngles;
        public bool target;
    }

    [System.Serializable]
    public class JointAngle {
        public float value;
        public int identifier;
    }

    // Gripper Portion
    [System.Serializable]
    public class GripperCommand {
        public int mode;
        public Gripper gripper;
        public float duration;
    }

    [System.Serializable]
    public class Gripper {
        public Finger[] finger;
    }

    [System.Serializable]
    public class Finger {
        public float value;
        public int fingerIdentifier;
    }

    // Delay Portion
    [System.Serializable]
    public class Delay {
        public float duration;
    }
}
