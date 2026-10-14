using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchEndUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text winnerText;

    [SerializeField] private Button rematchButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private TMP_Text waitingText;

    [SerializeField] private float buttonDelay = 2f;

    public event Action RematchClicked;
    public event Action MainMenuClicked;

    private void Awake()
    {
        panel.SetActive(false);

        rematchButton.onClick.AddListener(
            () => RematchClicked?.Invoke()
        );

        mainMenuButton.onClick.AddListener(
            () => MainMenuClicked?.Invoke()
        );
    }

    public void Show(MatchResult result, bool canControlMatch)
    {
        panel.SetActive(true);

        winnerText.text = result switch
        {
            MatchResult.HidersWin => "HIDERS WIN!",
            MatchResult.SeekerWin => "SEEKERS WIN!",
            _ => "MATCH ENDED"
        };

        rematchButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
        waitingText.gameObject.SetActive(false);

        StartCoroutine(
            ShowOptionsAfterDelay(canControlMatch)
        );
    }

    private IEnumerator ShowOptionsAfterDelay(bool canControlMatch)
    {
        yield return new WaitForSeconds(buttonDelay);

        if (canControlMatch)
        {
            rematchButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
        }
        else
        {
            waitingText.gameObject.SetActive(true);
            waitingText.text = "Waiting for host...";
        }
    }
}