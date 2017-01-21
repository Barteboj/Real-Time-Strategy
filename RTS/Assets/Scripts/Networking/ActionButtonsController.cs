using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ActionButtonsController : NetworkBehaviour
{
    [Command]
    public void CmdExecuteButtonAction(ActionButtonType actionButtonType, NetworkIdentity actingUnitNetworkIdentity)
    {
        ActionButtons.Instance.buttons.Find(item => item.ButtonType == actionButtonType).Act(actingUnitNetworkIdentity.gameObject);
    }

    public void ShowButtons(Unit unit)
    {
        foreach (ActionButtonType buttonType in unit.ButtonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.ButtonType == buttonType).Show();
        }
    }

    public void ShowButtons(Building building)
    {
        foreach (ActionButtonType buttonType in building.ButtonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.ButtonType == buttonType).Show();
        }
    }

    public void HideButtons(Unit unit)
    {
        foreach (ActionButtonType buttonType in unit.ButtonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.ButtonType == buttonType).Hide();
        }
    }

    public void HideButtons(Building building)
    {
        foreach (ActionButtonType buttonType in building.ButtonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.ButtonType == buttonType).Hide();
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
