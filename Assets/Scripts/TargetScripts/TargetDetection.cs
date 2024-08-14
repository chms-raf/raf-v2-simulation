/// |-------------------------------------------TargetDetection-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class determines whether the target object is within the Camera's viewpoint or not.
/// |-------------------------------------------------------------------------------------------------------------|

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class TargetDetection : MonoBehaviour {
    // Accessible Properties
    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject rendText;

    // Private variables
    private Camera _camera;
    private MeshRenderer _renderer;
    private Plane[] cameraFrustum;
    private Collider _collider;
    private GameObject _canvas;
    private GameObject _body;
    private GameObject _border;
    private GameObject _text;
    private GameObject _textBG;
    private GameObject _button;

    // Start is called before the first frame update
    void Start() {
        // Get all the required variables
        _camera = Camera.allCameras[1];
        _renderer = GetComponentInChildren<MeshRenderer>();
        _collider = GetComponentInChildren<Collider>();
        _canvas = this.transform.Find("Canvas").gameObject;
        _body = this.transform.Find("Body").gameObject;
        _border = _canvas.transform.Find("Border").gameObject;
        _text = _border.transform.Find("Text").gameObject;
        _textBG = _border.transform.Find("TextBackground").gameObject;
        _button = this.transform.Find("Button").gameObject;


        // Set the text to the name of the object
        _text.GetComponent<TextMeshProUGUI>().SetText(this.gameObject.name);
    }

    // Update is called once per frame
    void FixedUpdate() {
        // Get the variables needed for every update
        var bounds = _collider.bounds;
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(_camera);
        bool insideCam = true;

        // Iterate through and make sure every 75% of the vertices are within the camera frustum
        for (int i = 0; i < cameraFrustum.Length; i++) {
            bool A = cameraFrustum[i].GetSide(bounds.center);
            bool B = cameraFrustum[i].GetSide(bounds.min);
            bool C = cameraFrustum[i].GetSide(bounds.max);
            bool D = cameraFrustum[i].GetSide(bounds.center + (bounds.extents/2));
            bool E = cameraFrustum[i].GetSide(bounds.center - (bounds.extents/2));
            insideCam = insideCam && A && (A&B&C | A&B&D | A&C&D | A&C&D);
        }

        // If it is then update the bounding box
        if (insideCam) {
            // Focus the border on the screen to the bounds
            FocusOnBounds(bounds);

            // Show the border
            _canvas.SetActive(true);

            // Show the button
            ButtonOnUI();
        } else {
            // Hide the border
            _canvas.SetActive(false);

            // Remove the button from UI
            RemoveButton();
        }
    }

    // Focus the bounds of the selection box onto the target object
    public void FocusOnBounds(Bounds bounds) {
        // Get the center and extents of the bounds
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;
 
        // Calculate all 8 vertices of the box bounds
        Vector3[] worldCorners = new [] {
            new Vector3( c.x + e.x, c.y + e.y, c.z + e.z ),
            new Vector3( c.x + e.x, c.y + e.y, c.z - e.z ),
            new Vector3( c.x + e.x, c.y - e.y, c.z + e.z ),
            new Vector3( c.x + e.x, c.y - e.y, c.z - e.z ),
            new Vector3( c.x - e.x, c.y + e.y, c.z + e.z ),
            new Vector3( c.x - e.x, c.y + e.y, c.z - e.z ),
            new Vector3( c.x - e.x, c.y - e.y, c.z + e.z ),
            new Vector3( c.x - e.x, c.y - e.y, c.z - e.z ),
        };

        // Convert all 8 points to the screen points of the camera
        IEnumerable<Vector3> screenCorners = worldCorners.Select(corner => _camera.WorldToScreenPoint(corner));

        // Find the min and max for the X and Y
        float maxX = screenCorners.Max(corner => corner.x);
        float minX = screenCorners.Min(corner => corner.x);
        float maxY = screenCorners.Max(corner => corner.y);
        float minY = screenCorners.Min(corner => corner.y);

        // Get the rect transform of the border
        RectTransform rt = _border.GetComponent<RectTransform>();

        // Set the anchored position to the minX and minY (lower left corner of border)
        rt.anchoredPosition = new Vector2(minX,minY);

        // Set the size of the border to distance between Xs and Ys
        rt.sizeDelta = new Vector2(maxX - minX, maxY - minY);

        // Set the size of the text and text background
        _text.GetComponent<RectTransform>().sizeDelta = new Vector2(maxX - minX, _text.GetComponent<RectTransform>().sizeDelta.y);
        _textBG.GetComponent<RectTransform>().sizeDelta = new Vector2(maxX - minX, _textBG.GetComponent<RectTransform>().sizeDelta.y);
        
        // Set the Z distance to the distance between the camera and the target object
        _canvas.GetComponent<Canvas>().planeDistance = (_body.transform.position - _camera.transform.position).magnitude;
    }

    // Transfer the button to the UI with the proper sizing
    public void ButtonOnUI() {
        // Make the parent of the button be the rendered texture
        _button.transform.SetParent(rendText.transform, false);

        // Move the position of the button
        Vector2 screenPoint = _camera.ScreenToViewportPoint(_border.GetComponent<RectTransform>().localPosition);
        Vector2 localPoint = new Vector2((screenPoint.x+0.5f) * rendText.GetComponent<RectTransform>().rect.width, 
                                         (screenPoint.y+0.5f) * rendText.GetComponent<RectTransform>().rect.height);

        _button.GetComponent<RectTransform>().anchoredPosition = localPoint;

        // Resize the button
        Vector2 size = new Vector2(_border.GetComponent<RectTransform>().sizeDelta.x/_camera.pixelWidth  * rendText.GetComponent<RectTransform>().rect.width, 
                                   _border.GetComponent<RectTransform>().sizeDelta.y/_camera.pixelHeight * rendText.GetComponent<RectTransform>().rect.height);

        _button.GetComponent<RectTransform>().sizeDelta = size;

        // Set it active
        _button.SetActive(true);

    }

    // Remove the button from the UI
    public void RemoveButton() {
        // Make the parent be the target prefab
        _button.transform.SetParent(this.transform, false);

        // Set it not active
        _button.SetActive(false);
    }
}
