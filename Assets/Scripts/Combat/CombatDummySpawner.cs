using Unity.Netcode;
using UnityEngine;

public class CombatDummySpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject dummyPrefab;
    [SerializeField] private Vector3 spawnPosition =
        new Vector3(0f, 1f, 3f);

    private NetworkObject spawnedDummy;

    private void Start()
{
    StartCoroutine(WaitForHostAndSpawn());
}

private System.Collections.IEnumerator WaitForHostAndSpawn()
{
    while (NetworkManager.Singleton == null ||
           !NetworkManager.Singleton.IsListening)
    {
        yield return null;
    }

    NetworkManager networkManager = NetworkManager.Singleton;

    if (!networkManager.IsServer)
    {
        yield break;
    }

    SpawnDummy();
}

    private void SpawnDummy()
    {
        if (dummyPrefab == null)
        {
            Debug.LogError(
                "CombatDummySpawner: Dummy prefab is not assigned."
            );

            return;
        }

        spawnedDummy = Instantiate(
            dummyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        spawnedDummy.Spawn();

        Debug.Log("Combat dummy spawned.");
    }
}