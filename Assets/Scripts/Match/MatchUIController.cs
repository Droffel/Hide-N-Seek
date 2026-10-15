using UnityEngine;
using Unity.Netcode;

public class MatchUIController : MonoBehaviour
{
    [SerializeField] private MatchController matchController;
    [SerializeField] private MatchTimer matchTimer;
    [SerializeField] private MatchHUDUI matchHUD;
    [SerializeField] private MatchEndUI matchEndUI;

    private void Start()
    {
        matchController.MatchEnded += OnMatchEnded;
        matchEndUI.RematchClicked += OnRematchClicked;
        matchEndUI.MainMenuClicked += OnMainMenuClicked;
        matchTimer.TimeChanged += OnTimeChanged;

        matchHUD.SetTimer(matchTimer.RemainingSeconds);
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

        if (matchTimer != null)
        {
            matchTimer.TimeChanged -= OnTimeChanged;
        }
    }

    private void OnMatchEnded(MatchResult result)
    {

        PlayerInput localPlayerInput = GetLocalPlayerInput();

        if(localPlayerInput != null)
        {
            localPlayerInput.SetInputEnabled(false);
        }
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

    private void OnTimeChanged(int remainingSeconds)
    {
        matchHUD.SetTimer(remainingSeconds);
    }

    private PlayerInput GetLocalPlayerInput()
    {
        PlayerInput[] players = 
            FindObjectsByType<PlayerInput>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach(PlayerInput player in players)
        {
            if (player.IsOwner)
            {
                return player;
            }
        }

        return null;
    }
}