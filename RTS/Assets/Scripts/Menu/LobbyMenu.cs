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

    public Text player1StatusText;
    public Text player2StatusText;
    public Button playButton;
    public Text waitingForHostText;
    public Slider startingGoldSlider;
    public Slider startingLumberSlider;
    public Button chooseMapButton;
    public GameObject chooseMapMenu;
    public Text chosenMapNameText;
    public GameObject lobbyMainMenu;
    public Text MapLoadErrorMessageText;

    private void Start()
    {
        ((NetworkController)NetworkManager.singleton).SpawnLobbyMenuController();
        LobbyMenuController.InitializeLobbyGUI();
    }

    public void StartGame()
    {
        if (MapLoadController.CheckMap(MultiplayerController.Instance.mapName))
        {
            MultiplayerController.Instance.startingGold = LobbyMenuController.startingGold;
            MultiplayerController.Instance.startingLumber = LobbyMenuController.startingLumber;
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
            LobbyMenuController.startingGold = (int)startingGoldSlider.value;
        }
    }

    public void UpdateStartingLumberValue()
    {
        if (LobbyMenuController.isServer)
        {
            LobbyMenuController.startingLumber = (int)startingLumberSlider.value;
        }
    }
}
