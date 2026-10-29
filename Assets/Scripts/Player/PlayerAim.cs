using System;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerAim : NetworkBehaviour
{
    [Header("Aim Settings")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Transform cameraPivot;

    [Header("Network Settings")]
    [SerializeField, Min(0.02f)]
    private float syncInterval = 0.05f;

    private IPlayerInputSource inputSource;
    private PlayerMovement playerMovement;

    private float cameraPitch;
    private float nextSyncTime;

    private readonly NetworkVariable<float> replicatedPitch = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        inputSource = GetComponent<IPlayerInputSource>();
        playerMovement = GetComponent<PlayerMovement>();

    }

    public override void OnNetworkSpawn()
    {
        replicatedPitch.OnValueChanged += OnPitchChanged;

        if(IsOwner)
        {
            cameraPitch = 0f;
            ApplyPitch(cameraPitch);
        }
        else
        {
            ApplyPitch(replicatedPitch.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        replicatedPitch.OnValueChanged -= OnPitchChanged;
    }

    private void Update()
    {
        if(playerStats == null || cameraPivot == null)
        {
            return;
        }

        if(IsOwner)
        {
            UpdateLocalAim();
        }
        else
        {
            ApplyPitch(replicatedPitch.Value);
        }
    }

    private void UpdateLocalAim()
    {
        if(inputSource == null || playerMovement == null)
        {
            return;
        }

        if(!playerMovement.IsCursorLocked)
        {
            return;
        }

        float mouseY = inputSource.LookInput.y * playerStats.MouseSensitivity;

        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -playerStats.MaxLookAngle,
            playerStats.MaxLookAngle
        );

        ApplyPitch(cameraPitch);

        if(Time.time >= nextSyncTime)
        {
            nextSyncTime = Time.time + syncInterval;
            SubmitPitchRpc(cameraPitch);
        }
    }

    private void ApplyPitch(float pitch)
    {
        if (cameraPivot == null)
        {
            return;
        }

        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void OnPitchChanged(float oldPitch, float newPitch)
    {
        if (!IsOwner)
        {
            ApplyPitch(newPitch);
        }
    }

    [Rpc(SendTo.Server)]
    private void SubmitPitchRpc(float requestedPitch, RpcParams rpcParams = default)
    {
        if(rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }

        if(playerStats == null)
        {
            return;
        }

        float validatedPitch = Mathf.Clamp(
            requestedPitch, -playerStats.MaxLookAngle, playerStats.MaxLookAngle
        );

        replicatedPitch.Value = validatedPitch;
    }
    
}