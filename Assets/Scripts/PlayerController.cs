using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    
    CharacterController characterController;
    Vector3 moveDir = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    [Header("Animator setup")]
    public Animator anim;

    [Header("Multiplayer Camera Setup")]
    [SerializeField] private Camera localNestedCamera;

    [Header("Input System Setup")]
    [SerializeField] private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool jumpTriggered;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            
            if (localNestedCamera != null)
            {
                localNestedCamera.enabled = true;
                localNestedCamera.transform.localPosition = new Vector3(0, cameraYOffset, 0);
            }
            
            if(playerInput != null) playerInput.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if(localNestedCamera != null) localNestedCamera.enabled = false;
            this.enabled = false;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }
    public void OnJump(InputValue value)
    {
        jumpTriggered = value.isPressed;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float speedMultiplier = isSprinting ? runningSpeed : walkingSpeed;
        float curSpeedX = canMove ? speedMultiplier * moveInput.y : 0;
        float curSpeedY = canMove ? speedMultiplier * moveInput.x : 0;
        float movementDirectionY = moveDir.y;
        moveDir = (forward * curSpeedX) + (right * curSpeedY);
 
        if (jumpTriggered && canMove && characterController.isGrounded)
        {
            moveDir.y = jumpSpeed;
            jumpTriggered = false;
        }
        else
        {
            moveDir.y = movementDirectionY;
        }
 
        if (!characterController.isGrounded)
        {
            moveDir.y -= gravity * Time.deltaTime;
        }
 
        // Move the controller
        characterController.Move(moveDir * Time.deltaTime);
 
        // Player and Camera rotation
        if (canMove && localNestedCamera != null)
        {
            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            localNestedCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
        }
    }
}