using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.Netcode.Transports.UTP;
public class PlayerSpawnTests
{
    [Test]
    public void TestRunnerWorks()
    {
        Assert.IsTrue(true);
    }

    [UnityTest]
    public IEnumerator ClientConnects_PlayerIsSpawned()
    {
        AsyncOperation lobbyLoad =
            SceneManager.LoadSceneAsync("LobbyCreationScene");

        while (!lobbyLoad.isDone)
        {
            yield return null;
        }

        yield return null;

        NetworkManager networkManager = NetworkManager.Singleton;

        Assert.IsNotNull(
            networkManager,
            "No NetworkManager was found in LobbyCreationScene."
        );

        Debug.Log("TEST: NetworkManager found.");

        UnityTransport transport =
        networkManager.GetComponent<UnityTransport>();

        Assert.IsNotNull(
            transport,
            "UnityTransport was not found on NetworkManager."
        );

        transport.SetConnectionData(
            "127.0.0.1",
            7777,
            "127.0.0.1"
        );
        bool started = networkManager.StartHost();

        Assert.IsTrue(
            started,
            "NetworkManager failed to start as Host."
        );

        Debug.Log("TEST: Host started.");

        float timeout = 5f;

        while (!networkManager.IsConnectedClient && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(
            networkManager.IsConnectedClient,
            "Local client never connected to the Host."
        );

        Debug.Log("TEST: Local client connected.");

        AsyncOperation gameLoad =
            SceneManager.LoadSceneAsync("GameScene");

        while (!gameLoad.isDone)
        {
            yield return null;
        }

        yield return null;
        yield return null;

        Assert.IsNotNull(
            NetworkManager.Singleton,
            "NetworkManager disappeared when GameScene loaded."
        );

        ulong clientId = NetworkManager.Singleton.LocalClientId;

        NetworkObject player =
            NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

        Assert.IsNotNull(
        player,
        "No player object was spawned for the connected client."
        );

        Debug.Log("TEST PASSED: Connected client has a spawned player object.");

        

        yield return null;
    }
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Object.Destroy(NetworkManager.Singleton.gameObject);
        }
    yield return null;   // give Unity a frame to finish shutting down and destroying
}
}