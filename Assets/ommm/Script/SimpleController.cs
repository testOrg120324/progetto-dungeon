using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{

    public CharacterController controller;
    public Transform cam;

    public float velocity = 5;

    public float rotationVelocity = 180;
    public float upDownVelocity = 30;

    float currentAngle=0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        controller.SimpleMove(controller.transform.forward * Input.GetAxis("Vertical") * velocity

            + controller.transform.right * Input.GetAxis("Horizontal") * velocity

            );

        controller.transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * rotationVelocity * Time.deltaTime);

        cam.transform.localRotation = Quaternion.Euler(currentAngle, 0, 0);

        currentAngle += Input.GetAxis("Mouse Y") * upDownVelocity * Time.deltaTime;

        currentAngle = Mathf.Clamp(currentAngle, -80, 80);
    }
}
