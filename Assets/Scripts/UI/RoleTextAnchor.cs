using TMPro;
using UnityEngine;

public class RoleTextAnchor : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public static TMP_Text Text { get; private set; }

    private void Awake() => Text = text;

    private void OnDestroy()
    {
        if (Text == text)
            Text = null;
    }
}