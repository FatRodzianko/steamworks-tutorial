using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;
    [Header("UI Stuff")]
    [SerializeField] private GameObject buttons = null;
    [Header("Lobby List UI")]
    [SerializeField] private GameObject LobbyListCanvas;
    [SerializeField] private GameObject LobbyListItemPrefab;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject LobbyListScrollRect;
    [SerializeField] private TMP_InputField searchBox;
    public bool didPlayerSearchForLobbies = false;
    [Header("Create Lobby UI")]
    [SerializeField] private GameObject CreateLobbyCanvas;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle friendsOnlyToggle;
    public bool didPlayerNameTheLobby = false;
    public string lobbyName;

    public List<GameObject> listOfLobbyListItems = new List<GameObject>();
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        buttons.SetActive(true);
        LobbyListCanvas.SetActive(false);
        CreateLobbyCanvas.SetActive(false);
    }
    void Start()
    {
        
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
    public void CreateLobby()
    {
        buttons.SetActive(false);
        CreateLobbyCanvas.SetActive(true);
    }
    public void CreateNewLobby()
    {
        ELobbyType newLobbyType;
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
            lobbyName = lobbyNameInputField.text;
        }

        SteamLobby.instance.CreateNewLobby(newLobbyType);
    }
    public void GetListOfLobbies()
    {
        Debug.Log("Trying to get list of available lobbies ...");
        buttons.SetActive(false);
        LobbyListCanvas.SetActive(true);

        SteamLobby.instance.GetListOfLobbies();
    }
    public void DisplayLobbies(List<CSteamID> lobbyIDS, LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name") + " number of players: " + SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID).ToString() + " max players: " + SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID).ToString());

                if (didPlayerSearchForLobbies)
                {
                    Debug.Log("OnGetLobbyInfo: Player searched for lobbies");
                    if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name").ToLower().Contains(searchBox.text.ToLower()))
                    {
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
    }
    public void DestroyOldLobbyListItems()
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
    public void SearchForLobby()
    {
        if (!string.IsNullOrEmpty(searchBox.text))
        {
            didPlayerSearchForLobbies = true;
        }
        else
            didPlayerSearchForLobbies = false;
        SteamLobby.instance.GetListOfLobbies();
    }
    public void BackToMainMenu()
    {
        buttons.SetActive(true);
        CreateLobbyCanvas.SetActive(false);
        LobbyListCanvas.SetActive(false);

        if (listOfLobbyListItems.Count > 0)
            DestroyOldLobbyListItems();
        lobbyName = null;
        searchBox.text = "";
        lobbyNameInputField.text = "";
        didPlayerSearchForLobbies = false;
        didPlayerNameTheLobby = false;
        friendsOnlyToggle.isOn = false;
    }
}
