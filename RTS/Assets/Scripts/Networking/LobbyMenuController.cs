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
                instance = FindObjectOfType<LobbyMenuController>();
            }
            return instance;
        }
    }

    private LobbyMenu lobbyMenu;

    private Coroutine MapLoadErrorMessageCoroutine;

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
    private int startingGold;
    public int StartingGold
    {
        get
        {
            return startingGold;
        }
        set
        {
            startingGold = value;
        }
    }
    [SyncVar(hook = "OnStartingLumberValueChange")]
    private int startingLumber;
    public int StartingLumber
    {
        get
        {
            return startingLumber;
        }
        set
        {
            startingLumber = value;
        }
    }
    [SyncVar(hook = "OnMapNameChange")]
    private string mapName;
    public string MapName
    {
        get
        {
            return mapName;
        }
        set
        {
            mapName = value;
        }
    }

    private const string playerConnectedText = "Connected";

    public void OnStartingGoldValueChange(int newValue)
    {
        LobbyMenu.StartingGoldSlider.value = newValue;
    }

    public void OnStartingLumberValueChange(int newValue)
    {
        LobbyMenu.StartingLumberSlider.value = newValue;
    }

    public void OnMapNameChange(string newName)
    {
        mapName = newName;
        LobbyMenu.ChosenMapNameText.text = mapName;
        MultiplayerController.Instance.MapName = newName;
        LobbyMenu.ChooseMapMenu.SetActive(false);
        LobbyMenu.LobbyMainMenu.SetActive(true);
    }

    public void UpdateStartingGoldValue()
    {
        startingGold = (int)LobbyMenu.StartingGoldSlider.value;
    }

    public void UpdateStartingLumberValue()
    {
        startingLumber = (int)LobbyMenu.StartingLumberSlider.value;
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
            LobbyMenu.PlayButton.gameObject.SetActive(false);
            LobbyMenu.ChooseMapButton.gameObject.SetActive(false);
            LobbyMenu.WaitingForHostText.gameObject.SetActive(true);
            LobbyMenu.StartingGoldSlider.interactable = false;
            LobbyMenu.StartingLumberSlider.interactable = false;
            LobbyMenu.Player1StatusText.text = playerConnectedText;
            LobbyMenu.Player2StatusText.text = playerConnectedText;
            LobbyMenu.StartingGoldSlider.value = startingGold;
            LobbyMenu.StartingLumberSlider.value = startingLumber;
        }
        else
        {
            UpdateStartingGoldValue();
            UpdateStartingLumberValue();
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null)
            {
                LobbyMenu.Player1StatusText.text = playerConnectedText;
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.Player2StatusText.text = playerConnectedText;
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.PlayButton.interactable = true;
            }
        }
    }

    public IEnumerator MapLoadErrorMessageRoutine()
    {
        LobbyMenu.MapLoadErrorMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        LobbyMenu.MapLoadErrorMessageText.gameObject.SetActive(false);
    }

    public void ShowMapErrorMessage()
    {
        if (MapLoadErrorMessageCoroutine != null)
        {
            StopCoroutine(MapLoadErrorMessageCoroutine);
        }
        MapLoadErrorMessageCoroutine = StartCoroutine(MapLoadErrorMessageRoutine());
    }

    public void InitializeLobbyGUI()
    {
        if (!isServer)
        {
            LobbyMenu.PlayButton.gameObject.SetActive(false);
            LobbyMenu.ChooseMapButton.gameObject.SetActive(false);
            LobbyMenu.WaitingForHostText.gameObject.SetActive(true);
            LobbyMenu.StartingGoldSlider.interactable = false;
            LobbyMenu.StartingLumberSlider.interactable = false;
            LobbyMenu.Player1StatusText.text = playerConnectedText;
            LobbyMenu.Player2StatusText.text = playerConnectedText;
            LobbyMenu.StartingGoldSlider.value = startingGold;
            LobbyMenu.StartingLumberSlider.value = startingLumber;
            if (mapName != "")
            {
                LobbyMenu.ChosenMapNameText.text = mapName;
            }
        }
        else
        {
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null)
            {
                LobbyMenu.Player1StatusText.text = playerConnectedText;
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.Player2StatusText.text = playerConnectedText;
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null)
            {
                LobbyMenu.PlayButton.interactable = true;
            }
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && LobbyMenu.Player1StatusText.text != playerConnectedText)
            {
                LobbyMenu.Player1StatusText.text = playerConnectedText;
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null && LobbyMenu.Player2StatusText.text != playerConnectedText)
            {
                LobbyMenu.Player2StatusText.text = playerConnectedText;
            }
            if (MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player1) != null && MultiplayerController.Instance.GetPlayerByPlayerType(PlayerType.Player2) != null && !LobbyMenu.PlayButton.interactable)
            {
                LobbyMenu.PlayButton.interactable = true;
            }
        }
    }

    public void OpenChooseMapMenu()
    {
        LobbyMenu.ChooseMapMenu.SetActive(true);
    }

    public void CloseChooseMapMenu()
    {
        LobbyMenu.ChooseMapMenu.SetActive(false);
    }

    public void SetMapName()
    {
        LobbyMenu.ChooseMapMenu.SetActive(false);
    }

    public void StartGame()
    {
        MultiplayerController.Instance.StartingGold = startingGold;
        MultiplayerController.Instance.StartingLumber = startingLumber;
        MultiplayerController.Instance.StartGame();
    }
}
