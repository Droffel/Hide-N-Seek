using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float airControl = 0.5f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 85f;

    private CharacterController characterController;
    private IPlayerInputSource inputSource;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float cameraPitch;

    private bool cursorLocked = true;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputSource = GetComponent<IPlayerInputSource>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (cameraPivot != null)
            {
                Camera playerCamera =
                    cameraPivot.GetComponentInChildren<Camera>(true);

                if (playerCamera != null)
                {
                    playerCamera.gameObject.SetActive(false);
                }
            }

            return;
        }

        LockCursor();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (inputSource == null || cameraPivot == null)
            return;

        HandleCursorInput();
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 input = inputSource.MoveInput;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDirection =
            forward * input.y + right * input.x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        bool hasMovementInput = input.sqrMagnitude > 0.01f;

        float control = characterController.isGrounded
            ? 1f
            : airControl;

        float rate = hasMovementInput
            ? acceleration
            : deceleration;

        Vector3 targetVelocity =
            moveDirection * moveSpeed * control;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            rate * Time.deltaTime
        );

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (inputSource.JumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            horizontalVelocity +
            Vector3.up * verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (!cursorLocked)
            return;

        Vector2 lookInput = inputSource.LookInput;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        cameraPivot.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleCursorInput()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }

        if (!cursorLocked &&
            mouse != null &&
            mouse.leftButton.wasPressedThisFrame)
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            UnlockCursor();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!IsOwner)
            return;

        if (hasFocus && cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!hasFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}