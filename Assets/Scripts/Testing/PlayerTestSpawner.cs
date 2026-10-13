using Unity.Netcode;
using UnityEngine;

public class PlayerTestSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError(
                "PlayerTestSpawner: No NetworkManager found in the scene."
            );

            enabled = false;
            return;
        }

        networkManager.OnClientConnectedCallback += OnClientConnected;

        // Handle clients that connected before this component initialized.
        if (networkManager.IsServer)
        {
            foreach (ulong clientId in networkManager.ConnectedClientsIds)
            {
                OnClientConnected(clientId);
            }
        }
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager == null || !networkManager.IsServer)
        {
            return;
        }

        if (networkManager.ConnectedClients.TryGetValue(
                clientId,
                out NetworkClient client)
            && client.PlayerObject != null)
        {
            return;
        }

        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
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

        player.SpawnAsPlayerObject(clientId);

        Debug.Log("Spawned player for client " + clientId);
    }
}