using UnityEngine;
using Unity.Netcode;

public class MatchUIController : MonoBehaviour
{
    [SerializeField] private MatchController matchController;
    [SerializeField] private MatchEndUI matchEndUI;

    private void Start()
    {
        matchController.MatchEnded += OnMatchEnded;

        matchEndUI.RematchClicked += OnRematchClicked;
        matchEndUI.MainMenuClicked += OnMainMenuClicked;
    }

    private void OnDestroy()
    {
        if(matchController != null)
        {
            matchController.MatchEnded -= OnMatchEnded;
        }

        if(matchEndUI != null)
        {
            matchEndUI.RematchClicked -= OnRematchClicked;
            matchEndUI.MainMenuClicked -= OnMainMenuClicked;
        }
    }

    private void OnMatchEnded(MatchResult result)
    {
        bool isHost = NetworkManager.Singleton.IsServer;

        matchEndUI.Show(result, isHost);
    }

    private void OnRematchClicked()
    {
        matchController.Rematch();
    }

    private void OnMainMenuClicked()
    {
        matchController.ReturnToMainMenu();
    }
}