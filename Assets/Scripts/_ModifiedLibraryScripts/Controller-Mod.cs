// This script was modified from URDF Importer library since the controller was very limited
// This controller is used by the libraries other scripts so a basic version of it is used

// using System;
// using Unity.Robotics;
// using UnityEngine;
    
// namespace Unity.Robotics.UrdfImporter.Control
// {
//     public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
//     public enum ControlType { PositionControl };
    
//     public class Controller : MonoBehaviour
//     {
//         [HideInInspector]
//         public ControlType control = ControlType.PositionControl;
//         [HideInInspector]
//         public float stiffness;
//         [HideInInspector]
//         public float damping;
//         [HideInInspector]
//         public float forceLimit;
//         [HideInInspector]
//         public float speed = 5f; // Units: degree/s
//         [HideInInspector]
//         public float torque = 100f; // Units: Nm or N
//         [HideInInspector]
//         public float acceleration = 5f;// Units: m/s^2 / degree/s^2
//         [HideInInspector]
    
    
//         public void UpdateControlType(JointControl joint)
//         {
//             joint.controltype = control;
//             if (control == ControlType.PositionControl)
//             {
//                 ArticulationDrive drive = joint.joint.xDrive;
//                 drive.stiffness = stiffness;
//                 drive.damping = damping;
//                 joint.joint.xDrive = drive;
//             }
//         }
//     }
// }
