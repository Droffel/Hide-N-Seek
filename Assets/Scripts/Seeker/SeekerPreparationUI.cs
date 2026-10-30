using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SeekerPreparationUI : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject blackPanel;
    [SerializeField] private TMP_Text countdownText;

    private SeekerPreparationController preparationController;
    private PlayerRole playerRole;

    private void Awake()
    {
        preparationController =
            GetComponent<SeekerPreparationController>();

        playerRole = GetComponent<PlayerRole>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (blackPanel != null)
                blackPanel.SetActive(false);

            return;
        }

        if (preparationController == null || playerRole == null)
        {
            Debug.LogError(
                "SeekerPreparationUI: Missing preparation controller " +
                "or PlayerRole.",
                this
            );

            return;
        }

        if (blackPanel != null)
            blackPanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner ||
            preparationController == null ||
            playerRole == null ||
            blackPanel == null ||
            countdownText == null)
        {
            return;
        }

        bool showPreparation =
            playerRole.IsSeeker &&
            preparationController.IsPreparing;

        if (blackPanel.activeSelf != showPreparation)
            blackPanel.SetActive(showPreparation);

        if (showPreparation)
        {
            int secondsRemaining =
                Mathf.CeilToInt(preparationController.RemainingTime);

            countdownText.text = secondsRemaining.ToString();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (blackPanel != null)
            blackPanel.SetActive(false);
    }
}