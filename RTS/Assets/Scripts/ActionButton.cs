using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum ActionButtonType
{
    BuildBase,
    TrainPeasant,
    TrainWarrior,
    BuildBarracks
}

public class ActionButton : MonoBehaviour
{
    public ActionButtonType buttonType;
    public GameObject objectWithButton;

    public void Show()
    {
        objectWithButton.SetActive(true);
    }

    public void Hide()
    {
        objectWithButton.SetActive(false);
    }
}
