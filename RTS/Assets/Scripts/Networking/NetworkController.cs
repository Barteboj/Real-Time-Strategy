using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkController : NetworkManager
{
    public Text ipAddressText;
    public GameObject multiplayerControllerGameObject;

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
        Instantiate(multiplayerControllerGameObject, Vector3.zero, Quaternion.identity);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Shutdown();
        SceneManager.LoadScene("Offline");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Shutdown();
        SceneManager.LoadScene("Offline");
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
                FindObjectOfType<NetworkGameInitializer>().Initialize();
                readyClientsOnGameScene = 0;
            }
        }
    }
}
