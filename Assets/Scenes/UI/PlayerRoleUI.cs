using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRoleUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private float displayDuration = 10f;

    private PlayerRole playerRole;
    private Coroutine hideRoutine;

    private void Awake()
    {
        playerRole = GetComponent<PlayerRole>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        StartCoroutine(InitializeRoleUI());
    }

    private IEnumerator InitializeRoleUI()
    {
        while (RoleTextAnchor.Text == null)
        {
            yield return null;
        }

        roleText = RoleTextAnchor.Text;

        if(playerRole == null)
        {
        
            Debug.LogError(
                "PlayerRoleUI: missing PlayerRole.",
                this
            );

            yield break;
        }

        playerRole.OnRoleChanged += UpdateRoleText;

        UpdateRoleText(playerRole.CurrentRole);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && playerRole != null)
        {
            playerRole.OnRoleChanged -= UpdateRoleText;
        }
    }

    private void UpdateRoleText(PlayerRoleType role)
    {
        if (roleText == null)
            return;

        roleText.text = role == PlayerRoleType.Seeker
        ? "You are the SEEKER!"
        : "You are the HIDER!";

        roleText.gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if(roleText != null)
            roleText.gameObject.SetActive(false);

        hideRoutine = null;
    }
}