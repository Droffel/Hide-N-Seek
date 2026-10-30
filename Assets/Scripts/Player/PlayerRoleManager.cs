using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerRoleManager : NetworkBehaviour
{
    private bool rolesAssigned = false;
  
    public void AssignRolesAfterSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        if (rolesAssigned)
        {
            return;
        }

        List<PlayerRole> players = GetSpawnedPlayers();

        int connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (players.Count < connectedPlayers)
        {
            Debug.Log(
                $"Waiting for players before assigning roles. " +
                $"Spawned: {players.Count}/{connectedPlayers}"
            );

            return;
        }

        AssignRoles(players);
    }

    private List<PlayerRole> GetSpawnedPlayers()
    {
        List<PlayerRole> players = new List<PlayerRole>();

        foreach (PlayerRole role in FindObjectsByType<PlayerRole>(FindObjectsSortMode.None))
        {
            if (role.IsSpawned)
            {
                players.Add(role);
            }
        }
        return players;
    }
    private void AssignRoles(List<PlayerRole> players)
{
    if (!IsServer || players.Count == 0)
        return;

    foreach (PlayerRole player in players)
    {
        player.SetRole(PlayerRoleType.Hider);
    }

    int seekerIndex = Random.Range(0, players.Count);

    PlayerRole seeker = players[seekerIndex];
    seeker.SetRole(PlayerRoleType.Seeker);

    SeekerPreparationController preparation =
        seeker.GetComponent<SeekerPreparationController>();

    if (preparation == null)
    {
        Debug.LogError(
            $"PlayerRoleManager: Seeker {seeker.OwnerClientId} " +
            "is missing SeekerPreparationController."
        );

        return;
    }

    preparation.BeginPreparation();

    rolesAssigned = true;

    Debug.Log(
        $"Seeker assigned to client {seeker.OwnerClientId}. " +
        "30-second preparation started."
    );
}
}