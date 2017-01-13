using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum PlayerType
{
    Player1,
    Player2
}

public class PlayerOnline : NetworkBehaviour
{
    [SyncVar]
    public PlayerType playerType;
    public List<Building> activeBuildings;
    public List<Unit> activeUnits;
    public int allUnitsAmount = 0;
    public int allBuildingsAmount = 0;
    [SyncVar]
    public int kills;
    [SyncVar]
    public int razings;
    [SyncVar]
    public int allGatheredGold;
    [SyncVar]
    public int allGatheredLumber;
    [SyncVar(hook = "OnGoldAmountChange")]
    public int goldAmount;
    [SyncVar(hook = "OnLumberAmountChange")]
    public int lumberAmount;
    [SyncVar(hook = "OnFoodAmountChange")]
    public int foodAmount;
    [SyncVar(hook = "OnFoodMaxAmountChange")]
    public int foodMaxAmount;
    public Commander commander;
    public Selector selector;
    public ActionButtonsController actionButtonsController;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnGoldAmountChange(int newValue)
    {
        goldAmount = newValue;
        if (playerType == MultiplayerController.Instance.localPlayer.playerType)
        {
            ResourcesGUI.Instance.goldText.text = goldAmount.ToString();
        }
    }

    public void OnLumberAmountChange(int newValue)
    {
        lumberAmount = newValue;
        if (playerType == MultiplayerController.Instance.localPlayer.playerType)
        {
            ResourcesGUI.Instance.lumberText.text = lumberAmount.ToString();
        }
    }

    public void OnFoodAmountChange(int newValue)
    {
        foodAmount = newValue;
        if (playerType == MultiplayerController.Instance.localPlayer.playerType)
        {
            ResourcesGUI.Instance.foodText.text = foodAmount.ToString();
        }
    }

    public void OnFoodMaxAmountChange(int newValue)
    {
        foodMaxAmount = newValue;
        if (playerType == MultiplayerController.Instance.localPlayer.playerType)
        {
            ResourcesGUI.Instance.foodMaxText.text = foodMaxAmount.ToString();
        }
    }

    [Command]
    public void CmdUpdateMultiplayerController(NetworkIdentity networkIdentity, PlayerType playerType)
    {
        networkIdentity.GetComponent<PlayerOnline>().playerType = playerType;
        MultiplayerController.Instance.players.Add(networkIdentity.GetComponent<PlayerOnline>());
        foreach (PlayerOnline player in MultiplayerController.Instance.players)
        {
            RpcUpdateMultiplayerController(player.GetComponent<NetworkIdentity>());
        }
    }

    [ClientRpc]
    void RpcUpdateMultiplayerController(NetworkIdentity playerNetworkIdentity)
    {
        if (!MultiplayerController.Instance.players.Contains(playerNetworkIdentity.GetComponent<PlayerOnline>()))
        {
            MultiplayerController.Instance.players.Add(playerNetworkIdentity.GetComponent<PlayerOnline>());
        }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (isServer)
        {
            NetworkServer.Spawn(Instantiate(((NetworkController)NetworkManager.singleton).multiplayerControllerGameObject, Vector3.zero, Quaternion.identity));
            MultiplayerController.Instance.localPlayer = this;
            CmdUpdateMultiplayerController(GetComponent<NetworkIdentity>(), PlayerType.Player1);
        }
        else
        {
            playerType = PlayerType.Player2;
            MultiplayerController.Instance.localPlayer = this;
            CmdUpdateMultiplayerController(GetComponent<NetworkIdentity>(), PlayerType.Player2);
        }
    }
}
