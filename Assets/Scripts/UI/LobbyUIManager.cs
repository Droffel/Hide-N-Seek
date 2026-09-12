using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;

namespace SteamLobbyPanel
{
    public class LobbyUIManager : NetworkBehaviour
    {
        public static LobbyUIManager Instance;
        public Transform playerListParent;
        public List<TextMeshProUGUI> playerNameTexts = new List<TextMeshProUGUI>();
        public List<PlayerLobbyHandler> playerLobbyHandlers = new List<PlayerLobbyHandler>();
        public Button playGameButton;
    

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else if(Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            playGameButton.interactable = false;
        }

        public void UpdatePlayerLobbyUI()
        {
            playerNameTexts.Clear();
            playerLobbyHandlers.Clear();

            var lobby = new CSteamID(SteamLobby.Instance.lobbyID);
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobby);

            string hostAddressStr = SteamMatchmaking.GetLobbyData(lobby, "HostAdress");
            CSteamID hostID = CSteamID.Nil;
            if (!string.IsNullOrEmpty(hostAddressStr))
            {
                ulong.TryParse(hostAddressStr, out ulong parsedHostID);
                hostID = new CSteamID(parsedHostID);
            }

            List<CSteamID> orderedMembers = new List<CSteamID>();

            if(memberCount == 0)
            {
                Debug.LogWarning("Lobby has no members.. retrying...");
                StartCoroutine(RetryUpdate());
                return;
            }

            // Fix: Only add the host if it's a valid ID
            if (hostID != CSteamID.Nil)
            {
                orderedMembers.Add(hostID);
            }

            for(int i = 0; i < memberCount; i++)
            {
                CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);
                // Fix: Only add members if they aren't the host (prevents duplicate host entries)
                if(memberID != hostID)
                {
                    orderedMembers.Add(memberID);
                }
            }

            int j = 0;
            foreach(var member in orderedMembers)
            {
                // Safety Check: If Mirror hasn't instantiated the visual UI slot yet, 
                // break out or skip so it doesn't throw a "Transform child out of bounds" crash.
                if (j >= playerListParent.childCount)
                {
                    Debug.LogWarning($"[LobbyUI] Steam has {orderedMembers.Count} members, but UI only has {playerListParent.childCount} slots ready. Waiting for Mirror to catch up...");
                    break; 
                }

                Transform playerSlot = playerListParent.GetChild(j);
                
                // Safety Check: Make sure the child slot actually has a child UI text element
                if (playerSlot.childCount == 0)
                {
                    Debug.LogError($"[LobbyUI] Player slot at index {j} is missing its TextMeshProUGUI child object!");
                    continue;
                }

                TextMeshProUGUI txtMesh = playerSlot.GetChild(0).GetComponent<TextMeshProUGUI>();
                PlayerLobbyHandler playerLobbyHandler = playerSlot.GetComponent<PlayerLobbyHandler>();

                playerLobbyHandlers.Add(playerLobbyHandler);
                playerNameTexts.Add(txtMesh);

                string playerName = SteamFriends.GetFriendPersonaName(member);
                txtMesh.text = playerName;
                j++;
            }
        }


        public void OnPlayButtonClicked()
        {
            if (NetworkServer.active)
            {
                CustomNetworkManager.singleton.ServerChangeScene("GameplayScene");
            }
        }

        public void RegisterPlayer(PlayerLobbyHandler player)
        {
            player.transform.SetParent(playerListParent, false);
            UpdatePlayerLobbyUI();
        }

        [Server]
        public void CheckAllPlayersReady()
        {
            foreach(var player in playerLobbyHandlers)
            {
                if (!player.isReady)
                {
                    RpcSetPlayButtonInteractable(false);
                    return;
                }
            }
            RpcSetPlayButtonInteractable(true);
        }

        [ClientRpc]
        void RpcSetPlayButtonInteractable(bool truthStatus)
        {
            playGameButton.interactable = truthStatus;
        }

        private IEnumerator RetryUpdate()
        {
            yield return new WaitForSeconds(1f);
            UpdatePlayerLobbyUI();
        }
    }
}

