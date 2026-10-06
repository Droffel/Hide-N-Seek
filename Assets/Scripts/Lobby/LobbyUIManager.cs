using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{

    [SerializeField] private Transform playerListContainer;
    [SerializeField] private LobbyPlayerRowUI playerRowPrefab;
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;

    public TMP_InputField playerNameInput;
    public TMP_InputField joinCodeInput;

    public TMP_Text lobbyCodeText;
    public TMP_Text playerListText;

    public void ShowLobby(string lobbyCode)
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        lobbyCodeText.text = "Lobby Code: " + lobbyCode;
    }

    public string GetPlayerName()
    {
        return playerNameInput.text.Trim();
    }

    public string GetJoinCode()
    {
        return joinCodeInput.text.Trim();
    }

    public void SetPlayerList(
        List<LobbyPlayerDisplayData> players,
        System.Action onReadyClicked)
    {
        foreach(Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(LobbyPlayerDisplayData player in players)
        {
            LobbyPlayerRowUI row =
                Instantiate(playerRowPrefab, playerListContainer);

            row.Setup(
                player.PlayerName,
                player.IsReady,
                player.IsLocalPlayer,
                onReadyClicked
            );
        }
    }
}

