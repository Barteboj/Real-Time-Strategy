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
}
