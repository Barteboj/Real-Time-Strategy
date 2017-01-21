using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Worker : Unit
{
    private bool isCuttingLumber = false;
    public bool IsSelectingPlaceForBuilding { get; set; }
    private bool isGoingToBuildPlace = false;
    private bool haveFinishedPlacingBuilding = false;
    private bool isGoingForGold = false;
    private bool isReturningWithGold = false;
    private bool isGoingForLumber = false;
    private bool isReturningWithLumber = false;

    public int TakenGoldAmount { get; set; }
    private float timeOfGivingGold;

    public int TakenLumberAmount { get; set; }
    [SerializeField]
    private float timeOfGatheringLumber;
    [SerializeField]
    private float timeOfGivingLumber;

    private Building buildingToBuild;
    private Building castleToReturnWithGoods;
    private Mine mineToGoForGold;
    private LumberInGame lumberToCut;

    private BuildingType buildingToBuildType;
    private IntVector2 positionOfBuildingToBuild;
    private List<IntVector2> buildingToBuildPositionsInGrid;
    private Coroutine GatherLumberCoroutine;
    [SerializeField]
    private SpriteRenderer lumber;
    [SerializeField]
    private SpriteRenderer gold;

    protected override void Update()
    {
        base.Update();
        if (isServer)
        {
            if (isGoingToBuildPlace)
            {
                UpdateGoingToBuildPlace();
            }
            if (isGoingForGold)
            {
                UpdateGoingForGold();
            }
            if (isReturningWithGold)
            {
                UpdateReturningWithGold();
            }
            if (isGoingForLumber)
            {
                UpdateGoingForLumber();
            }
            if (isReturningWithLumber)
            {
                UpdateReturningWithLumber();
            }
        }
    }

    public void UpdateGoingToBuildPlace()
    {
        if (CheckIfIsInBuildingArea())
        {
            if (CheckIfCanBuildInBuildingArea() && (hasFinishedGoingToLastStep || !isFollowingPath))
            {
                isFollowingPath = false;
                isMoving = false;
                Build();
            }
            else if (hasFinishedGoingToLastStep)
            {
                CancelBuild();
            }
        }
    }

    public bool CheckIfIsInBuildingArea()
    {
        return buildingToBuildPositionsInGrid.Find(item => item.X == MapGridded.WorldToMapPosition(gameObject.transform.position).X && item.Y == MapGridded.WorldToMapPosition(gameObject.transform.position).Y) != null;
    }

    public bool CheckIfCanBuildInBuildingArea()
    {
        foreach (IntVector2 buildingPositionInGrid in buildingToBuildPositionsInGrid)
        {
            if (!MapGridded.Instance.IsInMap(buildingPositionInGrid) || (!MapGridded.Instance.MapGrid[buildingPositionInGrid.Y, buildingPositionInGrid.X].IsWalkable && MapGridded.Instance.MapGrid[buildingPositionInGrid.Y, buildingPositionInGrid.X].Unit != this))
            {
                return false;
            }
        }
        return true;
    }

    public void Build()
    {
        buildingToBuild = Instantiate(Buildings.Instance.BuildingsList.Find(item => item.BuildingType == buildingToBuildType && item.Owner == owner), MapGridded.MapToWorldPosition(positionOfBuildingToBuild), Quaternion.identity);
        buildingToBuild.Builder = this;
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).GoldAmount -= buildingToBuild.GoldCost;
        NetworkServer.Spawn(buildingToBuild.gameObject);
        buildingToBuild.PlaceOnMap();
        isGoingToBuildPlace = false;
        haveFinishedPlacingBuilding = true;
        RpcHideYourself();
        buildingToBuild.StartBuildProcess();
        RpcSelectBuildedBuilding(buildingToBuild.GetComponent<NetworkIdentity>());
    }

    [ClientRpc]
    void RpcSelectBuildedBuilding(NetworkIdentity buildingNetworkIdentity)
    {
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
        {
            MultiplayerController.Instance.LocalPlayer.Selector.UnSelectAll();
            MultiplayerController.Instance.LocalPlayer.Selector.Select(buildingNetworkIdentity.GetComponent<Building>());
        }
    }

    public void CancelBuild()
    {
        RpcShowBuildButtons();
        IsSelectingPlaceForBuilding = false;
        isGoingToBuildPlace = false;
        if (!haveFinishedPlacingBuilding && buildingToBuild != null)
        {
            buildingToBuild.HideBuildGrid();
            Destroy(buildingToBuild.gameObject);
        }
    }

    [ClientRpc]
    void RpcShowBuildButtons()
    {
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == owner && MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Count == 1 && MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
        {
            ShowBuildButtons();
        }
    }

    public void ShowBuildButtons()
    {
        foreach (ActionButtonType buttonType in buttonTypes)
        {
            ActionButton foundButton = ActionButtons.Instance.buttons.Find(item => item.ButtonType == buttonType);
            if (foundButton.GetType() == typeof(BuildButton))
            {
                foundButton.Show();
            }
        }
    }

    public void UpdateGoingForGold()
    {
        if (mineToGoForGold == null)
        {
            CancelGatheringGold();
        }
        else
        {
            if (CheckIfIsNextToMine() && hasFinishedGoingToLastStep)
            {
                isFollowingPath = false;
                isGoingForGold = false;
                TakeGold();
            }
            else if (!isFollowingPath)
            {
                GoForGold(mineToGoForGold);
            }
        }
    }

    public bool CheckIfIsNextToMine()
    {
        List<MapGridElement> AdjacentGridElements = MapGridded.Instance.GetAdjacentGridElements(MapGridded.WorldToMapPosition(gameObject.transform.position));
        foreach (MapGridElement actualMapGridElement in AdjacentGridElements)
        {
            if (actualMapGridElement.Mine != null && actualMapGridElement.Mine == mineToGoForGold)
            {
                return true;
            }
        }
        return false;
    }

    public void TakeGold()
    {
        gameObject.GetComponent<MinimapElement>().Hide();
        mineToGoForGold.VisitMine(this);
    }

    public void GoForGold(Mine mine)
    {
        if (mine != null)
        {
            mineToGoForGold = mine;
            List<MapGridElement> pathToMine = ASTARPathfinder.Instance.FindPathForMine(positionInGrid, mine);
            if (pathToMine != null)
            {
                RequestGoTo(pathToMine);
            }
            isGoingForGold = true;
        }
        else
        {
            isGoingForGold = false;
        }
    }

    public void UpdateReturningWithGold()
    {
        if (castleToReturnWithGoods == null)
        {
            CancelGatheringGold();
        }
        else
        {
            if (CheckIfIsNextToCastleToReturnGoods() && hasFinishedGoingToLastStep)
            {
                isFollowingPath = false;
                isReturningWithGold = false;
                RpcGiveGold();
            }
            else if (!isFollowingPath)
            {
                ReturnWithGold();
            }
        }
    }

    public bool CheckIfIsNextToCastleToReturnGoods()
    {
        List<MapGridElement> AdjacentGridElements = MapGridded.Instance.GetAdjacentGridElements(MapGridded.WorldToMapPosition(gameObject.transform.position));
        foreach (MapGridElement actualMapGridElement in AdjacentGridElements)
        {
            if (actualMapGridElement.Building != null && actualMapGridElement.Building == castleToReturnWithGoods)
            {
                return true;
            }
        }
        return false;
    }

    [ClientRpc]
    void RpcGiveGold()
    {
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
        {
            MultiplayerController.Instance.LocalPlayer.Selector.Unselect(this);
        }
        if (isServer)
        {
            RpcClearPositionInGrid();
            RpcSetGoldVisibility(false);
        }
        spriteRenderer.enabled = false;
        selectionCollider.SetActive(false);
        gameObject.GetComponent<MinimapElement>().Hide();
        if (isServer)
        {
            StartCoroutine(GivingGold());
        }
    }

    public IEnumerator GivingGold()
    {
        castleToReturnWithGoods.Visiters.Add(this);
        yield return new WaitForSeconds(timeOfGivingGold);
        castleToReturnWithGoods.Visiters.Remove(this);
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).GoldAmount += TakenGoldAmount;
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).AllGatheredGold += TakenGoldAmount;
        TakenGoldAmount = 0;
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.Width, castleToReturnWithGoods.Height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        RpcMoveFromTo(gameObject.transform.position, gameObject.transform.position);
        RpcShowYourself();
        FillPositionInGrid();
        GoForGold(mineToGoForGold);
    }

    public void ReturnWithGold()
    {
        gameObject.GetComponent<MinimapElement>().Show();
        List<MapGridElement> shortestPathToCastle = null;
        if (MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).ActiveBuildings.Find(item => item.BuildingType == BuildingType.Castle))
        {
            shortestPathToCastle = ASTARPathfinder.Instance.FindPathForNearestCastle(positionInGrid, owner, out castleToReturnWithGoods);
        }
        if (castleToReturnWithGoods != null)
        {
            if (shortestPathToCastle != null && shortestPathToCastle.Count == 0)
            {
                RequestGoTo(shortestPathToCastle);
            }
            else if (shortestPathToCastle != null)
            {
                RequestGoTo(shortestPathToCastle);
            }
            StartReturningWithGold();
        }
    }

    public void StartReturningWithGold()
    {
        isReturningWithGold = true;
    }

    public void UpdateGoingForLumber()
    {
        if (CheckIfIsNextToLumber() && hasFinishedGoingToLastStep)
        {
            isFollowingPath = false;
            isGoingForLumber = false;
            if (lumberToCut.IsDepleted || lumberToCut.IsBeingCut)
            {
                GoForLumber();
            }
            else
            {
                TakeLumber();
            }
        }
        else if (!isFollowingPath)
        {
            GoForLumber();
        }
    }

    public bool CheckIfIsNextToLumber()
    {
        List<MapGridElement> AdjacentGridElements = MapGridded.Instance.GetAdjacentGridElements(MapGridded.WorldToMapPosition(gameObject.transform.position));
        foreach (MapGridElement actualMapGridElement in AdjacentGridElements)
        {
            if (actualMapGridElement.Lumber != null && actualMapGridElement.Lumber == lumberToCut)
            {
                return true;
            }
        }
        return false;
    }

    public void GoForLumber()
    {
        LumberInGame lumberToCut;
        List<MapGridElement> pathForLumber = ASTARPathfinder.Instance.FindPathForLumber(positionInGrid, out lumberToCut);
        if (pathForLumber != null)
        {
            RequestGoTo(pathForLumber);
            this.lumberToCut = lumberToCut;
            isGoingForLumber = true;
        }
        else
        {
            isGoingForLumber = false;
        }
    }

    public void TakeLumber()
    {
        GatherLumberCoroutine = StartCoroutine(GatherLumber());
    }

    private IEnumerator GatherLumber()
    {
        lumberToCut.IsBeingCut = true;
        isCuttingLumber = true;
        yield return new WaitForSeconds(timeOfGatheringLumber);
        isCuttingLumber = false;
        TakenLumberAmount = 50;
        lumberToCut.RpcDeplete();
        RpcSetLumberVisibility(true);
        ReturnWithLumber();
    }

    public void ReturnWithLumber()
    {
        List<MapGridElement> shortestPathToCastle = null;
        if (MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).ActiveBuildings.Find(item => item.BuildingType == BuildingType.Castle))
        {
            shortestPathToCastle = ASTARPathfinder.Instance.FindPathForNearestCastle(positionInGrid, owner, out castleToReturnWithGoods);
        }
        if (castleToReturnWithGoods != null)
        {
            if (shortestPathToCastle != null && shortestPathToCastle.Count == 0)
            {
                RequestGoTo(shortestPathToCastle);
            }
            else if (shortestPathToCastle != null)
            {
                RequestGoTo(shortestPathToCastle);
            }
            StartReturningWithLumber();
        }

    }

    public void StartReturningWithLumber()
    {
        isReturningWithLumber = true;
    }

    public void UpdateReturningWithLumber()
    {
        if (castleToReturnWithGoods == null)
        {
            CancelGatheringLumber();
        }
        else
        {
            if (CheckIfIsNextToCastleToReturnGoods() && hasFinishedGoingToLastStep)
            {
                isFollowingPath = false;
                isReturningWithLumber = false;
                RpcSetLumberVisibility(false);
                RpcGiveLumber();
            }
            else if (!isFollowingPath)
            {
                ReturnWithLumber();
            }
        }
    }

    [ClientRpc]
    void RpcGiveLumber()
    {
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
        {
            MultiplayerController.Instance.LocalPlayer.Selector.Unselect(this);
        }
        if (isServer)
        {
            RpcClearPositionInGrid();
        }
        spriteRenderer.enabled = false;
        selectionCollider.SetActive(false);
        gameObject.GetComponent<MinimapElement>().Hide();
        if (isServer)
        {
            StartCoroutine(GivingLumber());
        }
    }

    public IEnumerator GivingLumber()
    {
        castleToReturnWithGoods.Visiters.Add(this);
        yield return new WaitForSeconds(timeOfGivingLumber);
        castleToReturnWithGoods.Visiters.Remove(this);
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).LumberAmount += TakenLumberAmount;
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).AllGatheredLumber += TakenLumberAmount;
        TakenLumberAmount = 0;
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.Width, castleToReturnWithGoods.Height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        RpcMoveFromTo(gameObject.transform.position, gameObject.transform.position);
        RpcShowYourself();
        GoForLumber();
    }

    public void PrepareBuild(Building buildingToBuildPrefab)
    {
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Count == 1 && MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
        {
            HideBuildButtons();
        }
        haveFinishedPlacingBuilding = false;
        IsSelectingPlaceForBuilding = true;
        this.buildingToBuild = Instantiate(buildingToBuildPrefab);
        this.buildingToBuild.Builder = this;
    }

    public void HideBuildButtons()
    {
        foreach (ActionButtonType buttonType in buttonTypes)
        {
            ActionButton foundButton = ActionButtons.Instance.buttons.Find(item => item.ButtonType == buttonType);
            if (foundButton.GetType() == typeof(BuildButton))
            {
                foundButton.Hide();
            }
        }
    }

    public void GoToBuildPlace(BuildingType buildingType, Vector2 buildPlaceInWorldSpace)
    {
        buildingToBuildType = buildingType;
        positionOfBuildingToBuild = MapGridded.WorldToMapPosition(buildPlaceInWorldSpace);
        SetBuildingToBuildPositionsInGrid(buildingType, buildPlaceInWorldSpace);
        IsSelectingPlaceForBuilding = false;
        RequestGoTo(positionOfBuildingToBuild);
        isGoingToBuildPlace = true;
        if (CheckIfIsInBuildingArea())
        {
            if (CheckIfCanBuildInBuildingArea())
            {
                isFollowingPath = false;
                isMoving = false;
                Build();
            }
        }
    }

    public void SetBuildingToBuildPositionsInGrid(BuildingType buildingType, Vector2 buildPlaceInWorldSpace)
    {
        Building buildingtoBuild = Buildings.Instance.BuildingsList.Find(item => item.BuildingType == buildingType && item.Owner == owner);
        IntVector2 positionOfBuildingToBuild = MapGridded.WorldToMapPosition(buildPlaceInWorldSpace);
        buildingToBuildPositionsInGrid = new List<IntVector2>();
        for (int row = 0; row < buildingtoBuild.Height; ++row)
        {
            for (int column = 0; column < buildingtoBuild.Width; ++column)
            {
                buildingToBuildPositionsInGrid.Add(new IntVector2(positionOfBuildingToBuild.X + column, positionOfBuildingToBuild.Y + row));
            }
        }
    }

    public void FinishBuild()
    {
        RpcShowYourself();
        RpcSetNewPosition(MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), buildingToBuild.Width, buildingToBuild.Height)));
        positionInGridSyncVar = MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), buildingToBuild.Width, buildingToBuild.Height));
        RpcStop();
    }

    public override void ShowYourself()
    {
        base.ShowYourself();
        gold.gameObject.SetActive(true);
        lumber.gameObject.SetActive(true);
    }

    public override void HideYourself()
    {
        base.HideYourself();
        gold.gameObject.SetActive(false);
        lumber.gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcStop()
    {
        isGoingToBuildPlace = false;
        isMoving = false;
        isFollowingPath = false;
    }

    public void CancelGatheringGold()
    {
        isGoingForGold = false;
        isReturningWithGold = false;
    }

    public void CancelGatheringLumber()
    {
        isGoingForLumber = false;
        isReturningWithLumber = false;
        if (isCuttingLumber)
        {
            lumberToCut.IsBeingCut = false;
        }
        if (GatherLumberCoroutine != null)
        {
            StopCoroutine(GatherLumberCoroutine);
        }
    }

    [ClientRpc]
    public void RpcSetLumberVisibility(bool shouldBeVisible)
    {
        lumber.enabled = shouldBeVisible;
    }

    [ClientRpc]
    public void RpcSetGoldVisibility(bool shouldBeVisible)
    {
        gold.enabled = shouldBeVisible;
    }
}