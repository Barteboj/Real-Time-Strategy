using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    private LobbyMenuController lobbyMenuController;

    public LobbyMenuController LobbyMenuController
    {
        get
        {
            if (lobbyMenuController == null)
            {
                lobbyMenuController = FindObjectOfType<LobbyMenuController>();
            }
            return lobbyMenuController;
        }
    }

    [SerializeField]
    private Text player1StatusText;
    public Text Player1StatusText
    {
        get
        {
            return player1StatusText;
        }
    }
    [SerializeField]
    private Text player2StatusText;
    public Text Player2StatusText
    {
        get
        {
            return player2StatusText;
        }
    }
    [SerializeField]
    private Button playButton;
    public Button PlayButton
    {
        get
        {
            return playButton;
        }
    }
    [SerializeField]
    private Text waitingForHostText;
    public Text WaitingForHostText
    {
        get
        {
            return waitingForHostText;
        }
    }
    [SerializeField]
    private Slider startingGoldSlider;
    public Slider StartingGoldSlider
    {
        get
        {
            return startingGoldSlider;
        }
    }
    [SerializeField]
    private Slider startingLumberSlider;
    public Slider StartingLumberSlider
    {
        get
        {
            return startingLumberSlider;
        }
    }
    [SerializeField]
    private Button chooseMapButton;
    public Button ChooseMapButton
    {
        get
        {
            return chooseMapButton;
        }
    }
    [SerializeField]
    private GameObject chooseMapMenu;
    public GameObject ChooseMapMenu
    {
        get
        {
            return chooseMapMenu;
        }
    }
    [SerializeField]
    private Text chosenMapNameText;
    public Text ChosenMapNameText
    {
        get
        {
            return chosenMapNameText;
        }
    }
    [SerializeField]
    private GameObject lobbyMainMenu;
    public GameObject LobbyMainMenu
    {
        get
        {
            return lobbyMainMenu;
        }
    }
    [SerializeField]
    private Text mapLoadErrorMessageText;
    public Text MapLoadErrorMessageText
    {
        get
        {
            return mapLoadErrorMessageText;
        }
    }

    private void Start()
    {
        ((NetworkController)NetworkManager.singleton).SpawnLobbyMenuController();
        LobbyMenuController.InitializeLobbyGUI();
    }

    public void StartGame()
    {
        if (MapLoadController.CheckMap(MultiplayerController.Instance.MapName))
        {
            MultiplayerController.Instance.StartingGold = LobbyMenuController.StartingGold;
            MultiplayerController.Instance.StartingLumber = LobbyMenuController.StartingLumber;
            NetworkServer.Destroy(LobbyMenuController.gameObject);
            MultiplayerController.Instance.StartGame();
        }
        else
        {
            LobbyMenuController.Instance.ShowMapErrorMessage();
        }
        
    }

    public void GoBackToMainMenu()
    {
        NetworkManager.Shutdown();
        SceneManager.LoadScene("Main Menu");
    }

    public void UpdateStartingGoldValue()
    {
        if (LobbyMenuController.isServer)
        {
            LobbyMenuController.StartingGold = (int)startingGoldSlider.value;
        }
    }

    public void UpdateStartingLumberValue()
    {
        if (LobbyMenuController.isServer)
        {
            LobbyMenuController.StartingLumber = (int)startingLumberSlider.value;
        }
    }
}
