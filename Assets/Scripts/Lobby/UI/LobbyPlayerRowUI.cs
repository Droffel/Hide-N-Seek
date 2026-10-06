using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class LobbyPlayerRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;

    public void Setup(
        string playerName,
        bool isReady,
        bool isLocalPlayer,
        Action onReadyClicked)
    {
        nameText.text = playerName;

        readyButtonText.text = isReady ? "Ready" : "Not Ready";

        readyButton.interactable = isLocalPlayer;

        readyButton.onClick.RemoveAllListeners();

        if(isLocalPlayer && onReadyClicked != null)
        {
            readyButton.onClick.AddListener(
                () => onReadyClicked()
            );
        }
    }
}
