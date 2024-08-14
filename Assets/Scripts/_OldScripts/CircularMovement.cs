using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMovement : MonoBehaviour
{

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // angle += (moveSpeed / (radius * Mathf.PI * 2.0f)) * Time.deltaTime;
        // transform.position = new Vector3(Mathf.Cos(angle), 0.3f * Mathf.Sin(Time.timeSinceLevelLoad) + 1, Mathf.Sin(angle)) * radius;

        Transform _transform = this.transform;
        Vector3 Movement = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.A))
            Movement += (Vector3.left * 0.05f);
        if (Input.GetKey(KeyCode.D))
            Movement += (Vector3.right * 0.05f);
        if (Input.GetKey(KeyCode.W))
            Movement += (Vector3.forward * 0.05f);
        if (Input.GetKey(KeyCode.S))
            Movement += (Vector3.back * 0.05f);
        if (Input.GetKey(KeyCode.Q))
            Movement += (Vector3.up * 0.05f);
        if (Input.GetKey(KeyCode.F))
            Movement += (Vector3.down * 0.05f);
        // else
        //     rb.velocity = Vector3.zero;

        rb.velocity = Movement;
        Movement = Vector3.zero;

        // if (Input.GetKey(KeyCode.A))
        //     Movement += Vector3.left;
        // else if (Input.GetKey(KeyCode.D))
        //     Movement += Vector3.right;
        // else if (Input.GetKey(KeyCode.W))
        //     Movement += Vector3.forward;
        // else if (Input.GetKey(KeyCode.S))
        //     Movement += Vector3.back;
        // else if (Input.GetKey(KeyCode.Q))
        //     Movement += Vector3.up;
        // else if (Input.GetKey(KeyCode.F))
        //     Movement += Vector3.down;
        
        // _transform.position = Vector3.Lerp(_transform.position, _transform.position + (Movement * 0.001f), 0.5f);
    }
}
