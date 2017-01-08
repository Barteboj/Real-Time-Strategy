using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ActionButtonsController : NetworkBehaviour
{
    [Command]
    public void CmdExecuteButtonAction(ActionButtonType actionButtonType, NetworkIdentity actingUnitNetworkIdentity)
    {
        ActionButtons.Instance.buttons.Find(item => item.buttonType == actionButtonType).Act(actingUnitNetworkIdentity.gameObject);
    }

    public void ShowButtons(Unit unit)
    {
        foreach (ActionButtonType buttonType in unit.buttonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
        }
    }

    public void ShowButtons(Building building)
    {
        foreach (ActionButtonType buttonType in building.buttonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
        }
    }

    public void HideButtons(Unit unit)
    {
        foreach (ActionButtonType buttonType in unit.buttonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Hide();
        }
    }

    public void HideButtons(Building building)
    {
        foreach (ActionButtonType buttonType in building.buttonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Hide();
        }
    }

    public void HideAllButtons()
    {
        foreach (ActionButton button in ActionButtons.Instance.buttons)
        {
            button.Hide();
        }
    }
}
