using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class TrainingButton : ActionButton
{
    [SerializeField]
    private UnitType unitType;
    public UnitType UnitType
    {
        get
        {
            return unitType;
        }
    }

    private void Awake()
    {
        buttonImage.sprite = Units.Instance.GetUnitPrefab(unitType, MultiplayerController.Instance.LocalPlayer.PlayerType).GetComponent<Unit>().Portrait;
    }

    public override void GiveActionButtonsControllerToExecuteOnServer()
    {
        MultiplayerController.Instance.LocalPlayer.ActionButtonsController.CmdExecuteButtonAction(buttonType, MultiplayerController.Instance.LocalPlayer.Selector.SelectedBuilding.GetComponent<NetworkIdentity>());
    }

    public override void Act(GameObject executioner)
    {
        Building selectedBuilding = executioner.GetComponent<Building>();
        PlayerOnline buildingPlayer = MultiplayerController.Instance.Players.Find(item => item.PlayerType == selectedBuilding.Owner);
        Unit unitToTrain = Units.Instance.GetUnitPrefab(unitType, selectedBuilding.Owner).GetComponent<Unit>();
        string messageToShow = "";
        if (unitToTrain.GoldCost > buildingPlayer.GoldAmount || unitToTrain.FoodCost > buildingPlayer.FoodMaxAmount - buildingPlayer.FoodAmount)
        {
            if (unitToTrain.GoldCost > buildingPlayer.GoldAmount)
            {
                messageToShow += "Not enough gold\n";
            }
            if (unitToTrain.FoodCost > buildingPlayer.FoodMaxAmount - buildingPlayer.FoodAmount)
            {
                messageToShow += "Not enough food\n";
            }
            MessagesController.Instance.RpcShowMessage(messageToShow.Remove(messageToShow.Length - 1), buildingPlayer.PlayerType);
        }
        else
        {
            buildingPlayer.GoldAmount -= unitToTrain.GoldCost;
            selectedBuilding.Train(unitToTrain);
        }
    }
}
