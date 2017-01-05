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

    private LobbyMenu lobbyMenu;

    public LobbyMenu LobbyMenu
    {
        get
        {
            if (lobbyMenu == null)
            {
                lobbyMenu = FindObjectOfType<LobbyMenu>();
            }
            return lobbyMenu;
        }
        set
        {
            lobbyMenu = value;
        }
    }

    [SyncVar(hook = "OnStartingGoldValueChange")]
    public int startingGold;
    [SyncVar(hook = "OnStartingLumberValueChange")]
    public int startingLumber;
    [SyncVar(hook = "OnMapNameChange")]
    public string mapName;

    public void OnStartingGoldValueChange(int newValue)
    {
        LobbyMenu.startingGoldSlider.value = newValue;
    }

    public void OnStartingLumberValueChange(int newValue)
    {
        LobbyMenu.startingLumberSlider.value = newValue;
    }

    public void OnMapNameChange(string newName)
    {
        mapName = newName;
        LobbyMenu.chosenMapNameText.text = mapName;
        MultiplayerController.Instance.mapName = newName;
        LobbyMenu.chooseMapMenu.SetActive(false);
        LobbyMenu.lobbyMainMenu.SetActive(true);
    }

    public void UpdateStartingGoldValue()
    {
        startingGold = (int)LobbyMenu.startingGoldSlider.value;
    }

    public void UpdateStartingLumberValue()
    {
        startingLumber = (int)LobbyMenu.startingLumberSlider.value;
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
            lobbyMenu = FindObjectOfType<LobbyMenu>();
            DontDestroyOnLoad(gameObject);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!isServer)
        {
            LobbyMenu.playButton.gameObject.SetActive(false);
            LobbyMenu.chooseMapButton.gameObject.SetActive(false);
            LobbyMenu.waitingForHostText.gameObject.SetActive(true);
            LobbyMenu.startingGoldSlider.interactable = false;
            LobbyMenu.startingLumberSlider.interactable = false;
            LobbyMenu.player1StatusText.text = "Connected";
            LobbyMenu.player2StatusText.text = "Connected";
            Debug.LogError(startingGold);
            LobbyMenu.startingGoldSlider.value = startingGold;
            LobbyMenu.startingLumberSlider.value = startingLumber;
        }
        else
        {
            UpdateStartingGoldValue();
            UpdateStartingLumberValue();
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null)
            {
                LobbyMenu.player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.playButton.interactable = true;
            }
        }
    }

    public void InitializeLobbyGUI()
    {
        if (!isServer)
        {
            LobbyMenu.playButton.gameObject.SetActive(false);
            LobbyMenu.chooseMapButton.gameObject.SetActive(false);
            LobbyMenu.waitingForHostText.gameObject.SetActive(true);
            LobbyMenu.startingGoldSlider.interactable = false;
            LobbyMenu.startingLumberSlider.interactable = false;
            LobbyMenu.player1StatusText.text = "Connected";
            LobbyMenu.player2StatusText.text = "Connected";
            LobbyMenu.startingGoldSlider.value = startingGold;
            LobbyMenu.startingLumberSlider.value = startingLumber;
            LobbyMenu.chosenMapNameText.text = mapName;
        }
        else
        {
            //UpdateStartingGoldValue();
            //UpdateStartingLumberValue();
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null)
            {
                LobbyMenu.player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.playButton.interactable = true;
            }
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && LobbyMenu.player1StatusText.text != "Connected")
            {
                LobbyMenu.player1StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null && LobbyMenu.player2StatusText.text != "Connected")
            {
                LobbyMenu.player2StatusText.text = "Connected";
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null && !LobbyMenu.playButton.interactable)
            {
                LobbyMenu.playButton.interactable = true;
            }
        }
    }

    public void OpenChooseMapMenu()
    {
        LobbyMenu.chooseMapMenu.SetActive(true);
    }

    public void CloseChooseMapMenu()
    {
        LobbyMenu.chooseMapMenu.SetActive(false);
    }

    public void SetMapName()
    {
        LobbyMenu.chooseMapMenu.SetActive(false);
    }

    public void StartGame()
    {
        MultiplayerController.Instance.startingGold = startingGold;
        MultiplayerController.Instance.startingLumber = startingLumber;
        MultiplayerController.Instance.StartGame();
    }
}
