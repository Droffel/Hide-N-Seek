using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CombatTests
{
    private PlayerMovement player;
    private PlayerHealth health;
    private FakePlayerInput input;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return NetTestUtils.HostGameWithLocalPlayer(p => player = p);

        health = player.GetComponent<PlayerHealth>();
        Assert.IsNotNull(health, "Player has no PlayerHealth.");

        input = NetTestUtils.InjectFakeInput(player);

        yield return NetTestUtils.WaitUntil(
            () => health.CurrentHealth > 0f,
            "Player health was never initialised.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return NetTestUtils.Cleanup();
    }

    [UnityTest]
    public IEnumerator NewPlayer_HasFullHealth_AndIsAlive()
    {
        Assert.AreEqual(health.MaxHealth, health.CurrentHealth, 0.001f);
        Assert.IsTrue(health.IsAlive);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Damage_ReducesHealth()
    {
        float max = health.MaxHealth;

        Assert.IsTrue(health.TryApplyDamage(30f), "Valid damage should be applied.");
        Assert.AreEqual(max - 30f, health.CurrentHealth, 0.001f);
        Assert.IsTrue(health.IsAlive, "Player should survive non-lethal damage.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator NegativeDamage_IsRejected_AndDoesNotHeal()
    {
        health.TryApplyDamage(30f);
        float before = health.CurrentHealth;

        Assert.IsFalse(health.TryApplyDamage(-50f), "Negative damage must be rejected.");
        Assert.AreEqual(before, health.CurrentHealth, 0.001f, "Negative damage must not heal.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator NegativeDamage_CannotRaiseHealthAboveMax()
    {
        Assert.IsFalse(health.TryApplyDamage(-1000f));
        Assert.AreEqual(health.MaxHealth, health.CurrentHealth, 0.001f);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ZeroDamage_IsRejected()
    {
        float before = health.CurrentHealth;

        Assert.IsFalse(health.TryApplyDamage(0f));
        Assert.AreEqual(before, health.CurrentHealth, 0.001f);
        yield return null;
    }

    [UnityTest]
    public IEnumerator NaNDamage_IsRejected()
    {
        float before = health.CurrentHealth;

        Assert.IsFalse(health.TryApplyDamage(float.NaN));
        Assert.AreEqual(before, health.CurrentHealth, 0.001f);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DamageGreaterThanHealth_ClampsToZero_AndKills()
    {
        Assert.IsTrue(health.TryApplyDamage(health.MaxHealth + 500f));

        Assert.AreEqual(0f, health.CurrentHealth, 0.001f, "Health must never go below zero.");
        Assert.IsFalse(health.IsAlive, "Overkill damage should kill the player.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator ExactlyLethalDamage_Kills()
    {
        health.TryApplyDamage(health.MaxHealth);

        Assert.AreEqual(0f, health.CurrentHealth, 0.001f);
        Assert.IsFalse(health.IsAlive);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RepeatedDamage_AddsUp_UntilDeath()
    {
        float hit = health.MaxHealth / 4f;

        for (int i = 0; i < 3; i++)
        {
            health.TryApplyDamage(hit);
            Assert.IsTrue(health.IsAlive, $"Player should survive hit {i + 1} of 4.");
        }

        health.TryApplyDamage(hit);

        Assert.IsFalse(health.IsAlive, "Player should die on the 4th quarter-health hit.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator DeadPlayer_IgnoresFurtherDamage()
    {
        health.TryApplyDamage(health.MaxHealth);

        Assert.IsFalse(health.TryApplyDamage(10f), "A dead player should not take damage.");
        Assert.AreEqual(0f, health.CurrentHealth, 0.001f);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DeadPlayer_CannotMove()
    {
        health.TryApplyDamage(health.MaxHealth);
        yield return null; // let the alive-state change disable movement

        Vector3 start = player.transform.position;

        input.MoveInput = Vector2.up; // holding W
        yield return new WaitForSeconds(0.5f);
        input.MoveInput = Vector2.zero;

        Vector3 moved = NetTestUtils.Flat(player.transform.position - start);

        Assert.Less(moved.magnitude, 0.05f, $"A dead player should not move. Moved: {moved}");
    }
}
