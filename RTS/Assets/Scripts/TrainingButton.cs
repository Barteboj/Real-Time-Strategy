using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class TrainingButton : ActionButton
{
    public UnitType unitType;

    private void Awake()
    {
        buttonImage.sprite = Units.Instance.GetUnitPrefab(unitType, MultiplayerController.Instance.localPlayer.playerType).GetComponent<Unit>().portrait;
    }

    public override void GiveActionButtonsControllerToExecuteOnServer()
    {
        MultiplayerController.Instance.localPlayer.actionButtonsController.CmdExecuteButtonAction(buttonType, MultiplayerController.Instance.localPlayer.selector.selectedBuilding.GetComponent<NetworkIdentity>());
    }

    public override void Act(GameObject executioner)
    {
        Building selectedBuilding = executioner.GetComponent<Building>();
        PlayerOnline buildingPlayer = MultiplayerController.Instance.players.Find(item => item.playerType == selectedBuilding.owner);
        Unit unitToTrain = Units.Instance.GetUnitPrefab(unitType, selectedBuilding.owner).GetComponent<Unit>();
        string messageToShow = "";
        if (unitToTrain.goldCost > buildingPlayer.goldAmount || unitToTrain.foodCost > buildingPlayer.foodMaxAmount - buildingPlayer.foodAmount)
        {
            if (unitToTrain.goldCost > buildingPlayer.goldAmount)
            {
                messageToShow += "Not enough gold\n";
            }
            if (unitToTrain.foodCost > buildingPlayer.foodMaxAmount - buildingPlayer.foodAmount)
            {
                messageToShow += "Not enough food\n";
            }
            MessagesController.Instance.RpcShowMessage(messageToShow.Remove(messageToShow.Length - 1), buildingPlayer.playerType);
        }
        else
        {
            buildingPlayer.goldAmount -= unitToTrain.goldCost;
            selectedBuilding.Train(unitToTrain);
        }
    }
}
