using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;

public class LobbyPlayer : NetworkBehaviour
{
    public event Action LobbyDataChanged;
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnPlayerNameChanged;
        IsReady.OnValueChanged += OnReadyChanged;
        
        if(LobbyController.Instance != null)
        {
            LobbyController.Instance?.RegisterPlayer(this);
        }

        if (IsOwner)
        {
            string chosenName = LobbyController.Instance.ChosenPlayerName;

            if (IsServer)
            {
                SetPlayerName(chosenName);
            }
            else
            {
                SetPlayerNameRpc(chosenName);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerNameRpc(string newName)
    {
        SetPlayerName(newName);
    }

    private void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            newName = "Player " + OwnerClientId;
        }

        newName = newName.Trim();

        if(newName.Length > 20)
        {
            newName = newName.Substring(0,20);
        }

        PlayerName.Value = new FixedString64Bytes(newName);
    }

    private void OnPlayerNameChanged(
        FixedString64Bytes oldName,
        FixedString64Bytes newName)
    {
        LobbyDataChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
        IsReady.OnValueChanged -= OnReadyChanged;

        if(LobbyController.Instance != null)
        {
            LobbyController.Instance?.UnregisterPlayer(this);
        }
    }

    public void ToggleReady()
    {
        if (!IsOwner)
        {
            return;
        }

        SetReadyRpc(!IsReady.Value);
    }

    [Rpc(SendTo.Server)]
    private void SetReadyRpc(bool ready)
    {
        IsReady.Value = ready;
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        LobbyDataChanged?.Invoke();
    }
}
