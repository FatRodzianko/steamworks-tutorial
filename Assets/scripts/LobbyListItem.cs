using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class LobbyListItem : MonoBehaviour
{
    public CSteamID lobbySteamId;
    public string lobbyName;
    public int numberOfPlayers;
    public int maxNumberOfPlayers;

    [SerializeField] private Text LobbyNameText;
    [SerializeField] private Text NumerOfPlayersText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetLobbyItemValues()
    {
        LobbyNameText.text = lobbyName;
        NumerOfPlayersText.text = "Number of Players: " + numberOfPlayers.ToString() + "/" + maxNumberOfPlayers.ToString();
    }
    public void JoinLobby()
    {
        Debug.Log("JoinLobby: Player selected to join lobby with steam id of: " + lobbySteamId.ToString());
        SteamLobby.instance.JoinLobby(lobbySteamId);
    }
}
