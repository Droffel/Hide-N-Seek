using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay.Models;
using TMPro;
public class RelayManager : MonoBehaviour
{
    public TMP_InputField joinCodeInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void OnClientConnected(ulong clientId)
    {
        Debug.Log("CLIENT CONNECTED! ID: " + clientId);
    }

    public void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("CLIENT DISCONNECTED! ID: " + clientId);
    }
    public void TestHost()
    {
        _ = StartRelayHost();
    }

    private async Task StartRelayHost()
    {
        Debug.Log("Starting Relay host...");

        await UnityServices.InitializeAsync();

        Debug.Log("Unity Services initialized");

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log("Signed in. Player ID: " + AuthenticationService.Instance.PlayerId);

        var allocation = await RelayService.Instance.CreateAllocationAsync(3);

        Debug.Log("Relay allocation created");

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log("JOIN CODE: " + joinCode);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

        bool success = NetworkManager.Singleton.StartHost();

        Debug.Log("Relay host started: " + success);
    }

    // Update is called once per frame
    public void TestJoin()
    {
        _ = StartRelayClient();
    }

    private async Task StartRelayClient()
    {
        try
        {
            string joinCode = joinCodeInput.text.Trim();

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("Join code empty!");
                return;
            }

            Debug.Log("Trying to join Relay with code: " + joinCode);

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Client signed in");

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("Joined Relay allocation");

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            bool success = NetworkManager.Singleton.StartClient();

            Debug.Log("Relay client started: " + success);
        }
        catch(System.Exception e)
        {
            Debug.LogError("Join error:");
            Debug.LogException(e);
        }
    }
}
