using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    
    [SerializeField] public Camera mainCam;
    [SerializeField] public GameObject camLeftLean;
    [SerializeField] public GameObject camRightLean;
    
    [SerializeField] private GameObject playerModel;

    private float camXRotation;
    private float camYRotation;
    private float cameraSpeed = 2f;
    private float FPSRayRange = 10f;

    private bool isInvOpen = false;

    private float walkSpeed = 10f;
    private float sprintSpeed = 20f;
    private float crouchSpeed = 5f;
    private float activeSpeed;

    private bool isWalking;
    private bool isSprinting;
    private bool isCrouching;

        [Header("HeadBob Parameters")]
    [SerializeField] private float crouchHeadBobAmount = 0.05f;
    [SerializeField] private float crouchHeadBobSpeed = 4f;
    [SerializeField] private float walkHeadBobAmount = 0.05f;
    [SerializeField] private float walkHeadBobSpeed = 6f;
    [SerializeField] private float sprintHeadBobAmount = 0.1f;
    [SerializeField] private float sprintHeadBobSpeed = 7f;
    private float defaultYCameraPosition;
    private float headbobTimer;
    private Vector3 cameraPosition;
    private Vector3 cameraInitialPosition;
    private Quaternion cameraInitialRotation;

    private Vector3 velocity = Vector3.zero;

    private bool isPlayerLeaning = false;
    

    private Vector3 gravity;
    
    ItemPickup hoveredPickup;
    private Image crosshair;
    private GameObject itemBox;
    private RectTransform itemBox_transform;
    private Text itemBox_text;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        activeSpeed = walkSpeed;
        crosshair = GameObject.FindWithTag("Crosshair").GetComponent<Image>();
        itemBox = GameObject.FindWithTag("ItemUIBox");
        itemBox_transform = itemBox.GetComponent<RectTransform>();
        itemBox_text = GameObject.FindWithTag("ItemUIText").GetComponent<Text>();

        defaultYCameraPosition = mainCam.transform.localPosition.y;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!isPlayerLeaning)
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

            Vector3 move = Vector3.ClampMagnitude(mainCam.transform.forward * y + mainCam.transform.right * x, 1);
            controller.Move(((move * activeSpeed) + (gravity)) * Time.deltaTime);
            
            HeadBobHandler(move);

        }
        
        FPSRay();
        if(Input.GetKeyDown(KeyCode.Mouse1))
            PickupItem();
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            activeSpeed = sprintSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            activeSpeed = walkSpeed;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isCrouching)
            {
                controller.height = 3f;
                activeSpeed = crouchSpeed; 
            }
            else
            {
                transform.position += new Vector3(0, 5f, 0f);
                controller.height = 7f;
                activeSpeed = walkSpeed;
            }

        }

        if (activeSpeed == walkSpeed)
        {
            isWalking = true;
            isSprinting = false;
            isCrouching= false;
        }
        else if (activeSpeed == sprintSpeed)
        {
            isWalking = false;
            isSprinting = true;
            isCrouching = false;
        }
        else if (activeSpeed == crouchSpeed)
        {
            isWalking = false;
            isSprinting = false;
            isCrouching = true;
        }
        
        
        
        if(Input.GetKey(KeyCode.E))
            PlayerLean(1);
        else if(Input.GetKey(KeyCode.Q))
            PlayerLean(-1);
        else
        {
            cameraInitialPosition = mainCam.transform.position;
            isPlayerLeaning = false;
        }

    }

    private void LateUpdate()
    {
        
    }


    void FPSRay()
    {
        Debug.Log(itemBox_transform.position);
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, FPSRayRange))
        {
            hoveredPickup = hit.collider.gameObject.GetComponent<ItemPickup>();
            if (hoveredPickup != null)
            {
                itemBox.transform.localScale = new Vector2 (1f, 1f);
                crosshair.color = Color.yellow;
                itemBox_text.text = hoveredPickup.Item.itemName;
                var itemPositionWorldToCanvas = mainCam.WorldToScreenPoint(hoveredPickup.transform.position);
                itemBox_transform.position = itemPositionWorldToCanvas;
            }
            
            else
            {
                itemBox_text.text = "";
                itemBox.transform.localScale = Vector2.zero;
                crosshair.color = Color.white;
            }
        }
        else
        {
            hoveredPickup = null;
            itemBox.transform.localScale = Vector2.zero;
            itemBox_text.text = "";
            crosshair.color = Color.white;
        }
    }

    void HeadBobHandler(Vector3 moveVector)
    {

        if (!controller.isGrounded) return;
        
        if (Mathf.Abs(moveVector.x) > 0.1f || Mathf.Abs(moveVector.z) > 0.1f)
        {
            headbobTimer += Time.deltaTime * (isCrouching ? crouchHeadBobSpeed :
                isWalking ? walkHeadBobSpeed :
                isSprinting ? sprintHeadBobSpeed : walkHeadBobSpeed);

            cameraPosition = new Vector3(
                mainCam.transform.localPosition.x,
                defaultYCameraPosition + Mathf.Sin(headbobTimer)  * (isCrouching ? crouchHeadBobAmount :
                    isWalking ? walkHeadBobAmount :
                    isSprinting ? sprintHeadBobAmount : walkHeadBobAmount) ,
                mainCam.transform.localPosition.z);

            mainCam.transform.localPosition = cameraPosition;
        }
    }

    void PickupItem()
    {
        if (hoveredPickup != null)
        {
            hoveredPickup.PickupItem();
        }
    }

    void PlayerLean(int direction)
    {
        if (direction != 0)
        {
            isPlayerLeaning = true;
            var newPosition = cameraInitialPosition + direction * mainCam.transform.right * 2f;
            mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, newPosition, ref velocity, 0.1f);

            var newRotation = Quaternion.Euler(mainCam.transform.rotation.x, mainCam.transform.rotation.y,
                mainCam.transform.rotation.z - direction*10f);
            mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, 
                newRotation, 
                20f * Time.deltaTime);
        }
    }

    void OpenInventory()
    {
        if (!isInvOpen)
        {
            isInvOpen = true;
        }
    }
}
