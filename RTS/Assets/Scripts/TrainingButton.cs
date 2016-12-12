using UnityEngine;
using System.Collections;

public class TrainingButton : ActionButton
{
    public Unit unitToTrain;

    public void Train()
    {
        string messageToShow = "";
        if (unitToTrain.goldCost > Players.Instance.LocalPlayer.GoldAmount)
        {
            if (messageToShow != "")
            {
                messageToShow += "\n";
            }
            messageToShow += "Not enough gold";
            MessagesController.Instance.ShowMessage(messageToShow);
        }
        else
        {
            Players.Instance.LocalPlayer.GoldAmount -= unitToTrain.goldCost;
            SelectController.Instance.selectedBuilding.Train(unitToTrain);
        }
    }
}
