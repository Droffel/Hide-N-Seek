using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using System.Text.RegularExpressions;
using System.Collections;
public class MatchController : NetworkBehaviour
{
    [SerializeField] private MatchTimer matchTimer;
    private readonly NetworkVariable<MatchState> matchState =
        new NetworkVariable<MatchState>(
            MatchState.Starting,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private readonly NetworkVariable<MatchResult> matchResult =
        new NetworkVariable<MatchResult>(
            MatchResult.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public MatchResult CurrentResult => matchResult.Value;

    public MatchState CurrentState => matchState.Value;

    public event Action<MatchState> StateChanged;
    public event Action<MatchResult> MatchEnded;

    public override void OnNetworkSpawn()
    {
        matchState.OnValueChanged += OnMatchStateChanged;
        matchResult.OnValueChanged += OnMatchResultChanged;

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
        matchResult.OnValueChanged -= OnMatchResultChanged;

        if (IsServer)
        {
            matchTimer.TimeExpired -= OnTimeExpired;
        }
    }

    private void OnTimeExpired()
    {
        EndMatch(MatchResult.HidersWin);
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

    public void EndMatch(MatchResult result)
    {
        if (!IsServer)
        {
            return;
        }

        if(matchState.Value == MatchState.Ended)
        {
            return;
        }

        matchTimer.StopTimer();
        matchResult.Value = result;
        SetMatchState(MatchState.Ended);

        Debug.Log("Match ended: " + result);
    }

    public void Rematch()
    {
        if (!IsServer)
        {
            return;
        }

        if(matchState.Value != MatchState.Ended)
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        NetworkManager.SceneManager.LoadScene(currentScene, LoadSceneMode.Single);
    }

    public void ReturnToMainMenu()
    {
        if (!IsServer)
        {
            return;
        }

        ReturnToMainMenuRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ReturnToMainMenuRpc()
    {
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        networkManager.Shutdown();

        yield return null;

        Destroy(networkManager.gameObject);

        yield return null;

        SceneManager.LoadScene("LobbyCreationScene");
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

    private void OnMatchResultChanged(MatchResult oldResult, MatchResult newResult)
    {
        if(newResult == MatchResult.None)
        {
            return;
        }

        MatchEnded?.Invoke(newResult);
    }
}
