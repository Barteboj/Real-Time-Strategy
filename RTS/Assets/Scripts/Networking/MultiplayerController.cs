using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerController : NetworkBehaviour
{
    public List<PlayerOnline> players = new List<PlayerOnline>();

    public PlayerOnline localPlayer;

    public string gameSceneName;

    private static MultiplayerController instance;

    public int startingGold;

    public int startingLumber;

    public static MultiplayerController Instance
    {
        get
        {
            if (instance == null)
            {
                MultiplayerController foundInstance = FindObjectOfType<MultiplayerController>();
                if (foundInstance != null)
                {
                    instance = foundInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("No Instance of MultiplayerController on scene and it is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartGame()
    {
        NetworkManager.singleton.ServerChangeScene(gameSceneName);
    }

    public PlayerOnline GetPlayerByPlayerType(PlayerType playerType)
    {
        return players.Find(item => item.playerType == playerType);
    }
}
