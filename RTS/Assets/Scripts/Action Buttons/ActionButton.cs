using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public enum ActionButtonType
{
    BuildBase,
    TrainPeasant,
    TrainWarrior,
    BuildBarracks,
    BuildFarm,
    BuildShootingRange,
    TrainArcher
}

public abstract class ActionButton : MonoBehaviour
{
    [SerializeField]
    protected ActionButtonType buttonType;
    public ActionButtonType ButtonType
    {
        get
        {
            return buttonType;
        }
    }
    [SerializeField]
    private GameObject objectWithButton;
    [SerializeField]
    protected Image buttonImage;

    public void Show()
    {
        objectWithButton.SetActive(true);
    }

    public void Hide()
    {
        objectWithButton.SetActive(false);
    }

    public abstract void Act(GameObject executioner);
    public abstract void GiveActionButtonsControllerToExecuteOnServer();
}
