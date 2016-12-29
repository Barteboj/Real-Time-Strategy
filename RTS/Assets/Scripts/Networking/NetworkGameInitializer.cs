using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameInitializer : NetworkBehaviour
{
    public NetworkGameController networkGameController;

    public void Initialize()
    {
        if (!isServer)
        {
            return;
        }
        GameObject instantiatedNetworkGameController = Instantiate(networkGameController.gameObject);
        NetworkServer.Spawn(instantiatedNetworkGameController);
        instantiatedNetworkGameController.GetComponent<NetworkGameController>().InitializeGame();
    }
}
