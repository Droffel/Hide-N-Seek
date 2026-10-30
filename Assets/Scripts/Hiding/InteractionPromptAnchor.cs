using UnityEngine;
using TMPro;

public class InteractionPromptAnchor : MonoBehaviour
{
    public static TMP_Text Text { get; private set; }

    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        Text = promptText;
        ClearPrompt();

        if (Text == null)
        {
            Debug.LogError(
                "InteractionPromptAnchor: Prompt Text is not assigned!",
                this
            );
        }
    }

    public void ShowHidePrompt()
    {
        if (Text != null)
        {
            Text.text = "Press E to hide";
        }
    }

    public void ShowCannotHidePrompt()
    {
        if (Text != null)
        {
            Text.text = "✕ Cannot hide here";
        }
    }

    public void ClearPrompt()
    {
        if (Text != null)
        {
            Text.text = "";
        }
    }

    private void OnDestroy()
    {
        if (Text == promptText)
        {
            Text = null;
        }
    }
}