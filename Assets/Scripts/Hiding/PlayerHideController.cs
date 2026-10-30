using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerHideController : NetworkBehaviour
{
    [Header("Object Detection")]
    [SerializeField] private float interactionRange = 4f;

    private Transform possessableRoot;
    private Camera playerCamera;
    private TMP_Text promptText;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        FindReferences();
    }

    private void FindReferences()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        if (possessableRoot == null)
        {
            PossessableRoot root =
                FindAnyObjectByType<PossessableRoot>();

            if (root != null)
            {
                possessableRoot = root.transform;
            }
        }

        if (promptText == null)
        {
            promptText = InteractionPromptAnchor.Text;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        FindReferences();

        if (possessableRoot == null ||
            playerCamera == null ||
            promptText == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
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
            }
            else
            {
                promptText.text = "✕ Cannot hide here";
            }
        }
        else
        {
            promptText.text = "";
        }
    }
}