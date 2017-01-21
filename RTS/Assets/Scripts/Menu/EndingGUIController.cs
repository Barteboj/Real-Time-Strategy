using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class EndingGUIController : MonoBehaviour
{
    [SerializeField]
    private Text outcomeText;
    [SerializeField]
    private PlayerStatisticGUI player1GoldStatistic;
    [SerializeField]
    private PlayerStatisticGUI player1LumberStatistic;
    [SerializeField]
    private PlayerStatisticGUI player1UnitsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player1BuildingsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player1KillsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player1RazingsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player2GoldStatistic;
    [SerializeField]
    private PlayerStatisticGUI player2LumberStatistic;
    [SerializeField]
    private PlayerStatisticGUI player2UnitsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player2BuildingsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player2KillsStatistic;
    [SerializeField]
    private PlayerStatisticGUI player2RazingsStatistic;

    private PlayerOnline player1;
    private PlayerOnline player2;

    private const string playerVictoryText = "Victory";
    private const string playerDefeatText = "Defeat";

    private void Start()
    {
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == MultiplayerController.Instance.winner)
        {
            outcomeText.text = playerVictoryText;
        }
        else
        {
            outcomeText.text = playerDefeatText;
        }
        PlayerOnline player1 = MultiplayerController.Instance.Players.Find(item => item.PlayerType == PlayerType.Player1);
        PlayerOnline player2 = MultiplayerController.Instance.Players.Find(item => item.PlayerType == PlayerType.Player2);
        player1GoldStatistic.Set(player1.AllGatheredGold, Mathf.Max(player1.AllGatheredGold, player2.AllGatheredGold));
        player1LumberStatistic.Set(player1.AllGatheredLumber, Mathf.Max(player1.AllGatheredLumber, player2.AllGatheredLumber));
        player1UnitsStatistic.Set(player1.AllUnitsAmount, Mathf.Max(player1.AllUnitsAmount, player2.AllUnitsAmount));
        player1BuildingsStatistic.Set(player1.AllBuildingsAmount, Mathf.Max(player1.AllBuildingsAmount, player2.AllBuildingsAmount));
        player1KillsStatistic.Set(player1.Kills, Mathf.Max(player1.Kills, player2.Kills));
        player1RazingsStatistic.Set(player1.Razings, Mathf.Max(player1.Razings, player2.Razings));
        player2GoldStatistic.Set(player2.AllGatheredGold, Mathf.Max(player1.AllGatheredGold, player2.AllGatheredGold));
        player2LumberStatistic.Set(player2.AllGatheredLumber, Mathf.Max(player1.AllGatheredLumber, player2.AllGatheredLumber));
        player2UnitsStatistic.Set(player2.AllUnitsAmount, Mathf.Max(player1.AllUnitsAmount, player2.AllUnitsAmount));
        player2BuildingsStatistic.Set(player2.AllBuildingsAmount, Mathf.Max(player1.AllBuildingsAmount, player2.AllBuildingsAmount));
        player2KillsStatistic.Set(player2.Kills, Mathf.Max(player1.Kills, player2.Kills));
        player2RazingsStatistic.Set(player2.Razings, Mathf.Max(player1.Razings, player2.Razings));
    }

    public void OnMainMenuButtonClick()
    {
        NetworkManager.Shutdown();
        SceneManager.LoadScene("Main Menu");
    }
}
