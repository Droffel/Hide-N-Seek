using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class MatchController : NetworkBehaviour
{
    [SerializeField] private string gameplaySceneName = "GameScene";

    public void StartMatch()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the servercan start the match.");
            return;
        }

        Debug.Log("Starting match...");

        NetworkManager.SceneManager.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single
        );
    }
}
