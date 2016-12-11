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

    public int takenGoldAmount;
    public float timeOfGivingGold;

    public Building buildingToBuild;
    public Building castleToReturnWithGold;
    public Mine mineToGoForGold;

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
        List<MapGridElement> shortestPathToMine;
        shortestPathToMine = ASTARPathfinder.Instance.FindNearestEntrancePath(positionInGrid, MapGridded.WorldToMapPosition(mineToGoForGold.transform.position), mineToGoForGold.width, mineToGoForGold.height);
        if (shortestPathToMine != null && shortestPathToMine.Count > 0)
        {
            RequestGoTo(new IntVector2(shortestPathToMine[shortestPathToMine.Count - 1].x, shortestPathToMine[shortestPathToMine.Count - 1].y));
        }
        isGoingForGold = true;
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

    public void TakeGold()
    {
        mineToGoForGold.VisitMine(this);
    }

    public void GiveGold()
    {
        if (SelectController.Instance.selectedUnit == this)
        {
            SelectController.Instance.Unselect();
            Unselect();
        }
        ClearPositionInGrid();
        spriteRenderer.enabled = false;
        selectionCollider.SetActive(false);
        StartCoroutine(GivingGold());
    }

    public IEnumerator GivingGold()
    {
        yield return new WaitForSeconds(timeOfGivingGold);
        Players.Instance.LocalPlayer.GoldAmount += takenGoldAmount;
        takenGoldAmount = 0;
        LeaveCastle();
    }

    public void LeaveCastle()
    {
        IntVector2 firstFreePlaceOnMapAroundCastle = MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(castleToReturnWithGold.transform.position), castleToReturnWithGold.width, castleToReturnWithGold.height);
        SetNewPositionOnMapSettingWorldPosition(firstFreePlaceOnMapAroundCastle);
        spriteRenderer.enabled = true;
        selectionCollider.SetActive(true);
        GoForGold(mineToGoForGold);
    }

    public void ReturnWithGold()
    {
        castleToReturnWithGold = FindNearestCastle();
        if (castleToReturnWithGold != null)
        {
            List<MapGridElement> shortestPathToCastle;
            shortestPathToCastle = ASTARPathfinder.Instance.FindNearestEntrancePath(positionInGrid, MapGridded.WorldToMapPosition(castleToReturnWithGold.transform.position), castleToReturnWithGold.width, castleToReturnWithGold.height);
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
            if (!isFollowingPath && (followedPath == null || (followedPath[followedPath.Count - 1].x == MapGridded.WorldToMapPosition(gameObject.transform.position).x && followedPath[followedPath.Count - 1].y == MapGridded.WorldToMapPosition(gameObject.transform.position).y)))
            {
                if (buildingToBuild.CouldBeBuildInPlace(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), this))
                {
                    Build();
                }
            }
        }
        if (isGoingForGold && !isFollowingPath && followedPath != null && positionInGrid.x == followedPath[followedPath.Count - 1].x && positionInGrid.y == followedPath[followedPath.Count - 1].y)
        {
            isGoingForGold = false;
            TakeGold();
        }
        else if (isGoingForGold && !isFollowingPath)
        {
            GoForGold(mineToGoForGold);
        }
        if (isReturningWithGold && !isFollowingPath && followedPath != null && positionInGrid.x == followedPath[followedPath.Count - 1].x && positionInGrid.y == followedPath[followedPath.Count - 1].y)
        {
            isReturningWithGold = false;
            GiveGold();
        }
        else if (isReturningWithGold && !isFollowingPath)
        {
            ReturnWithGold();
        }
    }
}
