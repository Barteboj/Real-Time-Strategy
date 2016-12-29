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

    void Start()
    {
        if (!isServer)
        {
            LobbyMenuController.Instance.playButton.gameObject.SetActive(false);
            LobbyMenuController.Instance.waitingForHostText.gameObject.SetActive(true);
            player1StatusText.text = "Connected";
            player2StatusText.text = "Connected";
        }
        else
        {
            if (MultiplayerController.Instance.Player1 != null)
            {
                player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.Player2 != null)
            {
                player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.Player1 != null && MultiplayerController.Instance.Player2 != null)
            {
                playButton.interactable = true;
            }
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (MultiplayerController.Instance.Player1 != null && player1StatusText.text != "Connected")
            {
                player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.Player2 != null && player2StatusText.text != "Connected")
            {
                player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.Player1 != null && MultiplayerController.Instance.Player2 != null && !playButton.interactable)
            {
                playButton.interactable = true;
            }
        }
    }

    public void StartGame()
    {
        MultiplayerController.Instance.StartGame();
    }
}
