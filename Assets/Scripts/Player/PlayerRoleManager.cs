using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRoleManager : NetworkBehaviour
{
    public void StartRound()
    {
        // Let the server make the assignment.
        if (!IsServer)
            return;
        
        AssignRoles();
    }
  
    public void AssignRolesAfterSpawn()
    {
        if (!IsServer)
            return;

        AssignRoles();
    }
    private void AssignRoles()
    {
        if (!IsServer)
            return;

        List<PlayerRole> players = new List<PlayerRole>();

        foreach (PlayerRole role in FindObjectsByType<PlayerRole>(FindObjectsSortMode.None))
        {
            if (role.IsSpawned)
                players.Add(role);
        }

        if (players.Count == 0)
        {
            Debug.LogWarning("Cannot assign roles: no players found.");
            return;
        }

        foreach (PlayerRole player in players)
            player.SetRole(PlayerRoleType.Hider);

        int seekerIndex = Random.Range(0, players.Count);
        players[seekerIndex].SetRole(PlayerRoleType.Seeker);

        Debug.Log($"Roles assigned. Total players: {players.Count}. " +
              $"Seeker: client {players[seekerIndex].OwnerClientId}.");
    }
}