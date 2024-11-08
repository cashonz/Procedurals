using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 100f;
    public float sensitivity = 3f;
    public Transform cameraTransform;
    private float rotY = 0;
    private float rotX = 0;
    public bool lockMouse = true;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        if(lockMouse)
        {
            rotY += Input.GetAxis("MouseX") * sensitivity;
            rotX += Input.GetAxis("MouseY") * - 1 * sensitivity;

            cameraTransform.localEulerAngles = new Vector3(rotX, rotY, 0);
        }
        
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += cameraTransform.forward * moveSpeed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.S))
        {
            transform.position += -cameraTransform.forward * moveSpeed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.D))
        {
            transform.position += cameraTransform.right * moveSpeed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.A))
        {
            transform.position += -cameraTransform.right * moveSpeed * Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(lockMouse)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            lockMouse = !lockMouse;
        }
    }
}
