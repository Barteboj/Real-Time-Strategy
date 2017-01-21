using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class BuildButton : ActionButton
{
    [SerializeField]
    private BuildingType buildingType;
    public BuildingType BuildingType
    {
        get
        {
            return buildingType;
        }
    }

    private void Awake()
    {
        buttonImage.sprite = Buildings.Instance.GetBuildingPrefab(buildingType, MultiplayerController.Instance.LocalPlayer.PlayerType).GetComponent<Building>().Portrait;
    }

    public override void Act(GameObject executioner)
    {
        Unit actingUnit = executioner.GetComponent<Unit>();
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, actingUnit.Owner).GetComponent<Building>();
        if (buildingToBuild.GoldCost > MultiplayerController.Instance.Players.Find(item => item.PlayerType == actingUnit.Owner).GoldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", actingUnit.Owner);
        }
        else
        {
            ((Worker)actingUnit).PrepareBuild(buildingToBuild);
        }
    }

    public override void GiveActionButtonsControllerToExecuteOnServer()
    {
        
        Unit actingUnit = MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits[0];
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, actingUnit.Owner).GetComponent<Building>();
        if (buildingToBuild.GoldCost > MultiplayerController.Instance.Players.Find(item => item.PlayerType == actingUnit.Owner).GoldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", actingUnit.Owner);
        }
        else
        {
            MultiplayerController.Instance.LocalPlayer.Commander.PlaceBuilding(buildingType);
        }
    }
}
