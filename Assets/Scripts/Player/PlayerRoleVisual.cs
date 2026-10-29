using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRoleVisuals : NetworkBehaviour
{
    [SerializeField] private GameObject hiderVisual;
    [SerializeField] private GameObject seekerVisual;

    private PlayerRole playerRole;

    public override void OnNetworkSpawn()
    {
        playerRole = GetComponent<PlayerRole>();

        if (playerRole == null)
        {
            Debug.LogError("PlayerRoleVisuals: PlayerRole component not found");
            return;
        }

        playerRole.OnRoleChanged += UpdateVisuals;

        UpdateVisuals(playerRole.CurrentRole);
    }

    private void UpdateVisuals(PlayerRoleType role)
    {
        bool isSeeker = role == PlayerRoleType.Seeker;

        seekerVisual.SetActive(isSeeker);
        hiderVisual.SetActive(!isSeeker);

    }
}