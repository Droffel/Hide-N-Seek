using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerHideCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 0.15f;

    private bool isHiding;

    public void SetHiding(bool hiding)
    {
        if (!IsOwner)
            return;

        isHiding = hiding;
    }

    private void Update()
    {
        if (!IsOwner || !isHiding)
            return;

        if (Mouse.current == null)
            return;

        float mouseX =
            Mouse.current.delta.ReadValue().x;

        transform.Rotate(
            Vector3.up * mouseX * mouseSensitivity
        );
    }
}