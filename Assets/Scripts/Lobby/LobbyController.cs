using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static LobbyController Instance;
    private readonly List<LobbyPlayer> players = new List<LobbyPlayer>();

    [SerializeField] private RelayManager relayManager;
    [SerializeField] private LobbyUIManager lobbyUI;
    [SerializeField] private string gameplaySceneName = "GameScene";
    public string ChosenPlayerName {get; private set;}

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public void RegisterPlayer(LobbyPlayer player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            player.LobbyDataChanged += RefreshPlayerList;
        }

        RefreshPlayerList();
    }

    public void UnregisterPlayer(LobbyPlayer player)
    {
        if (players.Contains(player))
        {
            player.LobbyDataChanged -= RefreshPlayerList;
            players.Remove(player);
        }

        RefreshPlayerList();
    }

    private void RefreshPlayerList()
    {
        List<LobbyPlayerDisplayData> displayPlayers =
            new List<LobbyPlayerDisplayData>();

        foreach(LobbyPlayer player in players)
        {
            if(player == null)
            {
                continue;
            }

            string playerName =
                player.PlayerName.Value.ToString();

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Connecting...";
            }

            displayPlayers.Add(new LobbyPlayerDisplayData(
                playerName,
                player.IsReady.Value,
                player.IsOwner
                )
            );
        }

        lobbyUI.SetPlayerList(
            displayPlayers,
            ToggleLocalReady
        );
    }

    public void ToggleLocalReady()
    {
        foreach(LobbyPlayer player in players)
        {
            if(player != null && player.IsOwner)
            {
                player.ToggleReady();
                return;
            }
        }
    }

    private void Onestroy()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -=OnClientConnected;
        }

        foreach (LobbyPlayer player in players)
        {
            if(player != null)
            {
                player.LobbyDataChanged -= RefreshPlayerList;
            }
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            lobbyUI.ShowLobby(relayManager.CurrentLobbyCode);
        }
    }

    public void HostGame()
    {
        ChosenPlayerName = lobbyUI.GetPlayerName();

        if (string.IsNullOrEmpty(ChosenPlayerName))
        {
            ChosenPlayerName = "Player";
        }

        _ = relayManager.StartRelayHost();
    }

    public void JoinGame()
    {
        ChosenPlayerName = lobbyUI.GetPlayerName();

        if (string.IsNullOrEmpty(ChosenPlayerName))
        {
            ChosenPlayerName = "Player";
        }

        string joinCode = lobbyUI.GetJoinCode();

        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Join code is empty!");
            return;
        }

        _ = relayManager.StartRelayClient(joinCode);
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the host can start the game");
            return;
        }

        if (!AllPlayersReady())
        {
            Debug.LogWarning("Not all players are ready");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single
        );
    }

    private bool AllPlayersReady()
    {
        if(players.Count == 0)
        {
            return false;
        }

        foreach(LobbyPlayer player in players)
        {
            if(player == null || !player.IsReady.Value)
            {
                return false;
            }
        }

        return true;
    }
}
