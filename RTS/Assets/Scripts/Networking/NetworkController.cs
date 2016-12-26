using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkController : NetworkManager
{
    public Text ipAddressText;

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
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        //PlayersOnline.Instance.AddPlayer()
        SceneManager.LoadScene("Lobby");
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        Debug.Log("Couldn't connect");
    }
}
