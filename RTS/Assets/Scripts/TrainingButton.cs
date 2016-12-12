using UnityEngine;
using System.Collections;

public class TrainingButton : ActionButton
{
    public Unit unitToTrain;

    public void Train()
    {
        string messageToShow = "";
        if (unitToTrain.goldCost > Players.Instance.LocalPlayer.GoldAmount || unitToTrain.foodCost > Players.Instance.LocalPlayer.FoodMaxAmount - Players.Instance.LocalPlayer.FoodAmount)
        {
            if (unitToTrain.goldCost > Players.Instance.LocalPlayer.GoldAmount)
            {
                messageToShow += "Not enough gold\n";
            }
            if (unitToTrain.foodCost > Players.Instance.LocalPlayer.FoodMaxAmount - Players.Instance.LocalPlayer.FoodAmount)
            {
                messageToShow += "Not enough food\n";
            }
            MessagesController.Instance.ShowMessage(messageToShow.Remove(messageToShow.Length - 1));
        }
        else
        {
            Players.Instance.LocalPlayer.GoldAmount -= unitToTrain.goldCost;
            SelectController.Instance.selectedBuilding.Train(unitToTrain);
        }
    }
}
