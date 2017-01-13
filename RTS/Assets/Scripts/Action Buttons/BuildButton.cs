using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class BuildButton : ActionButton
{
    public BuildingType buildingType;

    private void Awake()
    {
        buttonImage.sprite = Buildings.Instance.GetBuildingPrefab(buildingType, MultiplayerController.Instance.localPlayer.playerType).GetComponent<Building>().portrait;
    }

    public override void Act(GameObject executioner)
    {
        Unit actingUnit = executioner.GetComponent<Unit>();
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, actingUnit.owner).GetComponent<Building>();
        if (buildingToBuild.goldCost > MultiplayerController.Instance.players.Find(item => item.playerType == actingUnit.owner).goldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", actingUnit.owner);
        }
        else
        {
            ((Worker)actingUnit).PrepareBuild(buildingToBuild);
        }
    }

    public override void GiveActionButtonsControllerToExecuteOnServer()
    {
        
        Unit actingUnit = MultiplayerController.Instance.localPlayer.selector.selectedUnits[0];
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, actingUnit.owner).GetComponent<Building>();
        if (buildingToBuild.goldCost > MultiplayerController.Instance.players.Find(item => item.playerType == actingUnit.owner).goldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", actingUnit.owner);
        }
        else
        {
            MultiplayerController.Instance.localPlayer.commander.PlaceBuilding(buildingType);
        }
    }
}
