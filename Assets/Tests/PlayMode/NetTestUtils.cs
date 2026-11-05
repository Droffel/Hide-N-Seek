using System;
using System.Collections;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shared helpers for the PlayMode tests: loading scenes, starting a local host,
/// finding players, clicking UI, and cleaning up after each test.
/// </summary>
public static class NetTestUtils
{
    public const string LobbySceneName = "LobbyCreationScene";
    public const string GameSceneName = "GameScene";
    public const float DefaultTimeout = 10f;

    // ---------- Waiting ----------

    public static IEnumerator WaitUntil(Func<bool> condition, string failMessage, float timeout = DefaultTimeout)
    {
        float deadline = Time.realtimeSinceStartup + timeout;

        while (!condition())
        {
            if (Time.realtimeSinceStartup > deadline)
            {
                Assert.Fail($"Timed out after {timeout}s: {failMessage}");
            }

            yield return null;
        }
    }

    public static IEnumerator WaitFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }
    }

    // ---------- Scenes & networking ----------

    public static IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        Assert.IsNotNull(load,
            $"Scene '{sceneName}' could not be loaded. Is it in the Build Profile scene list?");

        while (!load.isDone)
        {
            yield return null;
        }

        yield return null; // let Awake/Start run
    }

    /// <summary>Starts a host on localhost (no Relay / internet needed).</summary>
    public static IEnumerator StartLocalHost()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        Assert.IsNotNull(networkManager, "No NetworkManager in the loaded scene.");

        UnityTransport transport = networkManager.GetComponent<UnityTransport>();
        Assert.IsNotNull(transport, "UnityTransport was not found on NetworkManager.");

        transport.SetConnectionData("127.0.0.1", 7777, "127.0.0.1");

        Assert.IsTrue(networkManager.StartHost(), "NetworkManager failed to start as Host.");

        yield return WaitUntil(
            () => networkManager.IsConnectedClient,
            "Local client never connected to the host.");
    }

    /// <summary>Loads GameScene the same way LobbyController.StartGame does.</summary>
    public static IEnumerator LoadGameSceneAsHost()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager.SceneManager != null)
        {
            SceneEventProgressStatus status =
                networkManager.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);

            Assert.AreEqual(SceneEventProgressStatus.Started, status,
                "Netcode refused to load GameScene.");
        }
        else
        {
            SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Single);
        }

        yield return WaitUntil(
            () => SceneManager.GetActiveScene().name == GameSceneName,
            "GameScene never became the active scene.");
    }

    /// <summary>
    /// Lobby -> local host -> GameScene, then hands back the local player.
    /// The player is forced to Hider so the 30 s seeker freeze can't block a test.
    /// </summary>
    public static IEnumerator HostGameWithLocalPlayer(Action<PlayerMovement> onReady)
    {
        yield return LoadScene(LobbySceneName);
        yield return StartLocalHost();
        yield return LoadGameSceneAsHost();

        PlayerMovement player = null;

        yield return WaitUntil(
            () => (player = FindLocalGamePlayer()) != null,
            "The local player (with PlayerMovement) was never spawned in GameScene.");

        // Give any role assignment a few frames to happen, then override it.
        yield return WaitFrames(5);
        ForceRole(player, PlayerRoleType.Hider);

        onReady(player);
    }

    public static IEnumerator Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            UnityEngine.Object.Destroy(NetworkManager.Singleton.gameObject);
        }

        yield return null; // let shutdown and Destroy finish
    }

    // ---------- Players ----------

    public static PlayerMovement FindLocalGamePlayer()
    {
        foreach (PlayerMovement movement in
                 UnityEngine.Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (movement.IsSpawned && movement.IsOwner)
            {
                return movement;
            }
        }

        return null;
    }

    public static LobbyPlayer FindLocalLobbyPlayer()
    {
        foreach (LobbyPlayer player in
                 UnityEngine.Object.FindObjectsByType<LobbyPlayer>(FindObjectsSortMode.None))
        {
            if (player.IsSpawned && player.IsOwner)
            {
                return player;
            }
        }

        return null;
    }

    public static void ForceRole(Component player, PlayerRoleType roleType)
    {
        PlayerRole role = player.GetComponent<PlayerRole>();

        if (role != null)
        {
            role.SetRole(roleType); // host is the server, so this is allowed
        }
    }

    /// <summary>
    /// Replaces keyboard/mouse with a FakePlayerInput the test controls.
    /// </summary>
    public static FakePlayerInput InjectFakeInput(Component player)
    {
        FakePlayerInput fake = new FakePlayerInput();

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Assert.IsNotNull(movement, "Player has no PlayerMovement.");
        movement.SetInputSource(fake);

        PlayerAim aim = player.GetComponent<PlayerAim>();
        if (aim != null)
        {
            aim.SetInputSource(fake);
        }

        return fake;
    }

    public static Vector3 Flat(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }

    // ---------- Lobby ----------

    /// <summary>
    /// Loads the lobby, types a name into the name field like a player would,
    /// and hosts locally. Waits until the local LobbyPlayer has its name.
    /// </summary>
    public static IEnumerator HostLobby(string typedName)
    {
        yield return LoadScene(LobbySceneName);

        LobbyUIManager ui = UnityEngine.Object.FindFirstObjectByType<LobbyUIManager>();
        Assert.IsNotNull(ui, "No LobbyUIManager in the lobby scene.");
        Assert.IsNotNull(LobbyController.Instance, "No LobbyController in the lobby scene.");

        ui.playerNameInput.text = typedName;          // what the player typed
        LobbyController.Instance.CapturePlayerName(); // what Host/Join does with it

        yield return StartLocalHost();

        yield return WaitUntil(() =>
            {
                LobbyPlayer me = FindLocalLobbyPlayer();
                return me != null && me.PlayerName.Value.Length > 0;
            },
            "The local LobbyPlayer never spawned with a name.");

        yield return null; // let the player list rebuild
    }

    public static LobbyPlayerRowUI[] FindRows()
    {
        return UnityEngine.Object.FindObjectsByType<LobbyPlayerRowUI>(FindObjectsSortMode.None);
    }

    public static bool AnyRowShowsName(string playerName)
    {
        foreach (LobbyPlayerRowUI row in FindRows())
        {
            foreach (TMP_Text text in row.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.text == playerName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Clicks the local player's Ready button through the EventSystem, like a mouse click.</summary>
    public static IEnumerator ClickLocalReadyButton()
    {
        yield return null; // old rows are destroyed at end of frame

        Assert.IsNotNull(EventSystem.current, "No EventSystem in the scene.");

        foreach (LobbyPlayerRowUI row in FindRows())
        {
            Button button = row.GetComponentInChildren<Button>();

            if (button != null && button.interactable)
            {
                ExecuteEvents.Execute(
                    button.gameObject,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.pointerClickHandler);

                yield break;
            }
        }

        Assert.Fail("No interactable Ready button was found for the local player.");
    }
}
