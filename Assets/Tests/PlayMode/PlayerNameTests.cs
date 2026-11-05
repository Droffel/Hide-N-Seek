using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PlayerNameTests
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return NetTestUtils.Cleanup();
    }

    [UnityTest]
    public IEnumerator TypedName_IsUsed()
        => AssertNameBecomes("Alice", "Alice");

    [UnityTest]
    public IEnumerator Name_WithSurroundingSpaces_IsTrimmed()
        => AssertNameBecomes("   Alice   ", "Alice");

    [UnityTest]
    public IEnumerator Name_LongerThan20Characters_IsCutTo20()
        => AssertNameBecomes("ABCDEFGHIJKLMNOPQRSTUVWXY", "ABCDEFGHIJKLMNOPQRST");

    [UnityTest]
    public IEnumerator Name_Exactly20Characters_IsKept()
        => AssertNameBecomes("ABCDEFGHIJKLMNOPQRST", "ABCDEFGHIJKLMNOPQRST");

    [UnityTest]
    public IEnumerator EmptyName_DefaultsToPlayer()
        => AssertNameBecomes("", "Player");

    [UnityTest]
    public IEnumerator WhitespaceOnlyName_DefaultsToPlayer()
        => AssertNameBecomes("      ", "Player");

    [UnityTest]
    public IEnumerator Name_WithSwedishCharacters_IsKept()
        => AssertNameBecomes("Åsa Öberg", "Åsa Öberg");

    private static IEnumerator AssertNameBecomes(string typed, string expected)
    {
        yield return NetTestUtils.HostLobby(typed);

        LobbyPlayer me = NetTestUtils.FindLocalLobbyPlayer();

        Assert.AreEqual(expected, me.PlayerName.Value.ToString(),
            $"Typing '{typed}' should give the name '{expected}'.");

        Assert.IsTrue(NetTestUtils.AnyRowShowsName(expected),
            $"The lobby player list should show '{expected}'.");
    }
}
