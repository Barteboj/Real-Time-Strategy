using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Worker : Unit
{
    public bool isSelectingPlaceForBuilding = false;
    private bool isGoingToBuildPlace = false;
    public bool haveFinishedPlacingBuilding = false;
    public bool isGoingForGold = false;
    public bool isReturningWithGold = false;
    public bool isGoingForLumber = false;
    public bool isReturningWithLumber = false;

    public int takenGoldAmount;
    public float timeOfGivingGold;

    public int takenLumberAmount = 0;
    public float timeOfGatheringLumber;
    public float timeOfGivingLumber;

    public Building buildingToBuild;
    public Building castleToReturnWithGoods;
    public Mine mineToGoForGold;
    public LumberInGame lumberToCut;

    public BuildingType buildingToBuildType;
    public IntVector2 positionOfBuildingToBuild;
    public List<IntVector2> buildingToBuildPositionsInGrid;
    private Coroutine GatherLumberCoroutine;

    public override void Update()
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
        return buildingToBuildPositionsInGrid.Find(item => item.x == MapGridded.WorldToMapPosition(gameObject.transform.position).x && item.y == MapGridded.WorldToMapPosition(gameObject.transform.position).y) != null;
    }

    public bool CheckIfCanBuildInBuildingArea()
    {
        foreach (IntVector2 buildingPositionInGrid in buildingToBuildPositionsInGrid)
        {
            if (!MapGridded.Instance.IsInMap(buildingPositionInGrid) || (!MapGridded.Instance.mapGrid[buildingPositionInGrid.y, buildingPositionInGrid.x].isWalkable && MapGridded.Instance.mapGrid[buildingPositionInGrid.y, buildingPositionInGrid.x].unit != this))
            {
                return false;
            }
        }
        return true;
    }

    public void Build()
    {
        buildingToBuild = Instantiate(Buildings.Instance.buildingsList.Find(item => item.buildingType == buildingToBuildType && item.owner == owner), MapGridded.MapToWorldPosition(positionOfBuildingToBuild), Quaternion.identity);
        buildingToBuild.builder = this;
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).goldAmount -= buildingToBuild.goldCost;
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
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            MultiplayerController.Instance.localPlayer.selectController.SelectBuildingLocally(buildingNetworkIdentity.GetComponent<Building>());
        }
    }

    public void CancelBuild()
    {
        RpcShowBuildButtons();
        isSelectingPlaceForBuilding = false;
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
        if (MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            ShowBuildButtons();
        }
    }

    public void ShowBuildButtons()
    {
        foreach (ActionButtonType buttonType in buttonTypes)
        {
            ActionButton foundButton = ActionButtons.Instance.buttons.Find(item => item.buttonType == buttonType);
            if (foundButton.GetType() == typeof(BuildButton))
            {
                foundButton.Show();
            }
        }
    }

    public void UpdateGoingForGold()
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

    public bool CheckIfIsNextToMine()
    {
        List<MapGridElement> AdjacentGridElements = MapGridded.Instance.GetAdjacentGridElements(MapGridded.WorldToMapPosition(gameObject.transform.position));
        foreach (MapGridElement actualMapGridElement in AdjacentGridElements)
        {
            if (actualMapGridElement.mine != null && actualMapGridElement.mine == mineToGoForGold)
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
        mineToGoForGold = mine;
        IntVector2 placeToGoInToMine = MapGridded.Instance.GetStrictFirstFreePlaceAround(MapGridded.WorldToMapPosition(mine.transform.position), 2, 2);
        if (placeToGoInToMine != null)
        {
            RequestGoTo(placeToGoInToMine);
        }
        isGoingForGold = true;
    }

    public void UpdateReturningWithGold()
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

    public bool CheckIfIsNextToCastleToReturnGoods()
    {
        List<MapGridElement> AdjacentGridElements = MapGridded.Instance.GetAdjacentGridElements(MapGridded.WorldToMapPosition(gameObject.transform.position));
        foreach (MapGridElement actualMapGridElement in AdjacentGridElements)
        {
            if (actualMapGridElement.building != null && actualMapGridElement.building == castleToReturnWithGoods)
            {
                return true;
            }
        }
        return false;
    }

    [ClientRpc]
    void RpcGiveGold()
    {
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            MultiplayerController.Instance.localPlayer.selectController.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
        }
        if (isServer)
        {
            RpcClearPositionInGrid();
        }
        HideYourself();
        if (isServer)
        {
            StartCoroutine(GivingGold());
        }
    }

    public IEnumerator GivingGold()
    {
        yield return new WaitForSeconds(timeOfGivingGold);
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).goldAmount += takenGoldAmount;
        takenGoldAmount = 0;
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.width, castleToReturnWithGoods.height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        RpcShowYourself();
        GoForGold(mineToGoForGold);
    }

    public void ReturnWithGold()
    {
        gameObject.GetComponent<MinimapElement>().Show();
        castleToReturnWithGoods = FindNearestCastle();
        if (castleToReturnWithGoods != null)
        {
            List<MapGridElement> shortestPathToCastle;
            shortestPathToCastle = ASTARPathfinder.Instance.FindNearestEntrancePath(positionInGrid, MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.width, castleToReturnWithGoods.height);
            if (shortestPathToCastle != null && shortestPathToCastle.Count == 0)
            {
                RequestGoTo(positionInGrid);
            }
            else if (shortestPathToCastle != null)
            {
                RequestGoTo(new IntVector2(shortestPathToCastle[shortestPathToCastle.Count - 1].x, shortestPathToCastle[shortestPathToCastle.Count - 1].y));
            }
            StartReturningWithGold();
        }
    }

    public Building FindNearestCastle()
    {
        if (MultiplayerController.Instance.localPlayer.buildings.Find(item => item.buildingType == BuildingType.Castle))
        {
            return MultiplayerController.Instance.players.Find(item => item.playerType == owner).buildings.Find(item => item.buildingType == BuildingType.Castle);
        }
        else
        {
            return null;
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
            if (lumberToCut.IsDepleted || lumberToCut.isBeingCut)
            {
                GoForNewLumber();
            }
            else
            {
                TakeLumber();
            }
        }
        else if (!isFollowingPath)
        {
            GoForLumber(lumberToCut);
        }
    }

    public bool CheckIfIsNextToLumber()
    {
        List<MapGridElement> AdjacentGridElements = MapGridded.Instance.GetAdjacentGridElements(MapGridded.WorldToMapPosition(gameObject.transform.position));
        foreach (MapGridElement actualMapGridElement in AdjacentGridElements)
        {
            if (actualMapGridElement.lumber != null && actualMapGridElement.lumber == lumberToCut)
            {
                return true;
            }
        }
        return false;
    }

    public void GoForNewLumber()
    {
        List<MapGridElement> mapGridElementsInWorkersSight = MapGridded.Instance.GetGridElementsFromArea(positionInGrid, sight, sight);
        if (mapGridElementsInWorkersSight.Count > 0)
        {
            foreach (MapGridElement mapGridElementInWorkerSight in mapGridElementsInWorkersSight)
            {
                if (mapGridElementInWorkerSight.lumber != null && !mapGridElementInWorkerSight.lumber.IsDepleted)
                {
                    GoForLumber(mapGridElementInWorkerSight.lumber);
                }
            }
        }
    }

    public void GoForLumber(LumberInGame lumber)
    {
        lumberToCut = lumber;
        IntVector2 placeToGoToChopTree = MapGridded.Instance.GetStrictFirstFreePlaceAround(MapGridded.WorldToMapPosition(lumber.transform.position), 1, 1);
        if (placeToGoToChopTree != null)
        {
            RequestGoTo(placeToGoToChopTree);
        }
        isGoingForLumber = true;
    }

    public void TakeLumber()
    {
        GatherLumberCoroutine = StartCoroutine(GatherLumber());
    }

    private IEnumerator GatherLumber()
    {
        lumberToCut.isBeingCut = true;
        yield return new WaitForSeconds(timeOfGatheringLumber);
        takenLumberAmount = 50;
        lumberToCut.Deplete();
        ReturnWithLumber();
    }

    public void ReturnWithLumber()
    {
        castleToReturnWithGoods = FindNearestCastle();
        if (castleToReturnWithGoods != null)
        {
            List<MapGridElement> shortestPathToCastle;
            shortestPathToCastle = ASTARPathfinder.Instance.FindNearestEntrancePath(positionInGrid, MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.width, castleToReturnWithGoods.height);
            if (shortestPathToCastle != null && shortestPathToCastle.Count == 0)
            {
                RequestGoTo(positionInGrid);
            }
            else if (shortestPathToCastle != null)
            {
                RequestGoTo(new IntVector2(shortestPathToCastle[shortestPathToCastle.Count - 1].x, shortestPathToCastle[shortestPathToCastle.Count - 1].y));
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
        if (CheckIfIsNextToCastleToReturnGoods() && hasFinishedGoingToLastStep)
        {
            isFollowingPath = false;
            isReturningWithLumber = false;
            RpcGiveLumber();
        }
        else if (!isFollowingPath)
        {
            ReturnWithLumber();
        }
    }

    [ClientRpc]
    void RpcGiveLumber()
    {
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            MultiplayerController.Instance.localPlayer.selectController.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
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
        yield return new WaitForSeconds(timeOfGivingLumber);
        MultiplayerController.Instance.localPlayer.lumberAmount += takenLumberAmount;
        MultiplayerController.Instance.localPlayer.UpdateResourcesGUI();
        takenLumberAmount = 0;
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.width, castleToReturnWithGoods.height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        RpcShowYourself();
        GoForLumber(lumberToCut);
    }

    public void PrepareBuild(Building buildingToBuildPrefab)
    {
        HideBuildButtons();
        haveFinishedPlacingBuilding = false;
        isSelectingPlaceForBuilding = true;
        this.buildingToBuild = Instantiate(buildingToBuildPrefab);
        this.buildingToBuild.builder = this;
    }

    public void HideBuildButtons()
    {
        foreach (ActionButtonType buttonType in buttonTypes)
        {
            ActionButton foundButton = ActionButtons.Instance.buttons.Find(item => item.buttonType == buttonType);
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
        isSelectingPlaceForBuilding = false;
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
        Building buildingtoBuild = Buildings.Instance.buildingsList.Find(item => item.buildingType == buildingType && item.owner == owner);
        IntVector2 positionOfBuildingToBuild = MapGridded.WorldToMapPosition(buildPlaceInWorldSpace);
        buildingToBuildPositionsInGrid = new List<IntVector2>();
        for (int row = 0; row < buildingtoBuild.height; ++row)
        {
            for (int column = 0; column < buildingtoBuild.width; ++column)
            {
                buildingToBuildPositionsInGrid.Add(new IntVector2(positionOfBuildingToBuild.x + column, positionOfBuildingToBuild.y + row));
            }
        }
    }

    public void FinishBuild()
    {
        RpcShowYourself();
        RpcSetNewPosition(MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), buildingToBuild.width, buildingToBuild.height)));
        positionInGridSyncVar = MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), buildingToBuild.width, buildingToBuild.height));
        RpcStop();
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

        if (GatherLumberCoroutine != null)
        {
            StopCoroutine(GatherLumberCoroutine);
        }
    }
}