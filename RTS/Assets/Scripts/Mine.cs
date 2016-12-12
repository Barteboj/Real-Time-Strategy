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
    [SerializeField]
    private int goldLeft;

    public int GoldLeft
    {
        get
        {
            return goldLeft;
        }
        set
        {
            goldLeft = value;
            SelectionInfoKeeper.Instance.goldLeftAmountText.text = goldLeft.ToString();
        }
    }

    public int width;
    public int height;

    public IntVector2 placeOnMapGrid;

    public GameObject selectionIndicator;

    public const string mineName = "Mine";

    public Sprite portrait;

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
        if (GoldLeft < amount)
        {
            amount = GoldLeft;
            GoldLeft = 0;
            Deplete();
        }
        else
        {
            GoldLeft -= amount;
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
            SelectionInfoKeeper.Instance.Hide();
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

    public void Select()
    {
        selectionIndicator.SetActive(true);
        SelectionInfoKeeper.Instance.unitName.text = mineName;
        SelectionInfoKeeper.Instance.goldLeftAmountText.text = GoldLeft.ToString();
        SelectionInfoKeeper.Instance.unitLevel.enabled = false;
        SelectionInfoKeeper.Instance.maxHealth.enabled = false;
        SelectionInfoKeeper.Instance.actualHealth.enabled = false;
        SelectionInfoKeeper.Instance.healthInfoGameObject.SetActive(false);
        SelectionInfoKeeper.Instance.levelInfoGameObject.SetActive(false);
        SelectionInfoKeeper.Instance.goldLeftInfoGameObject.SetActive(true);
        SelectionInfoKeeper.Instance.unitPortrait.sprite = portrait;
        SelectController.Instance.selectedUnit = null;
        SelectController.Instance.selectedBuilding = null;
        SelectController.Instance.selectedMine = this;
        SelectionInfoKeeper.Instance.Show();
    }

    public void Unselect()
    {
        selectionIndicator.SetActive(false);
        SelectionInfoKeeper.Instance.unitLevel.enabled = true;
        SelectionInfoKeeper.Instance.maxHealth.enabled = true;
        SelectionInfoKeeper.Instance.actualHealth.enabled = true;
        SelectionInfoKeeper.Instance.healthInfoGameObject.SetActive(true);
        SelectionInfoKeeper.Instance.levelInfoGameObject.SetActive(true);
        SelectionInfoKeeper.Instance.goldLeftInfoGameObject.SetActive(false);
        SelectionInfoKeeper.Instance.Hide();
    }
}
