using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MultiplayerController : MonoBehaviour
{
    private PlayerOnline player1;
    private PlayerOnline player2;

    public PlayerOnline Player1
    {
        get
        {
            return player1;
        }
        set
        {
            player1 = value;
        }
    }
    public PlayerOnline Player2
    {
        get
        {
            return player2;
        }
        set
        {
            player2 = value;
        }
    }

    public string gameSceneName;

    private static MultiplayerController instance;

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
}
