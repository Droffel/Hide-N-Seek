using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovementTests
{
    private PlayerMovement player;
    private CharacterController controller;
    private FakePlayerInput input;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return NetTestUtils.HostGameWithLocalPlayer(p => player = p);

        controller = player.GetComponent<CharacterController>();
        input = NetTestUtils.InjectFakeInput(player);

        yield return NetTestUtils.WaitUntil(
            () => controller.isGrounded,
            "Player never landed. Is there ground under the spawn points?",
            5f);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return NetTestUtils.Cleanup();
    }

    [UnityTest]
    public IEnumerator HoldingForward_MovesPlayerForward()
    {
        Vector3 start = player.transform.position;
        Vector3 forward = player.transform.forward;

        input.MoveInput = Vector2.up; // holding W
        yield return new WaitForSeconds(0.5f);
        input.MoveInput = Vector2.zero;

        Vector3 moved = NetTestUtils.Flat(player.transform.position - start);

        Assert.Greater(Vector3.Dot(moved, forward), 0.5f,
            $"Player should have moved forward. Moved: {moved}");
    }

    [UnityTest]
    public IEnumerator HoldingBack_MovesPlayerBackward()
    {
        Vector3 start = player.transform.position;
        Vector3 forward = player.transform.forward;

        input.MoveInput = Vector2.down; // holding S
        yield return new WaitForSeconds(0.5f);
        input.MoveInput = Vector2.zero;

        Vector3 moved = NetTestUtils.Flat(player.transform.position - start);

        Assert.Less(Vector3.Dot(moved, forward), -0.5f,
            $"Player should have moved backward. Moved: {moved}");
    }

    [UnityTest]
    public IEnumerator HoldingRight_StrafesRight()
    {
        Vector3 start = player.transform.position;
        Vector3 right = player.transform.right;

        input.MoveInput = Vector2.right; // holding D
        yield return new WaitForSeconds(0.5f);
        input.MoveInput = Vector2.zero;

        Vector3 moved = NetTestUtils.Flat(player.transform.position - start);

        Assert.Greater(Vector3.Dot(moved, right), 0.5f,
            $"Player should have strafed right. Moved: {moved}");
    }

    [UnityTest]
    public IEnumerator NoInput_PlayerStaysPut()
    {
        Vector3 start = player.transform.position;

        yield return new WaitForSeconds(0.5f);

        Vector3 moved = NetTestUtils.Flat(player.transform.position - start);

        Assert.Less(moved.magnitude, 0.05f,
            $"Player should not move without input. Moved: {moved}");
    }

    [UnityTest]
    public IEnumerator MovingMouseRight_TurnsPlayerRight()
    {
        float startYaw = player.transform.eulerAngles.y;

        input.LookInput = new Vector2(50f, 0f); // mouse moved right for one frame
        yield return null;
        input.LookInput = Vector2.zero;

        float turned = Mathf.DeltaAngle(startYaw, player.transform.eulerAngles.y);

        Assert.Greater(turned, 0f, "Moving the mouse right should turn the player right.");
    }

    [UnityTest]
    public IEnumerator PressingJump_LiftsPlayerOffTheGround()
    {
        float startY = player.transform.position.y;

        input.JumpPressed = true; // space pressed for exactly one frame
        yield return null;
        input.JumpPressed = false;

        yield return new WaitForSeconds(0.2f);

        Assert.Greater(player.transform.position.y, startY + 0.2f,
            "Player should be in the air shortly after jumping.");
    }

    [UnityTest]
    public IEnumerator Seeker_CannotMove_DuringPreparation()
    {
        NetTestUtils.ForceRole(player, PlayerRoleType.Seeker);

        SeekerPreparationController preparation =
            player.GetComponent<SeekerPreparationController>();
        Assert.IsNotNull(preparation, "Player has no SeekerPreparationController.");

        preparation.BeginPreparation();
        yield return NetTestUtils.WaitUntil(() => preparation.IsPreparing,
            "Preparation did not start.");

        Vector3 start = player.transform.position;

        input.MoveInput = Vector2.up;
        yield return new WaitForSeconds(0.5f);
        input.MoveInput = Vector2.zero;

        Vector3 moved = NetTestUtils.Flat(player.transform.position - start);

        Assert.Less(moved.magnitude, 0.05f,
            $"The seeker should be frozen during preparation. Moved: {moved}");
    }
}
