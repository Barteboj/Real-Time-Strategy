using UnityEngine;
using System.Collections;

public class Farm : Building
{
    public int foodGrowth;

    public override void FinishBuild()
    {
        base.FinishBuild();
        MultiplayerController.Instance.localPlayer.foodMaxAmount += foodGrowth;
        MultiplayerController.Instance.localPlayer.UpdateResourcesGUI();
    }

    public override void DestroyYourself()
    {
        base.DestroyYourself();
        MultiplayerController.Instance.localPlayer.foodMaxAmount -= foodGrowth;
        MultiplayerController.Instance.localPlayer.UpdateResourcesGUI();
    }
}
