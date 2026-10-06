using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnPlayerNameChanged;

        LobbyUIManager.Instance?.RegisterPlayer(this);

        if (IsOwner)
        {
            string chosenName = RelayManager.Instance.ChosenPlayerName;

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

        LobbyUIManager.Instance?.RefreshPlayerList();
    }

    private void OnPlayerNameChanged(
        FixedString64Bytes oldName,
        FixedString64Bytes newName)
    {
        LobbyUIManager.Instance?.RefreshPlayerList();
    }
}
