using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PlayerListItem : MonoBehaviour
{
    public string playerName;
    public int ConnectionId;
    public bool isPlayerReady;

    [SerializeField] private Text PlayerNameText;
    [SerializeField] private Text PlayerReadyStatus;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetPlayerListItemValues()
    {
        PlayerNameText.text = playerName;
        UpdatePlayerItemReadyStatus();
    }
    public void UpdatePlayerItemReadyStatus()
    {
        if (isPlayerReady)
        {
            PlayerReadyStatus.text = "Ready";
            PlayerReadyStatus.color = Color.green;
        }
        else
        {
            PlayerReadyStatus.text = "Not Ready";
            PlayerReadyStatus.color = Color.red;
        }
    }
}
