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
    public Slider startingGoldSlider;
    public Slider startingLumberSlider;

    [SyncVar(hook = "OnStartingGoldValueChange")]
    public int startingGold = -1;
    [SyncVar(hook = "OnStartingLumberValueChange")]
    public int startingLumber = -1;

    public void OnStartingGoldValueChange(int newValue)
    {
        startingGoldSlider.value = newValue;
    }

    public void OnStartingLumberValueChange(int newValue)
    {
        startingLumberSlider.value = newValue;
    }

    public void UpdateStartingGoldValue()
    {
        startingGold = (int)startingGoldSlider.value;
    }

    public void UpdateStartingLumberValue()
    {
        startingLumber = (int)startingLumberSlider.value;
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
        }
    }

    void Start()
    {
        if (!isServer)
        {
            playButton.gameObject.SetActive(false);
            waitingForHostText.gameObject.SetActive(true);
            startingGoldSlider.interactable = false;
            startingLumberSlider.interactable = false;
            player1StatusText.text = "Connected";
            player2StatusText.text = "Connected";
        }
        else
        {
            UpdateStartingGoldValue();
            UpdateStartingLumberValue();
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null)
            {
                player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                playButton.interactable = true;
            }
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && player1StatusText.text != "Connected")
            {
                player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null && player2StatusText.text != "Connected")
            {
                player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null && !playButton.interactable)
            {
                playButton.interactable = true;
            }
        }
    }

    public void StartGame()
    {
        MultiplayerController.Instance.startingGold = startingGold;
        MultiplayerController.Instance.startingLumber = startingLumber;
        MultiplayerController.Instance.StartGame();
    }
}
