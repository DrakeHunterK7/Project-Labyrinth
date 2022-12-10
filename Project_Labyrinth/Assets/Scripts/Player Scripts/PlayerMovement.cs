using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour, IDamageable
{
    public CharacterController controller;
    public CapsuleCollider playerCollider;
    
    [Header("General Parameters")]
    [SerializeField] public Camera mainCam;
    [SerializeField] public InventoryManager inventoryManager;
    [SerializeField] public Light collarLight;

    private Item selectedItem;
    
    [Header("Player Model Parameters")]
    [SerializeField] private GameObject playerModel;

    [SerializeField] public Transform equippedObjectLocation;

    [Header("Player Parameters")] 
    [SerializeField] public float health = 100f;

    public bool isEquipped = false;

    private float camXRotation;
    private float camYRotation;
    private float cameraSpeed = 2f;
    private float FPSRayRange = 15f;

    private bool isInvOpen = false;

 
    private float walkSpeed = 10f;
    private float sprintSpeed = 22f;
    private float crouchSpeed = 5f;
    private bool canCrouch = true;
    private float activeSpeed;
    public float speedBoostMultiplier = 1.0f;

    private bool isWalking;
    private bool isSprinting;
    private bool isCrouching;
    public bool isSpotted = false;

    [HideInInspector]
    public bool smelly = false;

    private float soundRadius = 0f;
    
    [Header("Crouch Parameters")]
    [SerializeField] private float crouchTime = 0.25f;
    [SerializeField] private float crouchHeight = 4f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 1.5f, 0);
        

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
    public Vector3 rayHitLocation;


    [Header("Audio Parameters")]
    [SerializeField] private AudioSource walkSound;
    [SerializeField] private AudioSource sprintSound;
    [SerializeField] private AudioSource heartBeatSound;
    [SerializeField] private AudioSource breatheSound;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] public AudioSource levelMusic;
    [SerializeField] public AudioSource chaseMusic;
    [SerializeField] private AudioSource pickupSound;
    [SerializeField] private AudioSource damageSound;


    private Vector3 velocity = Vector3.zero;

    private bool isPlayerLeaning = false;
    
    private Vector3 gravity;
    
    ItemPickup hoveredPickup;
    public GameObject interactable;
    private Image crosshair;
    private GameObject itemBox;
    private RectTransform itemBox_transform;
    private Text itemBox_text;
    [SerializeField] private RectTransform exitIcon;
    [SerializeField] private UnityEngine.UI.Image bloodOverlay;
    [SerializeField] private UnityEngine.UI.Image fadeOverlay;
    [SerializeField] private UnityEngine.UI.Image deathOverlay;
    [SerializeField] private UnityEngine.UI.Image deathBloodOverlay;
    private float overlayDisappearTime = 0f;
    public float fadeTime = 0f;
    public float deathFadeTime = 0f;
    public float deathFadeTime1 = 0f;
    public int fadeDirection = 1;

    
    [SerializeField] private Button quitgameBtn;
    [SerializeField] private Button mainmenuBtn;
    [SerializeField] private TextMeshProUGUI deathTitle;
    [SerializeField] private TextMeshProUGUI deathSubtitle;

    private bool deathTitleShow = false;
    private bool deathSubtitleShow = false;

    private GameObject exitPointer;
    

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
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
        levelMusic = GameObject.FindWithTag("LevelMusic").GetComponent<AudioSource>();
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameDirector.instance.NextLevel();
        }
        
        if (exitPointer != null)
        {
            var exitPosition = mainCam.WorldToScreenPoint(exitPointer.transform.position);
            exitIcon.position = exitPosition;
        }
        else
        {
            exitPointer = GameObject.FindWithTag("ExitPoint");
        }
        
        
        if (Input.GetKey(KeyCode.T))
        {
            exitIcon.gameObject.SetActive(true);
        }
        else
        {
            exitIcon.gameObject.SetActive(false);
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            smelly = false;
        }

        if (deathTitleShow)
        {
            deathFadeTime += Time.deltaTime;
            deathTitle.color = Color.Lerp(Color.clear, Color.red, deathFadeTime / 3f);
        }

        if (deathSubtitleShow)
        {
            deathFadeTime1 += Time.deltaTime;
            deathSubtitle.color = Color.Lerp(Color.clear, Color.white, deathFadeTime1 / 3f);
        }
            
        

        if (GameDirector.instance.isPlayerDead) return;
        
        if(overlayDisappearTime > 0)
            overlayDisappearTime -= Time.deltaTime;
        bloodOverlay.color = Color.Lerp(Color.clear, Color.white, overlayDisappearTime / 3f);
        
        if(fadeTime is >= 0 and <= 2)
            fadeTime += fadeDirection * Time.deltaTime;
        fadeOverlay.color = Color.Lerp(Color.black, Color.clear, fadeTime / 2f);

        playerCollider.center = controller.center;
        playerCollider.height = controller.height;

        if (GameDirector.instance.isLoadingNextLevel) return; 
        
        if (!isPlayerLeaning)
        {
            float x = (Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0);
            float y = (Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0);

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            camXRotation -= mouseY * cameraSpeed;
            camYRotation += mouseX * cameraSpeed;
            camXRotation = Mathf.Clamp(camXRotation, -90, 90);

            if(!Pause_Menu.isPaused)
            {
                mainCam.transform.eulerAngles = new Vector3(camXRotation, camYRotation, 0f);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            

            gravity = Vector3.zero;
            if (controller.isGrounded == false && canCrouch)
            {
                gravity += Physics.gravity;
            }

            var forwardDirectionVector = mainCam.transform.forward;

            Vector3 move = Vector3.ClampMagnitude(forwardDirectionVector * y + mainCam.transform.right * x, 1);
            move.y = 0;
            controller.Move(((move.normalized * activeSpeed * speedBoostMultiplier ) + (gravity)) * Time.deltaTime);

            HeadBobHandler(move);
            
            if (Input.GetKeyDown(KeyCode.C) && canCrouch)
            {
                if (!isCrouching)
                {
                    isCrouching = true;
                    isWalking = false;
                    isSprinting = false;
                    StartCoroutine(HandleCrouch());
                }
                else
                {
                    isCrouching = false;
                    isWalking = true;
                    StartCoroutine(HandleCrouch());
                }
            }
        }

        FPSRay();
        UpdateCollarLight();

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            PickupItem();
            
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
            UseItem();

        if (Input.GetKeyDown(KeyCode.Mouse2))
            DropItem();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isWalking = false;
            isSprinting = true;
            if (isCrouching)
            {
                isCrouching = false;
                StartCoroutine(HandleCrouch());
            }
            else
            {
                isCrouching = false;
            }
            
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isWalking = true;
            isSprinting = false;
            isCrouching = false;
        }

        

        if (isCrouching)
            activeSpeed = crouchSpeed;
        else if (isWalking)
        {
            soundRadius = 50f;
            activeSpeed = walkSpeed; 
        }
        else if (isSprinting)
        {
            soundRadius = 150f;
            activeSpeed = sprintSpeed;
        }
            
        else
            activeSpeed = walkSpeed;




        if (Input.GetKey(KeyCode.E))
            PlayerLean(1);
        else if (Input.GetKey(KeyCode.Q))
            PlayerLean(-1);
        else
        {
            if (isPlayerLeaning)
            {
                mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, cameraInitialPosition,
                    ref velocity, 0.1f);

                if (Vector3.Distance(mainCam.transform.position, cameraInitialPosition) < 0.25f)
                {
                    isPlayerLeaning = false;
                }
            }
            else
            {
                cameraInitialPosition = mainCam.transform.position;
            }
        }

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 && !isSprinting)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        if (isWalking && !isCrouching)
        {
            if (!walkSound.isPlaying)
            {
                walkSound.Play();
            }
        }
        else
        {
            walkSound.Stop();
        }

        if (isSprinting)
        {
            if (!sprintSound.isPlaying)
            {
                sprintSound.Play();
            }
        }
        else
        {
            sprintSound.Stop();
        }

        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                if (!heartBeatSound.isPlaying && !breatheSound.isPlaying)
                {
                    heartBeatSound.Play();
                    breatheSound.Play();
                    StartCoroutine(enemySpotted());
                }
            }
        }
        
        CreateSound();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }

    public void CreateSound()
    {
        if (isCrouching || controller.velocity.magnitude < walkSpeed-5f) return;
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, soundRadius);
        foreach(var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                foreach (MonoBehaviour script in collider.gameObject.GetComponents<MonoBehaviour>())
                {
                    if (script is IHearing targetScript)
                    {
                        targetScript.HeardSound(transform.position);
                    }
                }
            }
        }
    }

    private IEnumerator enemySpotted()
    {
        yield return new WaitForSeconds(5);
        heartBeatSound.Stop();
        breatheSound.Stop();
        yield break;
    }

    private IEnumerator HandleCrouch()
    
    {
        canCrouch = false;

        float currentCrouchTime = 0f;

        float targetHeight = isCrouching ? crouchHeight : 8f;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? crouchCenter: new Vector3(0f, 0f, 0f);
        Vector3 currentCenter = controller.center;
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(transform.position.x,
            transform.position.y + (targetHeight - currentHeight), transform.position.z);

        while (currentCrouchTime < crouchTime)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, currentCrouchTime / crouchTime);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, currentCrouchTime / crouchTime);
            
            transform.position = Vector3.Lerp(currentPosition, targetPosition, currentCrouchTime / crouchTime);
            currentCrouchTime += Time.deltaTime;

            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;
        
        canCrouch = true;
    }
    
    public void Damage(float damage)
    {
        if (GameDirector.instance.isLoadingNextLevel || GameDirector.instance.isPlayerDead) return;
        
        damageSound.Play();
        health -= damage;
        overlayDisappearTime = 3f;
        if(health <= 0)
            Death();
    }

    public bool HealthRestore(float amount)
    {
        if (health + amount <= 100)
        {
            health += amount;
            return true;
        }
        else if (health - 100 < 0.1f)
        {
            MessageManager.instance.DisplayMessage("Health is already full!", Color.yellow);
            return false;
        }
        else if (100-health < amount)
        {
            health += (100 - health);
            return true;
        }
        
        else return false;
    }
    

    void Death()
    {
        GameDirector.instance.isPlayerDead = true;
        deathOverlay.gameObject.SetActive(true);
        walkSound.Stop();
        sprintSound.Stop();
        breatheSound.Stop();
        heartBeatSound.Stop();
        deathSound.Play();
        StartCoroutine("StartDeathAnimation");
    }

    void UpdateCollarLight()
    {
        collarLight.color = Color.Lerp(Color.red, Color.white, health / 100f);
        collarLight.intensity = Mathf.Lerp(3.5f, 1, health/100f);
    }
    
    void FPSRay()
    {
        if (isEquipped) return;
        
        
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, FPSRayRange))
        {
            rayHitLocation = hit.point;
            hoveredPickup = hit.collider.gameObject.GetComponent<ItemPickup>();
            if (hit.collider.CompareTag("Interactable"))
            {
                interactable = hit.collider.gameObject;
            }
            
            
            if (hoveredPickup != null)
            {
                itemBox.transform.localScale = new Vector2 (1f, 1f);
                crosshair.color = Color.yellow;
                itemBox_text.text = hoveredPickup.Item.itemName;
                var itemPositionWorldToCanvas = mainCam.WorldToScreenPoint(hoveredPickup.transform.position);
                itemBox_transform.position = itemPositionWorldToCanvas;
            }
            else if (interactable != null)
            {
                itemBox.transform.localScale = new Vector2 (1f, 1f);
                crosshair.color = Color.green;
                itemBox_text.text = interactable.name;
                itemBox_transform.position = new Vector3(Screen.width/2,Screen.height/2, 0);
                // var itemPositionWorldToCanvas = mainCam.WorldToScreenPoint(interactable.transform.position);
                // itemBox_transform.position = itemPositionWorldToCanvas;
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
            rayHitLocation = mainCam.transform.position + mainCam.transform.forward * FPSRayRange;
            hoveredPickup = null;
            itemBox.transform.localScale = Vector2.zero;
            itemBox_text.text = "";
            crosshair.color = Color.white;
            interactable = null;
            
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
        if (interactable != null)
        {
            interactable.GetComponent<Interactable>()?.DoAction();
        }
        else if (hoveredPickup != null)
        {
            if (inventoryManager.Add(hoveredPickup.Item))
            {
                Destroy(hoveredPickup.gameObject);
                pickupSound.Play();
            }
            else
            {
                MessageManager.instance.DisplayMessage("Inventory is Full!", Color.yellow);
            }
        }
    }
    
    void UseItem()
    {
        if (inventoryManager.selectedItem != null)
        {
            inventoryManager.Use();
        }
        
    }

    public void EquipItem(GameObject itemToEquip)
    {
        var equippedItem = Instantiate(itemToEquip, equippedObjectLocation.position, Quaternion.identity);
        equippedItem.transform.SetParent(equippedObjectLocation);
        isEquipped = true;

        var rb = equippedItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    public void UnEquipItem()
    {
        if(equippedObjectLocation.childCount > 0)
            Destroy(equippedObjectLocation.GetChild(0).gameObject);
        isEquipped = false;
    }

    public IEnumerator RestoreVariable(PlayerAttributeAffected attribute, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        switch (attribute)
        {
            case PlayerAttributeAffected.Health:
                health = 100f;
                break;
            case PlayerAttributeAffected.Stamina:
                break;
            case PlayerAttributeAffected.MovementSpeed:
                speedBoostMultiplier = 1.0f;
                break;
        }
    }
    
    
    
    void DropItem()
    {
        if (inventoryManager.selectedItem != null)
        {
            inventoryManager.Drop();
        }
    }

    void PlayerLean(int direction)
    {
        if (direction != 0)
        {
            isPlayerLeaning = true;
            var newPosition = cameraInitialPosition + direction * mainCam.transform.right * 2f;
            mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, newPosition, ref velocity, 0.1f);
        }
    }

    IEnumerator StartDeathAnimation()
    {
       Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return new WaitForSeconds(5f);
        deathTitleShow = true;
        yield return new WaitForSeconds(5f);
        deathSubtitleShow = true;
        yield return new WaitForSeconds(5f);

        if (GameDirector.instance.gameLevel == 2)
        {
            SceneManager.LoadScene("Bad Ending");
        }
        else
        {
            quitgameBtn.gameObject.SetActive(true);
            mainmenuBtn.gameObject.SetActive(true);
        }
        
    }

    public void LoadMainMenu()
    {
        Destroy(this.gameObject);
        Destroy(GameDirector.instance.gameObject);
        SceneManager.LoadScene("Main_Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
