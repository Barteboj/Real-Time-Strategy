using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameController : NetworkBehaviour
{
    public void InitializeGame()
    {
        RpcInitializePlayer(MultiplayerController.Instance.startingGold, MultiplayerController.Instance.startingLumber);
    }

    [ClientRpc]
    public void RpcInitializePlayer(int startingGold, int startingLumber)
    {
        MapLoadController.Instance.LoadChosenMap();
        if (isServer)
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.player1StartingPosition.x, MapLoadController.Instance.player1StartingPosition.y, Camera.main.transform.position.z);
            GameObject instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefab(UnitType.Peasant, PlayerType.Player1), MapLoadController.Instance.player1StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefab(UnitType.Peasant, PlayerType.Player2), MapLoadController.Instance.player2StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            foreach(PlayerOnline player in MultiplayerController.Instance.players)
            {
                player.goldAmount = startingGold;
                player.lumberAmount = startingLumber;
                player.foodMaxAmount = 1;
            }
            MultiplayerController.Instance.isGameInitialized = true;
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
