using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkController : NetworkManager
{
    public Text ipAddressText;
    public GameObject multiplayerControllerGameObject;
    public GameObject lobbyMenuControllerGameObject;

    private int readyClientsOnGameScene = 0;

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
        ServerChangeScene("Lobby");
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name != "Ending")
        {
            Shutdown();
            SceneManager.LoadScene("Offline");
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name != "Ending")
        {
            Shutdown();
            SceneManager.LoadScene("Offline");
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
        if (SceneManager.GetActiveScene().name == "Game")
        {
            ++readyClientsOnGameScene;
            if (readyClientsOnGameScene == 2)
            {
                MapLoadController.Instance.LoadChosenMap();
                MultiplayerController.Instance.RpcInitializeGame();
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
