using UnityEngine;

public interface IPlayerInputSource
{
    Vector2 MoveInput { get; }
    Vector2 LookInput { get; }

    bool JumpPressed { get; }
}
