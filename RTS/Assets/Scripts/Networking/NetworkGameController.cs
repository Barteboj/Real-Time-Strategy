using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameController : NetworkBehaviour
{
    public void InitializeGame()
    {
        RpcInitGame();
        RpcInitializePlayer();
    }

    [ClientRpc]
    public void RpcInitializePlayer()
    {
        if (isServer)
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.player1StartingPosition.x, MapLoadController.Instance.player1StartingPosition.y, Camera.main.transform.position.z);
            GameObject instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefabFromUnitType(UnitType.Player1Peasant), MapLoadController.Instance.player1StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefabFromUnitType(UnitType.Player2Peasant), MapLoadController.Instance.player2StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
        }
        else
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.player2StartingPosition.x, MapLoadController.Instance.player2StartingPosition.y, Camera.main.transform.position.z);
        }
    }

    [ClientRpc]
    void RpcInitGame()
    {
        MapLoadController.Instance.LoadChosenMap();
    }
}
