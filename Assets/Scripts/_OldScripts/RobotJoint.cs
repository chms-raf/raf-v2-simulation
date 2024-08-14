using UnityEngine;
public class RobotJoint : MonoBehaviour
{
    public Vector3 Axis;
    public Vector3 StartOffset;
    public Vector3 Angles;
    private Transform _transform;
    void Awake () {
        _transform = this.transform;
        //StartOffset = _transform.localPosition;
        //Angles = _transform.localRotation.eulerAngles;
        StartOffset = _transform.GetComponent<ArticulationBody>().parentAnchorPosition;
    }

    void Update() {
        Angles = _transform.localRotation.eulerAngles;
        //Angles = _transform.GetComponent<ArticulationBody>().anchorRotation.eulerAngles;
    }
}