using UnityEngine;
using System.Collections;

public class Farm : Building
{
    [SerializeField]
    private int foodGrowth;

    public override void FinishBuild()
    {
        base.FinishBuild();
        if (isServer)
        {
            MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).FoodMaxAmount += foodGrowth;
        }
    }

    public override void DestroyYourself()
    {
        if (isServer)
        {
            MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).FoodMaxAmount -= foodGrowth;
        }
        base.DestroyYourself();
    }
}
