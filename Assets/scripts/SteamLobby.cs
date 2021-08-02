using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;
    /*[Header("UI Stuff")]
    [SerializeField] private GameObject buttons = null;
    [Header("Lobby List UI")]
    [SerializeField] private GameObject LobbyListCanvas;
    [SerializeField] private GameObject LobbyListItemPrefab;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject LobbyListScrollRect;
    [SerializeField] private TMP_InputField searchBox;
    public bool didPlayerSearchForLobbies = false;*/
    /*[Header("Create Lobby UI")]
    [SerializeField] private GameObject CreateLobbyCanvas;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle friendsOnlyToggle;
    public bool didPlayerNameTheLobby = false;*/

    private List<GameObject> listOfLobbyListItems = new List<GameObject>();

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> Callback_lobbyList;
    protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;

    public ulong current_lobbyID;
    public List<CSteamID> lobbyIDS = new List<CSteamID>();

    private const string HostAddressKey = "HostAddress";

    private NetworkManager networkManager;

    struct LobbyMetaData
    {
        public string m_Key;
        public string m_Value;
    }

    struct LobbyMembers
    {
        public CSteamID m_SteamID;
        public LobbyMetaData[] m_Data;
    }
    struct Lobby
    {
        public CSteamID m_SteamID;
        public CSteamID m_Owner;
        public LobbyMembers[] m_Members;
        public int m_MemberLimit;
        public LobbyMetaData[] m_Data;
    }

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized) { return; }
        MakeInstance();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        Callback_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    public void HostLobby()
    {
        //buttons.SetActive(false);

        //SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
        //CreateLobbyCanvas.SetActive(true);
    }
    public void JoinLobby(CSteamID lobbyId)
    {
        Debug.Log("JoinLobby: Will try to join lobby with steam id: " + lobbyId.ToString());
        SteamMatchmaking.JoinLobby(lobbyId);
    }
    public void GetListOfLobbies()
    {
        /*Debug.Log("Trying to get list of available lobbies ...");
        buttons.SetActive(false);
        LobbyListCanvas.SetActive(true);*/

        if (lobbyIDS.Count > 0)
            lobbyIDS.Clear();

        SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
        
        SteamAPICall_t try_getList = SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("OnLobbyCreated");
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            //buttons.SetActive(true);
            return;
        }

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey,
            SteamUser.GetSteamID().ToString());
        if (MainMenuManager.instance.didPlayerNameTheLobby)
        {
            SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "name",
            MainMenuManager.instance.lobbyName);
            MainMenuManager.instance.didPlayerNameTheLobby = false;
        }
        else
        {
            SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "name",
            SteamFriends.GetPersonaName().ToString() + "'s lobby");
        }
        
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("OnGameLobbyJoinRequested");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        
        current_lobbyID = callback.m_ulSteamIDLobby;
        Debug.Log("OnLobbyEntered for lobby with id: " + current_lobbyID.ToString());
        if (NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
        lobbyIDS.Clear();
        if (GameObject.Find("MainMenuManager"))
        {
            if (MainMenuManager.instance.listOfLobbyListItems.Count > 0)
                MainMenuManager.instance.DestroyOldLobbyListItems();
        }
        //buttons.SetActive(false);
    }
    void OnGetLobbiesList(LobbyMatchList_t result)
    {
        Debug.Log("Found " + result.m_nLobbiesMatching + " lobbies!");
        if (MainMenuManager.instance.listOfLobbyListItems.Count > 0)
            MainMenuManager.instance.DestroyOldLobbyListItems();
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyID);
            //Debug.Log("Lobby id is: " + lobbyID.m_SteamID.ToString());
            //ParseLobbyData(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
            //SteamNetworkingUtils.EstimatePingTimeFromLocalHost
        }
        
    }
    void OnGetLobbyInfo(LobbyDataUpdate_t result)
    {

        /*for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name") + " number of players: " + SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID).ToString() + " max players: " +  SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID).ToString());

                if (didPlayerSearchForLobbies)
                {
                    Debug.Log("OnGetLobbyInfo: Player searched for lobbies");
                    if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name").ToLower().Contains(searchBox.text))
                    {
                        GameObject newLobbyListItem = Instantiate(LobbyListItemPrefab) as GameObject;
                        LobbyListItem newLobbyListItemScript = newLobbyListItem.GetComponent<LobbyListItem>();

                        newLobbyListItemScript.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
                        newLobbyListItemScript.numberOfPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID);
                        newLobbyListItemScript.maxNumberOfPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID);
                        newLobbyListItemScript.SetLobbyItemValues();


                        newLobbyListItem.transform.SetParent(ContentPanel.transform);
                        newLobbyListItem.transform.localScale = Vector3.one;

                        listOfLobbyListItems.Add(newLobbyListItem);
                    }                    
                }
                else
                {
                    Debug.Log("OnGetLobbyInfo: Player DID NOT search for lobbies");
                    GameObject newLobbyListItem = Instantiate(LobbyListItemPrefab) as GameObject;
                    LobbyListItem newLobbyListItemScript = newLobbyListItem.GetComponent<LobbyListItem>();

                    newLobbyListItemScript.lobbySteamId = (CSteamID)lobbyIDS[i].m_SteamID;
                    newLobbyListItemScript.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
                    newLobbyListItemScript.numberOfPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID);
                    newLobbyListItemScript.maxNumberOfPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID);
                    newLobbyListItemScript.SetLobbyItemValues();


                    newLobbyListItem.transform.SetParent(ContentPanel.transform);
                    newLobbyListItem.transform.localScale = Vector3.one;

                    listOfLobbyListItems.Add(newLobbyListItem);
                }
                
                return;
            }
        }
        if (didPlayerSearchForLobbies)
            didPlayerSearchForLobbies = false;
        //LobbyListScrollRect.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        */
        MainMenuManager.instance.DisplayLobbies(lobbyIDS, result);
    }
    void DestroyOldLobbyListItems()
    {
        Debug.Log("DestroyOldLobbyListItems");
        foreach (GameObject lobbyListItem in listOfLobbyListItems)
        {
            GameObject lobbyListItemToDestroy = lobbyListItem;
            Destroy(lobbyListItemToDestroy);
            lobbyListItemToDestroy = null;
        }
        listOfLobbyListItems.Clear();
    }
    /*public void SearchForLobby()
    {
        if (!string.IsNullOrEmpty(searchBox.text))
        {
            didPlayerSearchForLobbies = true;            
        }
        else
            didPlayerSearchForLobbies = false;
        GetListOfLobbies();
    }*/
    public void CreateNewLobby(ELobbyType lobbyType)
    {
        /*ELobbyType newLobbyType;
        if (friendsOnlyToggle.isOn)
        {
            Debug.Log("CreateNewLobby: friendsOnlyToggle is on. Making lobby friends only.");
            newLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        }
        else
        {
            Debug.Log("CreateNewLobby: friendsOnlyToggle is OFF. Making lobby public.");
            newLobbyType = ELobbyType.k_ELobbyTypePublic;
        }

        if (!string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            Debug.Log("CreateNewLobby: player created a lobby name of: " + lobbyNameInputField.text);
            didPlayerNameTheLobby = true;
        }*/
        
        SteamMatchmaking.CreateLobby(lobbyType, networkManager.maxConnections);
    }
    /*void ParseLobbyData(CSteamID lobbyId)
    {
        Lobby lobby = new Lobby();
        lobby.m_SteamID = lobbyId; // ID, which was passed to the method
        lobby.m_Owner = SteamMatchmaking.GetLobbyOwner(lobbyId);
        lobby.m_Members = new LobbyMembers[SteamMatchmaking.GetNumLobbyMembers(lobbyId)];
        lobby.m_MemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);

        int DataCount = SteamMatchmaking.GetLobbyDataCount(lobbyId);

        lobby.m_Data = new LobbyMetaData[DataCount];
        for (int i = 0; i < DataCount; i++) // Getting all the lobby metadata
        {
            bool lobbyDataRet = SteamMatchmaking.GetLobbyDataByIndex(lobbyId, i, out lobby.m_Data[i].m_Key,
                Constants.k_nMaxLobbyKeyLength, out lobby.m_Data[i].m_Value, Constants.k_cubChatMetadataMax);

            if (!lobbyDataRet)
            {
                Debug.LogError("Error retrieving lobby metadata");
                continue;
            }
        }
        Debug.Log("Lobby info: Lobby Id: " + lobby.m_SteamID.ToString() + " Lobby owner id: " + lobby.m_Owner.ToString() + " number of players in lobby: " + lobby.m_Members.Length.ToString() + " max number of players allowed in lobby: " + lobby.m_MemberLimit.ToString());

    }*/
}
