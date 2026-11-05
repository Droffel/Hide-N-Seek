using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerHideController : NetworkBehaviour
{
    [Header("Object Detection")]
    [SerializeField] private float interactionRange = 4f;

    [Header("Exit Settings")]
    [SerializeField] private float exitHeightOffset = 0.6f;

    [Header("Player Visuals")]
    [SerializeField] private GameObject playerVisualRoot;

    private Transform possessableRoot;
    private Camera playerCamera;
    private TMP_Text promptText;

    private PlayerRole playerRole;
    private PlayerMovement playerMovement;
    private CharacterController characterController;
    private PlayerHideCamera hideCamera;

    private Collider hiddenObjectCollider;
    private Renderer[] playerRenderers;

    private NetworkVariable<bool> isHiding =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool IsHiding => isHiding.Value;

    public event System.Action<bool> OnHidingChanged;

    public override void OnNetworkSpawn()
    {
        playerRole = GetComponent<PlayerRole>();
        playerMovement = GetComponent<PlayerMovement>();
        characterController = GetComponent<CharacterController>();
        hideCamera = GetComponent<PlayerHideCamera>();

        if (playerVisualRoot != null)
        {
            playerRenderers =
                playerVisualRoot.GetComponentsInChildren<Renderer>(true);
        }

        isHiding.OnValueChanged += HandleHidingChanged;

        UpdatePlayerVisibility(isHiding.Value);

        if (IsOwner)
        {
            FindReferences();
        }

        OnHidingChanged?.Invoke(isHiding.Value);
    }

    public override void OnNetworkDespawn()
    {
        isHiding.OnValueChanged -= HandleHidingChanged;
    }

    private void HandleHidingChanged(bool oldValue, bool newValue)
    {
        Debug.Log("HIDING STATE CHANGED: " + newValue, this);
        OnHidingChanged?.Invoke(newValue);
    }

    private void UpdatePlayerVisibility(bool hiding)
    {
        if (playerRenderers == null)
            return;

        foreach (Renderer renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = !hiding;
            }
        }
    }

    private void FindReferences()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);

            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        if (possessableRoot == null)
        {
            PossessableRoot root =
                FindAnyObjectByType<PossessableRoot>();

            if (root != null)
                possessableRoot = root.transform;
        }

        if (promptText == null)
        {
            promptText = InteractionPromptAnchor.Text;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        FindReferences();

        if (playerRole == null || playerRole.IsSeeker)
        {
            ClearPrompt();
            return;
        }

        if (isHiding.Value)
        {
            ShowLeavePrompt();

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                RequestLeaveServerRpc();
            }

            return;
        }

        if (possessableRoot == null ||
            playerCamera == null ||
            promptText == null ||
            Mouse.current == null)
        {
            ClearPrompt();
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            playerCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionRange))
        {
            bool isPossessable =
                HideSpot.IsHideable(hit.transform, possessableRoot);

            if (isPossessable)
            {
                promptText.text = "Press E to hide";

                if (Keyboard.current != null &&
                    Keyboard.current.eKey.wasPressedThisFrame)
                {
                    RequestHideServerRpc(
                        ray.origin,
                        ray.direction
                    );
                }
            }
            else
            {
                promptText.text = "✕ Cannot hide here";
            }
        }
        else
        {
            ClearPrompt();
        }
    }

    private void ClearPrompt()
    {
        if (promptText != null)
            promptText.text = "";
    }

    private void ShowLeavePrompt()
    {
        if (promptText != null)
            promptText.text = "Press E to leave";
    }

    [ServerRpc]
    private void RequestHideServerRpc(
        Vector3 rayOrigin,
        Vector3 rayDirection)
    {
        if (isHiding.Value)
            return;

        PlayerRole role = GetComponent<PlayerRole>();

        if (role == null ||
            role.CurrentRole != PlayerRoleType.Hider)
            return;

        PossessableRoot root =
            FindAnyObjectByType<PossessableRoot>();

        if (root == null)
            return;

        if (Vector3.Distance(rayOrigin, transform.position) > 5f)
            return;

        if (!Physics.Raycast(
            rayOrigin,
            rayDirection.normalized,
            out RaycastHit hit,
            interactionRange))
            return;

        if (!HideSpot.IsHideable(hit.transform, root.transform))
            return;

        hiddenObjectCollider = hit.collider;

        isHiding.Value = true;

        MoveOwnerClientRpc(hit.transform.position, true);
    }

    [ServerRpc]
    private void RequestLeaveServerRpc()
    {
        if (!isHiding.Value)
            return;

        PlayerRole role = GetComponent<PlayerRole>();

        if (role == null ||
            role.CurrentRole != PlayerRoleType.Hider)
            return;

        Vector3 exitPosition = transform.position;

        if (hiddenObjectCollider != null)
        {
            Bounds bounds = hiddenObjectCollider.bounds;

            exitPosition = new Vector3(
                bounds.center.x,
                bounds.max.y + exitHeightOffset,
                bounds.center.z
            );
        }

        isHiding.Value = false;

        hiddenObjectCollider = null;

        MoveOwnerClientRpc(exitPosition, false);
    }

    [ClientRpc]
    private void MoveOwnerClientRpc(
        Vector3 targetPosition,
        bool hiding)
    {
        if (!IsOwner)
            return;

        if (characterController != null)
            characterController.enabled = false;

        transform.position = targetPosition;

        if (characterController != null)
            characterController.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = !hiding;

        if (hideCamera != null)
            hideCamera.SetHiding(hiding);
    }
}