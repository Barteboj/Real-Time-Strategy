using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyMenuController : NetworkBehaviour
{
    private static LobbyMenuController instance;

    public static LobbyMenuController Instance
    {
        get
        {
            if (instance == null)
            {
                LobbyMenuController newInstance = FindObjectOfType<LobbyMenuController>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not LobbyMenuController attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public Text player1StatusText;
    public Text player2StatusText;
    public Button playButton;
    public Text waitingForHostText;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    [ClientRpc]
    public void RpcGoToGameScene()
    {
        SceneManager.LoadScene("Game");
    }
}
