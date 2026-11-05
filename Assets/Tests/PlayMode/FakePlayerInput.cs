using UnityEngine;

/// <summary>
/// Simulated player input. Tests set these values to imitate a player
/// holding keys, moving the mouse, jumping or attacking.
/// </summary>
public class FakePlayerInput : IPlayerInputSource
{
    public Vector2 MoveInput { get; set; }
    public Vector2 LookInput { get; set; }
    public bool JumpPressed { get; set; }
    public bool AttackPressed { get; set; }
}
