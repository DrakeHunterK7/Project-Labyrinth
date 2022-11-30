using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    
    [SerializeField] public Camera mainCam;
    [SerializeField] private GameObject playerModel;

    private float camXRotation;
    private float camYRotation;
    private float cameraSpeed = 2f;
    private float FPSRayRange = 100f;

    private bool isInvOpen = false;

    public float speed = 120f;

    private Vector3 gravity;
    
    ItemPickup hoveredPickup;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        camXRotation -= mouseY*cameraSpeed;
        camYRotation += mouseX*cameraSpeed;
        camXRotation = Mathf.Clamp(camXRotation, -90, 90);

        mainCam.transform.eulerAngles = new Vector3(camXRotation, camYRotation, 0f);
            
        gravity = Vector3.zero;
        if (controller.isGrounded == false)
        {
            gravity += Physics.gravity;
        }
        
        Vector3 move = (mainCam.transform.forward * y + mainCam.transform.right * x);    

        controller.Move(((move * speed) + (gravity)) * Time.deltaTime);   
        
        FPSRay();
        
        if(Input.GetKeyDown(KeyCode.Mouse1))
            PickupItem();
        
        if(Input.GetKey(KeyCode.E))
            PlayerTilt(-1);
        else if(Input.GetKey(KeyCode.Q))
            PlayerTilt(1);
        
    }


    void FPSRay()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, FPSRayRange))
        {
            //Debug.Log(hit.collider.gameObject.name);
            hoveredPickup = hit.collider.gameObject.GetComponent<ItemPickup>();
        }
        else
        {
            hoveredPickup = null;
        }
    }

    void PickupItem()
    {
        if (hoveredPickup != null)
        {
            hoveredPickup.Pickup();
        }
    }

    void PlayerTilt(int direction)
    {
        var currentTime = Time.time;
        playerModel.transform.localRotation = Quaternion.Lerp(
            Quaternion.Euler(0,0,0), 
            Quaternion.Euler(0,0, direction*30), 
            currentTime/ (Time.time - currentTime) );
        
    }

    void OpenInventory()
    {
        if (!isInvOpen)
        {
            isInvOpen = true;
        }
    }
}
