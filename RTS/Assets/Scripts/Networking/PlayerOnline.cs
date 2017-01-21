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
    private PlayerType playerType;
    public PlayerType PlayerType
    {
        get
        {
            return playerType;
        }
    }
    private List<Building> activeBuildings = new List<Building>();
    public List<Building> ActiveBuildings
    {
        get
        {
            return activeBuildings;
        }
    }
    private List<Unit> activeUnits = new List<Unit>();
    public List<Unit> ActiveUnits
    {
        get
        {
            return activeUnits;
        }
    }
    public int AllUnitsAmount { get; set; }
    public int AllBuildingsAmount { get; set; }
    [SyncVar]
    private int kills;
    public int Kills
    {
        get
        {
            return kills;
        }
        set
        {
            kills = value;
        }
    }
    [SyncVar]
    private int razings;
    public int Razings
    {
        get
        {
            return razings;
        }
        set
        {
            razings = value;
        }
    }
    [SyncVar]
    private int allGatheredGold;
    public int AllGatheredGold
    {
        get
        {
            return allGatheredGold;
        }
        set
        {
            allGatheredGold = value;
        }
    }
    [SyncVar]
    private int allGatheredLumber;
    public int AllGatheredLumber
    {
        get
        {
            return allGatheredLumber;
        }
        set
        {
            allGatheredLumber = value;
        }
    }
    [SyncVar(hook = "OnGoldAmountChange")]
    private int goldAmount;
    public int GoldAmount
    {
        get
        {
            return goldAmount;
        }
        set
        {
            goldAmount = value;
        }
    }
    [SyncVar(hook = "OnLumberAmountChange")]
    private int lumberAmount;
    public int LumberAmount
    {
        get
        {
            return lumberAmount;
        }
        set
        {
            lumberAmount = value;
        }
    }
    [SyncVar(hook = "OnFoodAmountChange")]
    private int foodAmount;
    public int FoodAmount
    {
        get
        {
            return foodAmount;
        }
        set
        {
            foodAmount = value;
        }
    }
    [SyncVar(hook = "OnFoodMaxAmountChange")]
    private int foodMaxAmount;
    public int FoodMaxAmount
    {
        get
        {
            return foodMaxAmount;
        }
        set
        {
            foodMaxAmount = value;
        }
    }
    [SerializeField]
    private Commander commander;
    public Commander Commander
    {
        get
        {
            return commander;
        }
    }
    [SerializeField]
    private Selector selector;
    public Selector Selector
    {
        get
        {
            return selector;
        }
    }
    [SerializeField]
    public ActionButtonsController actionButtonsController;
    public ActionButtonsController ActionButtonsController
    {
        get
        {
            return actionButtonsController;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnGoldAmountChange(int newValue)
    {
        goldAmount = newValue;
        if (playerType == MultiplayerController.Instance.LocalPlayer.playerType)
        {
            ResourcesGUI.Instance.GoldText.text = goldAmount.ToString();
        }
    }

    public void OnLumberAmountChange(int newValue)
    {
        lumberAmount = newValue;
        if (playerType == MultiplayerController.Instance.LocalPlayer.playerType)
        {
            ResourcesGUI.Instance.LumberText.text = lumberAmount.ToString();
        }
    }

    public void OnFoodAmountChange(int newValue)
    {
        foodAmount = newValue;
        if (playerType == MultiplayerController.Instance.LocalPlayer.playerType)
        {
            ResourcesGUI.Instance.FoodText.text = foodAmount.ToString();
        }
    }

    public void OnFoodMaxAmountChange(int newValue)
    {
        foodMaxAmount = newValue;
        if (playerType == MultiplayerController.Instance.LocalPlayer.playerType)
        {
            ResourcesGUI.Instance.FoodMaxText.text = foodMaxAmount.ToString();
        }
    }

    [Command]
    public void CmdUpdateMultiplayerController(NetworkIdentity networkIdentity, PlayerType playerType)
    {
        networkIdentity.GetComponent<PlayerOnline>().playerType = playerType;
        MultiplayerController.Instance.Players.Add(networkIdentity.GetComponent<PlayerOnline>());
        foreach (PlayerOnline player in MultiplayerController.Instance.Players)
        {
            RpcUpdateMultiplayerController(player.GetComponent<NetworkIdentity>());
        }
    }

    [ClientRpc]
    void RpcUpdateMultiplayerController(NetworkIdentity playerNetworkIdentity)
    {
        if (!MultiplayerController.Instance.Players.Contains(playerNetworkIdentity.GetComponent<PlayerOnline>()))
        {
            MultiplayerController.Instance.Players.Add(playerNetworkIdentity.GetComponent<PlayerOnline>());
        }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (isServer)
        {
            NetworkServer.Spawn(Instantiate(((NetworkController)NetworkManager.singleton).MultiplayerControllerGameObject, Vector3.zero, Quaternion.identity));
            MultiplayerController.Instance.LocalPlayer = this;
            CmdUpdateMultiplayerController(GetComponent<NetworkIdentity>(), PlayerType.Player1);
        }
        else
        {
            playerType = PlayerType.Player2;
            MultiplayerController.Instance.LocalPlayer = this;
            CmdUpdateMultiplayerController(GetComponent<NetworkIdentity>(), PlayerType.Player2);
        }
    }
}
