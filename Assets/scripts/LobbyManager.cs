using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    [Header("UI Elements")]
    [SerializeField] private Text LobbyNameText;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject PlayerListItemPrefab;
    [SerializeField] private Button ReadyUpButton;
    [SerializeField] private Button StartGameButton;

    public bool havePlayerListItemsBeenCreated = false;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public GameObject localGamePlayerObject;
    public GamePlayer localGamePlayerScript;


    public ulong currentLobbyId;
    // Start is called before the first frame update
    private MyNetworkManager game;
    private MyNetworkManager Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = MyNetworkManager.singleton as MyNetworkManager;
        }
    }
    void Awake()
    {
        MakeInstance();
        ReadyUpButton.gameObject.SetActive(true);
        ReadyUpButton.GetComponentInChildren<Text>().text = "Ready Up";
        StartGameButton.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    public void FindLocalGamePlayer()
    {
        localGamePlayerObject = GameObject.Find("LocalGamePlayer");
        localGamePlayerScript = localGamePlayerObject.GetComponent<GamePlayer>();
    }
    public void UpdateLobbyName()
    {
        Debug.Log("UpdateLobbyName");
        currentLobbyId = Game.GetComponent<SteamLobby>().current_lobbyID;
        string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)currentLobbyId, "name");
        Debug.Log("UpdateLobbyName: new lobby name will be: " + lobbyName);
        LobbyNameText.text = lobbyName;
    }
    public void UpdateUI()
    {
        Debug.Log("Executing UpdateUI");
        if (!havePlayerListItemsBeenCreated)
            CreatePlayerListItems();
        if (playerListItems.Count < Game.GamePlayers.Count)
            CreateNewPlayerListItems();
        if (playerListItems.Count > Game.GamePlayers.Count)
            RemovePlayerListItems();
        if (playerListItems.Count == Game.GamePlayers.Count)
            UpdatePlayerListItems();
    }
    private void CreatePlayerListItems()
    {
        Debug.Log("Executing CreatePlayerListItems. This many players to create: " + Game.GamePlayers.Count.ToString());
        foreach (GamePlayer player in Game.GamePlayers)
        {
            Debug.Log("CreatePlayerListItems: Creating playerlistitem for player: " + player.playerName);
            GameObject newPlayerListItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

            newPlayerListItemScript.playerName = player.playerName;
            newPlayerListItemScript.ConnectionId = player.ConnectionId;
            newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
            newPlayerListItemScript.playerSteamId = player.playerSteamId;
            newPlayerListItemScript.SetPlayerListItemValues();


            newPlayerListItem.transform.SetParent(ContentPanel.transform);
            newPlayerListItem.transform.localScale = Vector3.one;

            playerListItems.Add(newPlayerListItemScript);
        }
        havePlayerListItemsBeenCreated = true;
    }
    private void CreateNewPlayerListItems()
    {
        Debug.Log("Executing CreateNewPlayerListItems");
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (!playerListItems.Any(b => b.ConnectionId == player.ConnectionId))
            {
                Debug.Log("CreateNewPlayerListItems: Player not found in playerListItems: " + player.playerName);
                GameObject newPlayerListItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem newPlayerListItemScript = newPlayerListItem.GetComponent<PlayerListItem>();

                newPlayerListItemScript.playerName = player.playerName;
                newPlayerListItemScript.ConnectionId = player.ConnectionId;
                newPlayerListItemScript.isPlayerReady = player.isPlayerReady;
                newPlayerListItemScript.playerSteamId = player.playerSteamId;
                newPlayerListItemScript.SetPlayerListItemValues();


                newPlayerListItem.transform.SetParent(ContentPanel.transform);
                newPlayerListItem.transform.localScale = Vector3.one;

                playerListItems.Add(newPlayerListItemScript);
            }
        }
       
    }
    private void RemovePlayerListItems()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (!Game.GamePlayers.Any(b => b.ConnectionId == playerListItem.ConnectionId))
            {
                Debug.Log("RemovePlayerListItems: player list item fro connection id: " + playerListItem.ConnectionId.ToString() + " does not exist in the game players list");
                playerListItemsToRemove.Add(playerListItem);
            }
        }
        if (playerListItemsToRemove.Count > 0)
        {
            foreach (PlayerListItem playerListItemToRemove in playerListItemsToRemove)
            {
                GameObject playerListItemToRemoveObject = playerListItemToRemove.gameObject;
                playerListItems.Remove(playerListItemToRemove);
                Destroy(playerListItemToRemoveObject);
                playerListItemToRemoveObject = null;
            }
        }
    }
    private void UpdatePlayerListItems()
    {
        Debug.Log("Executing UpdatePlayerListItems");
        foreach (GamePlayer player in Game.GamePlayers)
        { 
            foreach(PlayerListItem playerListItemScript in playerListItems)
            {   
                if (playerListItemScript.ConnectionId == player.ConnectionId)
                {
                    playerListItemScript.playerName = player.playerName;
                    playerListItemScript.isPlayerReady = player.isPlayerReady;
                    playerListItemScript.SetPlayerListItemValues();
                    if (player == localGamePlayerScript)
                        ChangeReadyUpButtonText();
                }
            }
        }
        CheckIfAllPlayersAreReady();
    }
    public void PlayerReadyUp()
    {
        Debug.Log("Executing PlayerReadyUp");
        localGamePlayerScript.ChangeReadyStatus();
    }
    void ChangeReadyUpButtonText()
    {
        if (localGamePlayerScript.isPlayerReady)
            ReadyUpButton.GetComponentInChildren<Text>().text = "Unready";
        else
            ReadyUpButton.GetComponentInChildren<Text>().text = "Ready Up";
    }
    void CheckIfAllPlayersAreReady()
    {
        Debug.Log("Executing CheckIfAllPlayersAreReady");
        bool areAllPlayersReady = false;
        foreach (GamePlayer player in Game.GamePlayers)
        {
            if (player.isPlayerReady)
            {
                areAllPlayersReady = true;
            }
            else
            {
                Debug.Log("CheckIfAllPlayersAreReady: Not all players are ready. Waiting for: " + player.playerName);
                areAllPlayersReady = false;
                break;
            }
        }
        if (areAllPlayersReady)
        {
            Debug.Log("CheckIfAllPlayersAreReady: All players are ready!");
            if (localGamePlayerScript.IsGameLeader)
            {
                Debug.Log("CheckIfAllPlayersAreReady: Local player is the game leader. They can start the game now.");
                StartGameButton.gameObject.SetActive(true);
            }
        }
        else
        { 
            if(StartGameButton.gameObject.activeInHierarchy)
                StartGameButton.gameObject.SetActive(false);
        }
    }
    public void DestroyPlayerListItems()
    {
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            GameObject playerListItemObject = playerListItem.gameObject;
            Destroy(playerListItemObject);
            playerListItemObject = null;
        }
        playerListItems.Clear();
    }
    public void StartGame()
    {
        localGamePlayerScript.CanLobbyStartGame();
    }
    public void PlayerQuitLobby()
    {
        localGamePlayerScript.QuitLobby();
    }

}
