using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using System.Text.RegularExpressions;
public class MatchController : NetworkBehaviour
{
    [SerializeField] private MatchTimer matchTimer;
    private readonly NetworkVariable<MatchState> matchState =
        new NetworkVariable<MatchState>(
            MatchState.Starting,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public MatchState CurrentState => matchState.Value;

    public event Action<MatchState> StateChanged;

    public override void OnNetworkSpawn()
    {
        matchState.OnValueChanged += OnMatchStateChanged;

        Debug.Log(
            "MatchController spawned. Current state: "
            + matchState.Value
        );

        if (IsServer)
        {
            matchTimer.TimeExpired += OnTimeExpired;
            BeginMatch();
        }
    }

    public override void OnNetworkDespawn()
    {
        matchState.OnValueChanged -= OnMatchStateChanged;

        if (IsServer)
        {
            matchTimer.TimeExpired -= OnTimeExpired;
        }
    }

    private void OnTimeExpired()
    {
        EndMatch();
    }

    public void BeginMatch()
    {
        if (!IsServer)
        {
            return;
        }

        SetMatchState(MatchState.Playing);
        matchTimer.StartTimer();
    }

    public void EndMatch()
    {
        if (!IsServer)
        {
            return;
        }

        matchTimer.StopTimer();
        SetMatchState(MatchState.Ended);
    }

    private void SetMatchState(MatchState newState)
    {
        if (!IsServer)
        {
            return;
        }

        if(matchState.Value == newState)
        {
            return;
        }

        matchState.Value = newState;
    }

    private void OnMatchStateChanged(MatchState oldState, MatchState newState)
    {
        Debug.Log("Match state changed: " + oldState + " -> " + newState);

        StateChanged?.Invoke(newState);
    }
}
