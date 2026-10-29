using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Player Configuration")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;

    private CharacterController characterController;
    private IPlayerInputSource inputSource;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool cursorLocked = true;
    public bool IsCursorLocked => cursorLocked;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputSource = GetComponent<IPlayerInputSource>();

        if (playerStats == null)
        {
            Debug.LogError(
                "PlayerMovement: PlayerStats is not assigned.",
                this
            );
        }
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
        
        if(playerStats == null || inputSource == null || cameraPivot == null )
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
            : playerStats.AirControl;

        float rate = hasMovementInput
            ? playerStats.Acceleration
            : playerStats.Deceleration;

        Vector3 targetVelocity =
            moveDirection * playerStats.MoveSpeed * control;

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
                    playerStats.JumpHeight * -2f * playerStats.Gravity
                );
            }
        }

        verticalVelocity += playerStats.Gravity * Time.deltaTime;

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

        float mouseX = lookInput.x * playerStats.MouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

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