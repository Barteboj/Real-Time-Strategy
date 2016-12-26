using UnityEngine;
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

    private Coroutine GatherLumberCoroutine;

    public bool HasGold
    {
        get
        {
            return takenGoldAmount > 0;
        }
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

    public void PrepareBuild(Building buildingToBuildPrefab)
    {
        HideBuildButtons();
        haveFinishedPlacingBuilding = false;
        isSelectingPlaceForBuilding = true;
        this.buildingToBuild = Instantiate(buildingToBuildPrefab);
        this.buildingToBuild.builder = this;
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

    public void GoToBuildPlace()
    {
        isSelectingPlaceForBuilding = false;
        buildingToBuild.gameObject.SetActive(false);
        RequestGoTo(MapGridded.WorldToMapPosition(buildingToBuild.transform.position));
        isGoingToBuildPlace = true;
    }

    public void Build()
    {
        Players.Instance.LocalPlayer.GoldAmount -= buildingToBuild.goldCost;
        buildingToBuild.gameObject.SetActive(true);
        buildingToBuild.PlaceOnMap();
        isGoingToBuildPlace = false;
        haveFinishedPlacingBuilding = true;
        gameObject.SetActive(false);
        buildingToBuild.StartBuildProcess();
        if (SelectController.Instance.selectedUnit == this)
        {
            SelectController.Instance.SelectBuilding(buildingToBuild);
        }
    }

    public void FinishBuild()
    {
        gameObject.transform.position = MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), buildingToBuild.width, buildingToBuild.height));
        InitializePositionInGrid();
        isGoingToBuildPlace = false;
        gameObject.SetActive(true);
    }

    public void CancelBuild()
    {
        ShowBuildButtons();
        isSelectingPlaceForBuilding = false;
        isGoingToBuildPlace = false;
        if (!haveFinishedPlacingBuilding && buildingToBuild != null)
        {
            buildingToBuild.HideBuildGrid();
            Destroy(buildingToBuild.gameObject);
        }
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

    public void TakeGold()
    {
        gameObject.GetComponent<MinimapElement>().Hide();
        mineToGoForGold.VisitMine(this);
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

    public void GiveGold()
    {
        if (SelectController.Instance.selectedUnit == this)
        {
            SelectController.Instance.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
        }
        ClearPositionInGrid();
        spriteRenderer.enabled = false;
        selectionCollider.SetActive(false);
        gameObject.GetComponent<MinimapElement>().Hide();
        StartCoroutine(GivingGold());
    }

    public void GiveLumber()
    {
        if (SelectController.Instance.selectedUnit == this)
        {
            SelectController.Instance.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
        }
        ClearPositionInGrid();
        spriteRenderer.enabled = false;
        selectionCollider.SetActive(false);
        gameObject.GetComponent<MinimapElement>().Hide();
        StartCoroutine(GivingLumber());
    }

    public IEnumerator GivingGold()
    {
        yield return new WaitForSeconds(timeOfGivingGold);
        Players.Instance.LocalPlayer.GoldAmount += takenGoldAmount;
        takenGoldAmount = 0;
        LeaveCastle();
    }

    public IEnumerator GivingLumber()
    {
        yield return new WaitForSeconds(timeOfGivingLumber);
        Players.Instance.LocalPlayer.LumberAmount += takenLumberAmount;
        takenLumberAmount = 0;
        LeaveCastleForLumber();
    }

    public void LeaveCastle()
    {
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.width, castleToReturnWithGoods.height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        spriteRenderer.enabled = true;
        selectionCollider.SetActive(true);
        gameObject.GetComponent<MinimapElement>().Show();
        GoForGold(mineToGoForGold);
    }

    public void LeaveCastleForLumber()
    {
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGoods.transform.position), castleToReturnWithGoods.width, castleToReturnWithGoods.height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        spriteRenderer.enabled = true;
        selectionCollider.SetActive(true);
        gameObject.GetComponent<MinimapElement>().Show();
        GoForLumber(lumberToCut);
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

    public Building FindNearestCastle()
    {
        if (Players.Instance.LocalPlayer.HasCastle)
        {
            return Players.Instance.LocalPlayer.castles[0];
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

    public void StartReturningWithLumber()
    {
        isReturningWithLumber = true;
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

    public override void Update()
    {
        base.Update();
        if (isSelectingPlaceForBuilding)
        {
            Vector2 griddedPosition = new Vector2(SelectController.Instance.GetGridPositionFromMousePosition().x, SelectController.Instance.GetGridPositionFromMousePosition().y);
            buildingToBuild.transform.position = SelectController.Instance.GetGriddedWorldPositionFromMousePosition();
            buildingToBuild.ShowBuildGrid();
            if (Input.GetMouseButtonUp(0))
            {
                if (buildingToBuild.CouldBeBuildInPlace(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), this))
                {
                    buildingToBuild.HideBuildGrid();
                    GoToBuildPlace();
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                CancelBuild();
            }
        }
        if (isGoingToBuildPlace)
        {
            if (buildingToBuild.CheckIfIsInBuildingArea(MapGridded.WorldToMapPosition(gameObject.transform.position)))
            {
                if (buildingToBuild.CouldBeBuildInPlace(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), this) && hasFinishedGoingToLastStep)
                {
                    isFollowingPath = false;
                    Build();
                }
                else if (hasFinishedGoingToLastStep)
                {
                    CancelBuild();
                }
            }
        }
        if (isGoingForGold && CheckIfIsNextToMine() && hasFinishedGoingToLastStep)
        {
            isFollowingPath = false;
            isGoingForGold = false;
            TakeGold();
        }
        else if (isGoingForGold && !isFollowingPath)
        {
            GoForGold(mineToGoForGold);
        }
        if (isReturningWithGold && CheckIfIsNextToCastleToReturnGoods() && hasFinishedGoingToLastStep)
        {
            isFollowingPath = false;
            isReturningWithGold = false;
            GiveGold();
        }
        else if (isReturningWithGold && !isFollowingPath)
        {
            ReturnWithGold();
        }
        if (isGoingForLumber && CheckIfIsNextToLumber() && hasFinishedGoingToLastStep)
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
        else if (isGoingForLumber && !isFollowingPath)
        {
            GoForLumber(lumberToCut);
        }
        if (isReturningWithLumber && CheckIfIsNextToCastleToReturnGoods() && hasFinishedGoingToLastStep)
        {
            isFollowingPath = false;
            isReturningWithLumber = false;
            GiveLumber();
        }
        else if (isReturningWithLumber && !isFollowingPath)
        {
            ReturnWithLumber();
        }
    }
}
