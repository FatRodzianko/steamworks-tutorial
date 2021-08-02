using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using Steamworks;
using UnityEngine.UI;

public class MessageManager : NetworkBehaviour
{
    public static MessageManager instance;
    [SerializeField] private TMP_InputField messageBox;
    [SerializeField] private Text messageText;
    [SyncVar(hook = nameof(HandleNewMessageText))] public string messageTextSynced = "New Text";

    private void Awake()
    {
        MakeInstance();
    }
    // Start is called before the first frame update
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

    public void SendMessageToPlayers()
    {

        if (!string.IsNullOrEmpty(messageBox.text))
        {
            string newMessage = messageBox.text;
            CmdSendMessageToPlayers(newMessage);
        }
    }
    public void HandleNewMessageText(string oldValue, string newValue)
    {
        Debug.Log("HandleNewMessageText with new value " + newValue);
        if (isServer)
            messageTextSynced = newValue;
        if (isClient && (oldValue != newValue))
        {
            UpdateMessageText(newValue);
        }
    }
    void UpdateMessageText(string newMessage)
    {
        messageText.text = newMessage;
    }
    [Command(requiresAuthority = false)]
    void CmdSendMessageToPlayers(string newMessage)
    {
       HandleNewMessageText(messageTextSynced, newMessage);
    }
}
