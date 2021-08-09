using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    [SerializeField] private GamePlayer gamePlayerPrefab;
    [SerializeField] public int minPlayers = 1;
    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();
    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Debug.Log("Starting Server");
        ServerChangeScene("Scene_SteamworksLobby");
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("Client connected.");
        base.OnClientConnect(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Checking if player is in correct scene. Player's scene name is: " + SceneManager.GetActiveScene().name.ToString() + ". Correct scene name is: TitleScreen");
        if (SceneManager.GetActiveScene().name == "Scene_SteamworksLobby")
        {
            bool isGameLeader = GamePlayers.Count == 0; // isLeader is true if the player count is 0, aka when you are the first player to be added to a server/room

            GamePlayer GamePlayerInstance = Instantiate(gamePlayerPrefab);

            GamePlayerInstance.IsGameLeader = isGameLeader;
            GamePlayerInstance.ConnectionId = conn.connectionId;
            GamePlayerInstance.playerNumber = GamePlayers.Count + 1;

            GamePlayerInstance.playerSteamId = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.current_lobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
            Debug.Log("Player added. Player name: " + GamePlayerInstance.playerName + ". Player connection id: " + GamePlayerInstance.ConnectionId.ToString());
        }
    }
    public void StartGame()
    {
        if (CanStartGame() && SceneManager.GetActiveScene().name == "Scene_SteamworksLobby")
        {
            ServerChangeScene("Scene_SteamworksGame");
        }
    }
    private bool CanStartGame()
    {
        if (numPlayers < minPlayers)
            return false;
        foreach (GamePlayer player in GamePlayers)
        {
            if (!player.isPlayerReady)
                return false;
        }
        return true;
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            GamePlayer player = conn.identity.GetComponent<GamePlayer>();
            GamePlayers.Remove(player);
        }
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        GamePlayers.Clear();
    }
    public void HostShutDownServer()
    {
        GameObject NetworkManagerObject = GameObject.Find("NetworkManager");
        Destroy(this.GetComponent<SteamManager>());
        Destroy(NetworkManagerObject);
        Shutdown();
        SceneManager.LoadScene("Scene_Steamworks");

        Start();

    }
}
