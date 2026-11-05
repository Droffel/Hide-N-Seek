using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class LobbyTests
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return NetTestUtils.Cleanup();
    }

    [UnityTest]
    public IEnumerator HostingLobby_ShowsLobbyPanel_AndListsPlayer()
    {
        yield return NetTestUtils.HostLobby("Alice");

        LobbyUIManager ui = Object.FindFirstObjectByType<LobbyUIManager>();

        Assert.IsTrue(ui.lobbyPanel.activeSelf, "Lobby panel should be visible after hosting.");
        Assert.IsFalse(ui.mainMenuPanel.activeSelf, "Main menu should be hidden after hosting.");

        Assert.AreEqual(1, NetTestUtils.FindRows().Length, "Exactly one player row was expected.");
        Assert.IsTrue(NetTestUtils.AnyRowShowsName("Alice"), "The player list should show 'Alice'.");
    }

    [UnityTest]
    public IEnumerator NewPlayer_IsNotReady()
    {
        yield return NetTestUtils.HostLobby("Alice");

        LobbyPlayer me = NetTestUtils.FindLocalLobbyPlayer();
        Assert.IsFalse(me.IsReady.Value, "A player who just joined should not be ready.");
    }

    [UnityTest]
    public IEnumerator ClickingReady_TogglesReadyState()
    {
        yield return NetTestUtils.HostLobby("Alice");

        LobbyPlayer me = NetTestUtils.FindLocalLobbyPlayer();

        yield return NetTestUtils.ClickLocalReadyButton();
        yield return NetTestUtils.WaitUntil(() => me.IsReady.Value,
            "Player did not become ready after clicking Ready.");

        yield return NetTestUtils.ClickLocalReadyButton();
        yield return NetTestUtils.WaitUntil(() => !me.IsReady.Value,
            "Player did not become un-ready after clicking Ready again.");
    }

    [UnityTest]
    public IEnumerator StartGame_WhenNotReady_StaysInLobby()
    {
        yield return NetTestUtils.HostLobby("Alice");

        LogAssert.Expect(LogType.Warning, "Not all players are ready");
        LobbyController.Instance.StartGame();

        yield return NetTestUtils.WaitFrames(10);

        Assert.AreEqual(NetTestUtils.LobbySceneName, SceneManager.GetActiveScene().name,
            "The game should not start while a player is not ready.");
    }

    [UnityTest]
    public IEnumerator StartGame_WhenAllReady_LoadsGameScene_AndSpawnsPlayer()
    {
        yield return NetTestUtils.HostLobby("Alice");

        LobbyPlayer me = NetTestUtils.FindLocalLobbyPlayer();

        yield return NetTestUtils.ClickLocalReadyButton();
        yield return NetTestUtils.WaitUntil(() => me.IsReady.Value,
            "Player did not become ready after clicking Ready.");

        LobbyController.Instance.StartGame();

        yield return NetTestUtils.WaitUntil(
            () => SceneManager.GetActiveScene().name == NetTestUtils.GameSceneName,
            "GameScene was not loaded after the host started the game.");

        yield return NetTestUtils.WaitUntil(
            () => NetTestUtils.FindLocalGamePlayer() != null,
            "No local player was spawned in GameScene.");
    }
}
