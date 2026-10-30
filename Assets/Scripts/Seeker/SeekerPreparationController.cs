using Unity.Netcode;
using UnityEngine;

public class SeekerPreparationController : NetworkBehaviour
{
    public const float PreparationDuration = 30f;

    private readonly NetworkVariable<double> preparationEndTime =
        new NetworkVariable<double>(
            0d,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool IsPreparing =>
        preparationEndTime.Value > 0d &&
        NetworkManager.Singleton != null &&
        NetworkManager.Singleton.ServerTime.Time <
            preparationEndTime.Value;

    public float RemainingTime
    {
        get
        {
            if (NetworkManager.Singleton == null ||
                preparationEndTime.Value <= 0d)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                (float)(
                    preparationEndTime.Value -
                    NetworkManager.Singleton.ServerTime.Time
                )
            );
        }
    }

    public void BeginPreparation()
    {
        if (!IsServer)
            return;

        preparationEndTime.Value =
            NetworkManager.Singleton.ServerTime.Time +
            PreparationDuration;

        Debug.Log(
        $"[Preparation] Started for client {OwnerClientId}. " +
        $"End time: {preparationEndTime.Value}"
        );
    }

    public override void OnNetworkSpawn()
    {
        preparationEndTime.OnValueChanged += OnPreparationChanged;
    }

    public override void OnNetworkDespawn()
    {
        preparationEndTime.OnValueChanged -= OnPreparationChanged;
    }

    private void OnPreparationChanged(double oldValue, double newValue)
    {
        // The UI reads the synchronized end time.
        // No additional state needs to be stored here.
    }
}