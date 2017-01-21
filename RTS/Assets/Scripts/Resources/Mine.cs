using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Mine : NetworkBehaviour
{
    private bool toDestroy = false;

    [SerializeField]
    private Sprite nonVisitedSprite;
    [SerializeField]
    private Sprite visitedSprite;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private float timeOfMining;

    private List<Worker> miners = new List<Worker>();
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
            SelectionInfoKeeper.Instance.GoldLeftAmountText.text = goldLeft.ToString();
        }
    }

    [SerializeField]
    private int width;
    [SerializeField]
    private int height;

    private IntVector2 placeOnMapGrid;

    [SerializeField]
    private GameObject selectionIndicator;
    public GameObject SelectionIndicator
    {
        get
        {
            return selectionIndicator;
        }
    }

    public const string mineName = "Mine";

    [SerializeField]
    private Sprite portrait;
    public Sprite Portrait
    {
        get
        {
            return portrait;
        }
    }

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
        SelectionInfoKeeper.Instance.GoldLeftAmountText.text = goldLeft.ToString();
    }

    public void FillPositionInGrid()
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].Mine = this;
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
                positionsInMap.Add(new IntVector2(minePositionInMap.X + column, minePositionInMap.Y + row));
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
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(visiter))
        {
            MultiplayerController.Instance.LocalPlayer.Selector.Unselect(visiter);
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
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(visiter))
        {
            MultiplayerController.Instance.LocalPlayer.Selector.Unselect(visiter);
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
        if (visiter.TakenGoldAmount > 0)
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
            if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedMine == this)
            {
                MultiplayerController.Instance.LocalPlayer.Selector.Unselect(this);
            }
        }
    }

    public IEnumerator Mining(Worker miner)
    {
        yield return new WaitForSeconds(timeOfMining);
        miner.TakenGoldAmount = TakeGold(100);
        LeaveMine(miner);
    }
}
