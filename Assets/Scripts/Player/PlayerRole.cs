using System;
using UnityEngine;
using Unity.Netcode;


public enum PlayerRoleType
{
    Hider,
    Seeker
}

public class PlayerRole : NetworkBehaviour
{
    private NetworkVariable<PlayerRoleType> role =
        new NetworkVariable<PlayerRoleType>(
            PlayerRoleType.Hider,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
    );
    public event Action<PlayerRoleType> OnRoleChanged;

    public PlayerRoleType CurrentRole => role.Value;

    public bool IsSeeker => role.Value == PlayerRoleType.Seeker;

    public override void OnNetworkSpawn()
    {
        role.OnValueChanged += HandleRoleChanged;

        OnRoleChanged?.Invoke(role.Value);
    }

    public override void OnNetworkDespawn()
    {
        role.OnValueChanged -= HandleRoleChanged;
    }
    private void HandleRoleChanged(
        PlayerRoleType previousRole,
        PlayerRoleType newRole)
    {
        OnRoleChanged?.Invoke(newRole);
    }

    public void SetRole(PlayerRoleType newRole)
    {
        if(!IsServer)
            return;

            role.Value = newRole;
    }
    
}
