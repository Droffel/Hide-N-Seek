using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;

    public TMP_Text lobbyCodeText;
    public TMP_Text playerListText;

    private List<LobbyPlayer> players = new List<LobbyPlayer>();

    private void Awake()
    {
        Instance = this;
    }

    public void ShowLobby(string lobbyCode)
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        lobbyCodeText.text = "Lobby Code: " + lobbyCode;

        RefreshPlayerList();
    }

    public void RegisterPlayer(LobbyPlayer player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }

        RefreshPlayerList();
    }

    public void UnregisterPlayer(LobbyPlayer player)
    {
        players.Remove(player);

        RefreshPlayerList();
    }

    public void RefreshPlayerList()
    {
        StringBuilder text = new StringBuilder();

        text.AppendLine("Players:");

        foreach(LobbyPlayer player in players)
        {
            if(player == null)
            {
                continue;
            }

            string playerName = player.PlayerName.Value.ToString();

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Connecting...";
            }
            
            text.AppendLine(playerName);
        }

        playerListText.text = text.ToString();
    }
}

