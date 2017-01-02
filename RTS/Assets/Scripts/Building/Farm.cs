using UnityEngine;
using System.Collections;

public class Farm : Building
{
    public int foodGrowth;

    public override void FinishBuild()
    {
        base.FinishBuild();
        if (isServer)
        {
            MultiplayerController.Instance.players.Find(item => item.playerType == owner).foodMaxAmount += foodGrowth;
        }
    }

    public override void DestroyYourself()
    {
        if (isServer)
        {
            MultiplayerController.Instance.players.Find(item => item.playerType == owner).foodMaxAmount -= foodGrowth;
        }
        base.DestroyYourself();
    }
}
