using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    private readonly NetworkVariable<bool> isAlive =
        new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool IsAlive => isAlive.Value;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private CharacterController characterController;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        isAlive.OnValueChanged += OnAliveChanged;

        ApplyAliveState(isAlive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isAlive.OnValueChanged -= OnAliveChanged;
    }

    public bool TryKill()
{
    if (!IsServer)
    {
        return false;
    }

    if (!isAlive.Value)
    {
        return false;
    }

    isAlive.Value = false;

    Debug.Log($"Object {NetworkObjectId} has been eliminated.");

    // Despawn this network object after a short delay.
    StartCoroutine(DespawnAfterDelay());

    return true;
}

private System.Collections.IEnumerator DespawnAfterDelay()
{
    yield return new WaitForSeconds(1f);

    if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
    {
        NetworkObject.Despawn(true);
    }
}

    private void OnAliveChanged(bool oldValue, bool newValue)
    {
        ApplyAliveState(newValue);
    }

    private void ApplyAliveState(bool alive)
    {
        if(playerInput != null)
        {
            playerInput.SetInputEnabled(alive);
        }

        if(characterController != null)
        {
            characterController.enabled = alive;
        }
        if(playerMovement != null)
        {
            playerMovement.enabled = alive;
        }

        if(!alive && IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}