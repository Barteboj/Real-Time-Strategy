using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerController : NetworkBehaviour
{
    public List<PlayerOnline> Players { get; set; }
    public PlayerOnline LocalPlayer { get; set; }
    private string gameSceneName = "Game";
    private static MultiplayerController instance;
    public int StartingGold { get; set; }
    public int StartingLumber { get; set; }
    [SerializeField]
    private Color[] playerColors;
    public Color[] PlayerColors
    {
        get
        {
            return playerColors;
        }
    }
    [SyncVar]
    private bool isGameInitialized = false;
    public bool IsGameInitialized
    {
        get
        {
            return isGameInitialized;
        }
    }
    [SyncVar]
    private bool hasGameEnded = false;
    public string MapName { get; set; }

    [SyncVar]
    public PlayerType winner;

    private const string endingSceneName = "Ending";

    public static MultiplayerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MultiplayerController>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            Players = new List<PlayerOnline>();
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        if (isServer && isGameInitialized && !hasGameEnded)
        {
            if (Players.Find(item => item.ActiveUnits.Count == 0 && item.ActiveBuildings.Count == 0) != null)
            {
                winner = Players.Find(item => item.ActiveUnits.Count > 0 || item.ActiveBuildings.Count > 0).PlayerType;
                RpcSetWinner(winner);
                NetworkManager.singleton.ServerChangeScene(endingSceneName);
                hasGameEnded = true;
            }
        }
    }

    [ClientRpc]
    void RpcSetWinner(PlayerType winner)
    {
        this.winner = winner;
        LocalPlayer.Selector.enabled = false;
    }

    public void StartGame()
    {
        NetworkManager.singleton.ServerChangeScene(gameSceneName);
    }

    public PlayerOnline GetPlayerByPlayerType(PlayerType playerType)
    {
        return Players.Find(item => item.PlayerType == playerType);
    }

    [ClientRpc]
    public void RpcInitializeGame()
    {
        if (isServer)
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.Player1StartingPosition.x, MapLoadController.Instance.Player1StartingPosition.y, Camera.main.transform.position.z);
            GameObject instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefab(UnitType.Peasant, PlayerType.Player1), MapLoadController.Instance.Player1StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefab(UnitType.Peasant, PlayerType.Player2), MapLoadController.Instance.Player2StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            foreach (PlayerOnline player in Players)
            {
                player.GoldAmount = StartingGold;
                player.AllGatheredGold = StartingGold;
                player.LumberAmount = StartingLumber;
                player.AllGatheredLumber = StartingLumber;
                player.FoodMaxAmount = 1;
            }
            isGameInitialized = true;
        }
        else
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.Player2StartingPosition.x, MapLoadController.Instance.Player2StartingPosition.y, Camera.main.transform.position.z);
        }
        FindObjectOfType<GameCameraController>().SelectionArea.GetComponent<MinimapElement>().enabled = true;
    }
}
