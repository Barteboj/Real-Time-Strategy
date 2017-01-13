using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Mine : NetworkBehaviour
{
    private bool toDestroy = false;

    public Sprite nonVisitedSprite;
    public Sprite visitedSprite;
    public SpriteRenderer spriteRenderer;

    public float timeOfMining;

    public List<Worker> miners;
    [SerializeField]
    [SyncVar(hook = "OnGoldLeftChange")]
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

    private void Update()
    {
        if (toDestroy && miners.Count == 0)
        {
            Deplete();
        }
    }

    public void OnGoldLeftChange(int newValue)
    {
        goldLeft = newValue;
        SelectionInfoKeeper.Instance.goldLeftAmountText.text = goldLeft.ToString();
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

    public IntVector2[] GetMapPositions(IntVector2 minePositionInMap)
    {
        List<IntVector2> positionsInMap = new List<IntVector2>();
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                positionsInMap.Add(new IntVector2(minePositionInMap.x + column, minePositionInMap.y + row));
            }
        }
        return positionsInMap.ToArray();
    }
    
    public int TakeGold(int amount)
    {
        if (goldLeft < amount)
        {
            amount = goldLeft;
            goldLeft = 0;
            toDestroy = true;
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
        if (MultiplayerController.Instance.localPlayer.selector.selectedUnits.Contains(visiter))
        {
            MultiplayerController.Instance.localPlayer.selector.Unselect(visiter);
        }
        visiter.RpcClearPositionInGrid();
        visiter.RpcHideYourself();
        RpcVisitMine(visiter.GetComponent<NetworkIdentity>());
        StartCoroutine(Mining(visiter));
    }

    [ClientRpc]
    void RpcVisitMine(NetworkIdentity visiterNetworkIdentity)
    {
        Worker visiter = visiterNetworkIdentity.GetComponent<Worker>();
        spriteRenderer.sprite = visitedSprite;
        if (MultiplayerController.Instance.localPlayer.selector.selectedUnits.Contains(visiter))
        {
            MultiplayerController.Instance.localPlayer.selector.Unselect(visiter);
        }
    }

    public void LeaveMine(Worker visiter)
    {
        miners.Remove(visiter);
        if (miners.Count == 0)
        {
            RpcSetNonVisitedMineSprite();
        }
        IntVector2 firstFreePlaceOnMapAroundMine = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(gameObject.transform.position), width, height);
        visiter.SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundMine);
        visiter.RpcMoveFromTo(visiter.transform.position, visiter.transform.position);
        visiter.RpcShowYourself();
        if (visiter.takenGoldAmount > 0)
        {
            visiter.RpcSetGoldVisibility(true);
            visiter.ReturnWithGold();
        }
    }

    [ClientRpc]
    void RpcSetNonVisitedMineSprite()
    {
        spriteRenderer.sprite = nonVisitedSprite;
    }

    public void Deplete()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (MultiplayerController.Instance != null)
        {
            if (MultiplayerController.Instance.localPlayer.selector.selectedMine == this)
            {
                MultiplayerController.Instance.localPlayer.selector.Unselect(this);
            }
        }
    }

    public IEnumerator Mining(Worker miner)
    {
        yield return new WaitForSeconds(timeOfMining);
        miner.takenGoldAmount = TakeGold(100);
        LeaveMine(miner);
    }
}
