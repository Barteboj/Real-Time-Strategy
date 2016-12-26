using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerOnline : NetworkBehaviour
{
    [SyncVar]
    public int playerID = -1;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    [Command]
    public void CmdSetPlayerId(int id)
    {
        playerID = id;
        RpcSetPlayerJoinedGUI();
        if (id == 1)
        {
            LobbyMenuController.Instance.playButton.interactable = true;
        }
    }

    [ClientRpc]
    public void RpcSetPlayerJoinedGUI()
    {
        if (playerID == 0)
        {
            LobbyMenuController.Instance.player1StatusText.text = "Connected";
        }
        else
        {
            LobbyMenuController.Instance.player2StatusText.text = "Connected";
        }
    }

    void Start()
    {
        if (!isLocalPlayer)
        {
            if (playerID == 0)
            {
                LobbyMenuController.Instance.player1StatusText.text = "Connected";
            }
            else if (playerID == 1)
            {
                LobbyMenuController.Instance.player2StatusText.text = "Connected";
            }
            return;
        }
        if (isServer)
        {
            CmdSetPlayerId(0);
        }
        else if (isClient)
        {
            CmdSetPlayerId(1);
            LobbyMenuController.Instance.playButton.gameObject.SetActive(false);
            LobbyMenuController.Instance.waitingForHostText.gameObject.SetActive(true);
        }
    }
}
