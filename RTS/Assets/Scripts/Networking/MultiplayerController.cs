using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerController : NetworkBehaviour
{
    public List<PlayerOnline> players = new List<PlayerOnline>();

    public PlayerOnline localPlayer;

    public string gameSceneName;

    private static MultiplayerController instance;

    public int startingGold;

    public int startingLumber;

    public Color[] playerColors;

    public bool isGameInitialized = false;

    public bool hasGameEnded = false;

    public string mapName;

    [SyncVar]
    public PlayerType winner;

    public static MultiplayerController Instance
    {
        get
        {
            if (instance == null)
            {
                MultiplayerController foundInstance = FindObjectOfType<MultiplayerController>();
                instance = foundInstance;
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
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        if (isServer && isGameInitialized && !hasGameEnded)
        {
            if (players.Find(item => item.activeUnits.Count == 0 && item.activeBuildings.Count == 0) != null)
            {
                winner = players.Find(item => item.activeUnits.Count > 0 || item.activeBuildings.Count > 0).playerType;
                NetworkManager.singleton.ServerChangeScene("Ending");
                hasGameEnded = true;
            }
        }
    }

    public void StartGame()
    {
        NetworkManager.singleton.ServerChangeScene(gameSceneName);
    }

    public PlayerOnline GetPlayerByPlayerType(PlayerType playerType)
    {
        return players.Find(item => item.playerType == playerType);
    }

    [ClientRpc]
    public void RpcInitializeGame()
    {
        if (isServer)
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.player1StartingPosition.x, MapLoadController.Instance.player1StartingPosition.y, Camera.main.transform.position.z);
            GameObject instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefab(UnitType.Peasant, PlayerType.Player1), MapLoadController.Instance.player1StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            instantiatedPeasant = Instantiate(Units.Instance.GetUnitPrefab(UnitType.Peasant, PlayerType.Player2), MapLoadController.Instance.player2StartingPosition, Quaternion.identity);
            NetworkServer.Spawn(instantiatedPeasant);
            foreach (PlayerOnline player in players)
            {
                player.goldAmount = startingGold;
                player.lumberAmount = startingLumber;
                player.foodMaxAmount = 1;
            }
            isGameInitialized = true;
        }
        else
        {
            Camera.main.transform.position = new Vector3(MapLoadController.Instance.player2StartingPosition.x, MapLoadController.Instance.player2StartingPosition.y, Camera.main.transform.position.z);
        }
    }
}
