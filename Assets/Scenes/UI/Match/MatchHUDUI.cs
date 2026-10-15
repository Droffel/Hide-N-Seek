using TMPro;
using UnityEngine;

public class MatchHUDUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    public void SetTimer(int totalSeconds)
    {
        int minutes = totalSeconds/60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
