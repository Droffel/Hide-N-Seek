using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[DefaultExecutionOrder(-100)]
public class PlayerInput : NetworkBehaviour, IPlayerInputSource
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    private bool inputEnabled = true;

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if(!enabled)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            JumpPressed = false;
            AttackPressed = false;
        }
    }
    private void Update()
    {

        if (!IsOwner || !inputEnabled)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            JumpPressed = false;
            AttackPressed = false;
            return;
        }

        ReadMovementInput();
        ReadMouseInput();
        ReadJumpInput();
        ReadAttackInput();
    }

    private void ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            MoveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed)
            horizontal -= 1f;
        
        if (keyboard.dKey.isPressed)
            horizontal += 1f;

        if (keyboard.sKey.isPressed)
            vertical -= 1f;

        if (keyboard.wKey.isPressed)
            vertical += 1f;

        MoveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void ReadMouseInput()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            LookInput = Vector2.zero;
            return;
        }

        LookInput = mouse.delta.ReadValue();
    }

    private void ReadJumpInput()
    {
        Keyboard keyboard = Keyboard.current;

        JumpPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
    }
    private void ReadAttackInput()
    {
        Mouse mouse = Mouse.current;

        AttackPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
    }
}
