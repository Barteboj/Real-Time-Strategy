using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class EndingGUIController : MonoBehaviour
{
    public Text outcomeText;

    public PlayerStatisticGUI player1GoldStatistic;
    public PlayerStatisticGUI player1LumberStatistic;
    public PlayerStatisticGUI player1UnitsStatistic;
    public PlayerStatisticGUI player1BuildingsStatistic;
    public PlayerStatisticGUI player1KillsStatistic;
    public PlayerStatisticGUI player1RazingsStatistic;
    public PlayerStatisticGUI player2GoldStatistic;
    public PlayerStatisticGUI player2LumberStatistic;
    public PlayerStatisticGUI player2UnitsStatistic;
    public PlayerStatisticGUI player2BuildingsStatistic;
    public PlayerStatisticGUI player2KillsStatistic;
    public PlayerStatisticGUI player2RazingsStatistic;

    private PlayerOnline player1;
    private PlayerOnline player2;

    private void Start()
    {
        if (MultiplayerController.Instance.localPlayer.playerType == MultiplayerController.Instance.winner)
        {
            outcomeText.text = "Victory";
        }
        else
        {
            outcomeText.text = "Defeat";
        }
        PlayerOnline player1 = MultiplayerController.Instance.players.Find(item => item.playerType == PlayerType.Player1);
        PlayerOnline player2 = MultiplayerController.Instance.players.Find(item => item.playerType == PlayerType.Player2);
        player1GoldStatistic.Set(player1.goldAmount, Mathf.Max(player1.goldAmount, player2.goldAmount));
        player1LumberStatistic.Set(player1.lumberAmount, Mathf.Max(player1.lumberAmount, player2.lumberAmount));
        player1UnitsStatistic.Set(player1.allUnitsAmount, Mathf.Max(player1.allUnitsAmount, player2.allUnitsAmount));
        player1BuildingsStatistic.Set(player1.allBuildingsAmount, Mathf.Max(player1.allBuildingsAmount, player2.allBuildingsAmount));
        player1KillsStatistic.Set(player1.kills, Mathf.Max(player1.kills, player2.kills));
        player1RazingsStatistic.Set(player1.razings, Mathf.Max(player1.razings, player2.razings));
        player2GoldStatistic.Set(player2.goldAmount, Mathf.Max(player1.goldAmount, player2.goldAmount));
        player2LumberStatistic.Set(player2.lumberAmount, Mathf.Max(player1.lumberAmount, player2.lumberAmount));
        player2UnitsStatistic.Set(player2.allUnitsAmount, Mathf.Max(player1.allUnitsAmount, player2.allUnitsAmount));
        player2BuildingsStatistic.Set(player2.allBuildingsAmount, Mathf.Max(player1.allBuildingsAmount, player2.allBuildingsAmount));
        player2KillsStatistic.Set(player2.kills, Mathf.Max(player1.kills, player2.kills));
        player2RazingsStatistic.Set(player2.razings, Mathf.Max(player1.razings, player2.razings));
    }

    public void OnMainMenuButtonClick()
    {
        NetworkManager.Shutdown();
        SceneManager.LoadScene("Main Menu");
    }
}
