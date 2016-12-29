using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerOnline : NetworkBehaviour
{
    [SyncVar]
    public int playerID = -1;

    [SyncVar]
    public bool isReady = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    [Command]
    public void CmdSetPlayerId(int id)
    {
        playerID = id;
        if (playerID == 0)
        {
            MultiplayerController.Instance.Player1 = this;
        }
        else
        {
            MultiplayerController.Instance.Player2 = this;
        }
        RpcLol();
    }

    [ClientRpc]
    void RpcLol()
    {
        Debug.Log("Lol");
    }

    void Start()
    {
        if (isServer)
        {
            CmdSetPlayerId(0);
        }
        else if (isClient)
        {
            CmdSetPlayerId(1);
        }
        if (!isServer)
        {
            Debug.Log("Not server");
            return;
        }
    }
}
