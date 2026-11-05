using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject playerVisuals;

    private readonly NetworkVariable<bool> isAlive =
        new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private readonly NetworkVariable<float> currentHealth =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool IsAlive => isAlive.Value;
    public float CurrentHealth => currentHealth.Value;
    public float MaxHealth => playerStats != null ? playerStats.MaxHealth : 100f;

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

        if (IsServer)
        {
            currentHealth.Value = MaxHealth;
        }

        ApplyAliveState(isAlive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isAlive.OnValueChanged -= OnAliveChanged;
    }

    /// <summary>
    /// Server only. Returns true if the damage was applied.
    /// Zero, negative (would heal) and NaN damage are rejected.
    /// Damage larger than the remaining health clamps health to 0 and kills.
    /// </summary>
    public bool TryApplyDamage(float amount)
    {
        if (!IsServer || !isAlive.Value)
        {
            return false;
        }

        if (float.IsNaN(amount) || amount <= 0f)
        {
            return false;
        }

        currentHealth.Value = Mathf.Max(0f, currentHealth.Value - amount);

        if (currentHealth.Value <= 0f)
        {
            TryKill();
        }

        return true;
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

        currentHealth.Value = 0f;
        isAlive.Value = false;

        MatchController matchController = FindFirstObjectByType<MatchController>();

        if (matchController != null)
        {
            matchController.CheckForSeekerWin();
        }

        Debug.Log($"Object {NetworkObjectId} has been eliminated.");

        return true;
    }

    private void OnAliveChanged(bool oldValue, bool newValue)
    {
        ApplyAliveState(newValue);
    }

    private void ApplyAliveState(bool alive)
    {
        if (playerInput != null)
        {
            playerInput.SetInputEnabled(alive);
        }

        if (characterController != null)
        {
            characterController.enabled = alive;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = alive;
        }

        if (playerVisuals != null)
        {
            playerVisuals.SetActive(alive);
        }

        if (!alive && IsOwner)
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        }
    }
}