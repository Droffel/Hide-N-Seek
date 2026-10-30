using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private PlayerRoleManager roleManager;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField, Min(0.1f)] private float clearanceRadius = 0.5f;

    [Tooltip("Only include layers that should block a spawn location.")]
    [SerializeField] private LayerMask obstructionMask = -0;
    private NetworkManager networkManager;

    private readonly Dictionary<ulong, NetworkObject> spawnedPlayers =
        new Dictionary<ulong, NetworkObject>();

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError(
                "PlayerSpawner: No NetworkManager found in the scene."
            );

            enabled = false;
            return;
        }

        if (!networkManager.IsServer)
        {
            return;
        }

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnDestroy()
    {
        if(networkManager == null)
        {
            return;
        }

        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!networkManager.IsServer)
        {
            return;
        }

        SpawnPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        spawnedPlayers.Remove(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if(!networkManager.IsServer)
        {
            return;
        }

        if (spawnedPlayers.ContainsKey(clientId))
        {
            return;
        }
        
        if (playerPrefab == null)
        {
            Debug.LogError(
                "PlayerTestSpawner: Player prefab has not been assigned."
            );

            return;
        }

        if (!TryGetValidSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogError($"PlayerSpawner: No valid spawn point for client {clientId}. " +
                "Add more spawn points or clear obstructed locations.");

            return;
        }

        NetworkObject player = Instantiate(
            playerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        player.SpawnAsPlayerObject(clientId, true);

        spawnedPlayers.Add(clientId, player);

        Debug.Log($"Spawned client {clientId} at {spawnPosition}");

        if (roleManager != null)
            roleManager.AssignRolesAfterSpawn();
    }

    private bool TryGetValidSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerSpawner: No spawn points assigned.");
            return false;
        }
        List<Transform> candidates = new List<Transform>();

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                candidates.Add(point);
            }
        } 

        for (int i = 0; i < candidates.Count; i++)
        {
            int randomIndex = Random.Range(i, candidates.Count);

            (candidates[i], candidates[randomIndex]) = (candidates[randomIndex], candidates[i]);
        }

        foreach (Transform point in candidates)
        {
            Vector3 candidate = point.position;

            if (Physics.CheckSphere(
                candidate,
                clearanceRadius,
                obstructionMask,
                QueryTriggerInteraction.Ignore
            ))
            {
                continue;
            }

            bool occupied = false;

            foreach (NetworkObject existingPlayer in spawnedPlayers.Values)
            {
                if(existingPlayer == null || !existingPlayer.IsSpawned)
                {
                    continue;
                }

                Vector3 existingPosition = existingPlayer.transform.position;

                if (Vector3.Distance(candidate, existingPosition) < clearanceRadius * 2f)
                {
                    occupied = true;
                    break;
                }
            }

            if(occupied)
            {
                continue;
            }

            position = candidate;
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if(spawnPoints == null)
        {
            return;
        }

        Gizmos.color = Color.green;

        foreach (Transform point in spawnPoints)
        {
            if(point != null)
            {
                Gizmos.DrawWireSphere(point.position, clearanceRadius);
            }
        }
    }
}