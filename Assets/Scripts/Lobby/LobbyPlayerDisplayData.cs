public struct LobbyPlayerDisplayData
{
    public string PlayerName;
    public bool IsReady;
    public bool IsLocalPlayer;

    public LobbyPlayerDisplayData(
        string playerName,
        bool isReady,
        bool isLocalPlayer)
    {
        PlayerName = playerName;
        IsReady = isReady;
        IsLocalPlayer = isLocalPlayer;
    }
}
