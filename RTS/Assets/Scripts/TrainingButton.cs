using UnityEngine;
using System.Collections;

public class TrainingButton : ActionButton
{
    public Unit unitToTrain;

    public void Train()
    {
        SelectController.Instance.selectedBuilding.Train(unitToTrain);
    }
}
