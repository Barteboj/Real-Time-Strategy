using UnityEngine;
using System.Collections;
using System;

public class BuildButton : ActionButton
{
    public Building building;

    public void Build()
    {
        try
        {
            if (building.goldCost > Players.Instance.LocalPlayer.GoldAmount)
            {
                MessagesController.Instance.ShowMessage("Not enough gold");
            }
            else
            {
                ((Worker)SelectController.Instance.selectedUnit).PrepareBuild(building);
            }
        }
        catch (InvalidCastException e)
        {
            Debug.LogError("Trying to build with unit that is not a worker");
        }
    }
}
