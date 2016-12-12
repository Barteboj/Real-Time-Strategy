using UnityEngine;
using System.Collections;

public class Farm : Building
{
    public int foodGrowth;

    public override void FinishBuild()
    {
        base.FinishBuild();
        Players.Instance.LocalPlayer.FoodMaxAmount += foodGrowth;
    }

    public override void DestroyYourself()
    {
        base.DestroyYourself();
        Players.Instance.LocalPlayer.FoodMaxAmount -= foodGrowth;
    }
}
