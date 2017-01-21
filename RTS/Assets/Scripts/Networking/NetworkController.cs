using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkController : NetworkManager
{
    [SerializeField]
    private Text ipAddressText;
    [SerializeField]
    private GameObject multiplayerControllerGameObject;
    public GameObject MultiplayerControllerGameObject
    {
        get
        {
            return multiplayerControllerGameObject;
        }
    }
    [SerializeField]
    private GameObject lobbyMenuControllerGameObject;

    private int readyClientsOnGameScene = 0;

    private const string lobbySceneName = "Lobby";
    private const string endingSceneName = "Ending";
    private const string offlineSceneName = "Offline";
    private const string gameSceneName = "Game";

    public void HostGame()
    {
        StartHost();
    }

    public void JoinGame()
    {
        networkAddress = ipAddressText.text;
        StartClient();
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        ServerChangeScene(lobbySceneName);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name != endingSceneName)
        {
            Shutdown();
            SceneManager.LoadScene(offlineSceneName);
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name != endingSceneName)
        {
            Shutdown();
            SceneManager.LoadScene(offlineSceneName);
        }
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        if (!ClientScene.ready)
        {
            ClientScene.Ready(conn);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            ++readyClientsOnGameScene;
            if (readyClientsOnGameScene == 2)
            {
                if (MapLoadController.CheckMap(MultiplayerController.Instance.MapName))
                {
                    MapLoadController.Instance.LoadChosenMap();
                    MultiplayerController.Instance.RpcInitializeGame();
                }
                else
                {
                    LobbyMenuController.Instance.ShowMapErrorMessage();
                }
            }
        }
    }

    public void SpawnLobbyMenuController()
    {
        if (FindObjectOfType<NetworkIdentity>().isServer)
        {
            NetworkServer.Spawn(Instantiate(((NetworkController)NetworkManager.singleton).lobbyMenuControllerGameObject, Vector3.zero, Quaternion.identity));
        }
    }
}
