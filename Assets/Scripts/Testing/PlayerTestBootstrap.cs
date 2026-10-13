using Unity.Netcode;
using UnityEngine;

public class PlayerTestBootstrap : MonoBehaviour
{
    private void Start()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager was not found.");
            return;
        }

        if (networkManager.IsListening)
        {
            Debug.Log("Network session is already running.");
            return;
        }

        bool started = networkManager.StartHost();

        if (!started)
        {
            Debug.LogError("Failed to start the local host.");
        }
    }
}