using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mine : MonoBehaviour
{
    public Sprite nonVisitedSprite;
    public Sprite visitedSprite;
    public SpriteRenderer spriteRenderer;

    public float timeOfMining;

    public List<Worker> miners;

    public int goldLeft;

    public int width;
    public int height;

    public IntVector2 placeOnMapGrid;

    void Start()
    {
        FillPositionInGrid();
    }

    public void FillPositionInGrid()
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].mine = this;
            }
        }
    }
    
    public int TakeGold(int amount)
    {
        if (goldLeft < amount)
        {
            amount = goldLeft;
            goldLeft = 0;
            Deplete();
        }
        else
        {
            goldLeft -= amount;
        }
        return amount;
    }

    public void VisitMine(Worker visiter)
    {
        miners.Add(visiter);
        spriteRenderer.sprite = visitedSprite;
        if (SelectController.Instance.selectedUnit == visiter)
        {
            SelectController.Instance.Unselect();
            visiter.Unselect();
        }
        visiter.ClearPositionInGrid();
        visiter.gameObject.SetActive(false);
        StartCoroutine(Mining(visiter));
    }

    public void LeaveMine(Worker visiter)
    {
        miners.Remove(visiter);
        if (miners.Count == 0)
        {
            spriteRenderer.sprite = nonVisitedSprite;
        }
        IntVector2 firstFreePlaceOnMapAroundMine = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(gameObject.transform.position), width, height);
        visiter.SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundMine);
        visiter.gameObject.SetActive(true);
        visiter.ReturnWithGold();
    }

    public void Deplete()
    {
        //Destroy(gameObject);
    }

    public IEnumerator Mining(Worker miner)
    {
        yield return new WaitForSeconds(timeOfMining);
        miner.takenGoldAmount = TakeGold(100);
        LeaveMine(miner);
    }
}
