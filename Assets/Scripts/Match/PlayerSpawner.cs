using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

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

        Vector3 spawnPosition = new Vector3(
            clientId * 2f,
            1f,
            0f
        );

        NetworkObject player = Instantiate(
            playerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        player.SpawnWithOwnership(clientId, true);

        spawnedPlayers.Add(clientId, player);
    }
}