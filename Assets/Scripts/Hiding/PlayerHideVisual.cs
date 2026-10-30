using UnityEngine;
using Unity.Netcode;

public class PlayerHideVisibility : NetworkBehaviour
{
    [SerializeField] private GameObject hiderVisual;

    private PlayerRole playerRole;
    private PlayerHideController hideController;

    public override void OnNetworkSpawn()
    {
        playerRole = GetComponent<PlayerRole>();
        hideController = GetComponent<PlayerHideController>();

        if (playerRole == null || hideController == null)
        {
            Debug.LogError(
                "PlayerHideVisibility: Required components missing.",
                this
            );

            return;
        }

        playerRole.OnRoleChanged += UpdateVisibility;
        hideController.OnHidingChanged += UpdateHidingVisibility;

        UpdateVisibility(playerRole.CurrentRole);
        UpdateHidingVisibility(hideController.IsHiding);
    }

    private void UpdateVisibility(PlayerRoleType role)
    {
        UpdateHidingVisibility(hideController.IsHiding);
    }

    private void UpdateHidingVisibility(bool isHiding)
    {
        if (hiderVisual == null || playerRole == null)
        {
            return;
        }

        bool isHider =
            playerRole.CurrentRole == PlayerRoleType.Hider;

        hiderVisual.SetActive(isHider && !isHiding);
    }

    public override void OnNetworkDespawn()
    {
        if (playerRole != null)
        {
            playerRole.OnRoleChanged -= UpdateVisibility;
        }

        if (hideController != null)
        {
            hideController.OnHidingChanged -= UpdateHidingVisibility;
        }
    }
}