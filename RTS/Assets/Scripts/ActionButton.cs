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
    public ActionButtonType buttonType;
    public GameObject objectWithButton;
    public Image buttonImage;

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
