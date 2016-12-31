using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectController : NetworkBehaviour
{
    public Unit selectedUnit;
    public Building selectedBuilding;
    public Mine selectedMine;

    public bool isSelectingBuildingPlace = false;
    public Building buildingToBuild;

    [Command]
    void CmdRightClickCommand(Vector2 mousePositionInWorld, PlayerType commander)
    {
        IntVector2 mousePositionOnMap = MapGridded.WorldToMapPosition(mousePositionInWorld);
        if (selectedUnit != null && MapGridded.Instance.IsInMap(mousePositionOnMap) && selectedUnit.owner == commander)
        {
            if (selectedUnit.GetType() == typeof(Worker))
            {
                if (!((Worker)selectedUnit).isSelectingPlaceForBuilding)
                {
                    ((Worker)selectedUnit).CancelBuild();
                    ((Worker)selectedUnit).CancelGatheringGold();
                    ((Worker)selectedUnit).CancelGatheringLumber();
                    RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray((Vector3)mousePositionInWorld - Vector3.forward, Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
                    if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Mine>())
                    {
                        ((Worker)selectedUnit).GoForGold(hitInfo.collider.transform.parent.GetComponent<Mine>());
                    }
                    else if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<LumberInGame>() && !hitInfo.collider.transform.parent.GetComponent<LumberInGame>().IsDepleted)
                    {
                        ((Worker)selectedUnit).GoForLumber(hitInfo.collider.transform.parent.GetComponent<LumberInGame>());
                    }
                    else
                    {
                        selectedUnit.RequestGoTo(mousePositionOnMap);
                    }
                }
                else
                {
                    ((Worker)selectedUnit).CancelBuild();
                }
            }
            else if (selectedUnit.GetType() == typeof(Warrior))
            {
                RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray((Vector3)mousePositionInWorld - Vector3.forward, Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
                if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Unit>())
                {
                    ((Warrior)selectedUnit).StartAttack(hitInfo.collider.transform.parent.GetComponent<Unit>());
                }
                else
                {
                    if (((Warrior)selectedUnit).isAttacking)
                    {
                        ((Warrior)selectedUnit).StopAttack();
                    }
                    selectedUnit.RequestGoTo(mousePositionOnMap);
                }
            }
            else
            {
                selectedUnit.RequestGoTo(mousePositionOnMap);
            }
        }
    }

    [Command]
    void CmdLeftClickCommand(Vector2 mousePositionInWorld, PlayerType commander)
    {
        if (selectedUnit != null && selectedUnit.GetType() == typeof(Worker))
        {
            if (!((Worker)selectedUnit).isSelectingPlaceForBuilding)
            {
                SelectItem(mousePositionInWorld);
            }
        }
        else
        {
            SelectItem(mousePositionInWorld);
        }
    }

    [Command]
    void CmdOrderUnitToBuild(BuildingType buildingType, Vector2 buildPlaceInWorldSpace)
    {
        ((Worker)selectedUnit).GoToBuildPlace(buildingType, buildPlaceInWorldSpace);
    }

    void Update()
    {
        if (!hasAuthority || SceneManager.GetActiveScene().name != "Game" || MapGridded.Instance.mapGrid == null)
        {
            return;
        }
        if (isSelectingBuildingPlace)
        {
            Vector2 griddedPosition = new Vector2(GetGridPositionFromMousePosition().x, GetGridPositionFromMousePosition().y);
            buildingToBuild.transform.position = GetGriddedWorldPositionFromMousePosition();
            buildingToBuild.ShowBuildGrid();
            if (Input.GetMouseButtonUp(0))
            {
                if (buildingToBuild.CouldBeBuildInPlace(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), selectedUnit))
                {
                    isSelectingBuildingPlace = false;
                    buildingToBuild.HideBuildGrid();
                    CmdOrderUnitToBuild(buildingToBuild.buildingType, buildingToBuild.transform.position);
                    Destroy(buildingToBuild);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isSelectingBuildingPlace = false;
                Destroy(buildingToBuild);
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(1) && selectedUnit != null && MapGridded.Instance.IsInMap(GetGridPositionFromMousePosition()) && selectedUnit.owner == MultiplayerController.Instance.localPlayer.playerType)
            {
                CmdRightClickCommand(Camera.main.ScreenToWorldPoint(Input.mousePosition), MultiplayerController.Instance.localPlayer.playerType);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                CmdLeftClickCommand(Camera.main.ScreenToWorldPoint(Input.mousePosition), MultiplayerController.Instance.localPlayer.playerType);
            }
            RaycastHit2D underMouseCursorInfo = GetWhatIsUnderMouseCursor();
            if (underMouseCursorInfo.collider != null && underMouseCursorInfo.collider.GetComponent<Cost>())
            {
                Cost costToShow = underMouseCursorInfo.collider.GetComponent<Cost>();
                CostGUI.Instance.ShowCostGUI(costToShow.goldCost, costToShow.lumberCost, costToShow.foodCost);
            }
            else if (CostGUI.Instance.IsVisible)
            {
                CostGUI.Instance.HideCostGUI();
            }
        }
    }

    public void PlaceBuilding(BuildingType buildingType)
    {
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, selectedUnit.owner).GetComponent<Building>();
        if (buildingToBuild.goldCost > MultiplayerController.Instance.players.Find(item => item.playerType == selectedUnit.owner).goldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", selectedUnit.owner);
        }
        else
        {
            ((Worker)selectedUnit).HideBuildButtons();
            isSelectingBuildingPlace = true;
            this.buildingToBuild = Instantiate(buildingToBuild);
            this.buildingToBuild.builder = (Worker)selectedUnit;
        }
    }

    public RaycastHit2D GetWhatIsUnderMouseCursor()
    {
        return Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
    }

    public void SelectItem(Vector2 mousePositionInWorld)
    {
        RaycastHit2D selectionInfo = Physics2D.GetRayIntersection(new Ray((Vector3)mousePositionInWorld - Vector3.forward, Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
        if (selectionInfo.collider != null)
        {
            if (selectionInfo.collider.transform.parent.GetComponent<Unit>())
            {
                SelectUnit(selectionInfo.collider.transform.parent.GetComponent<Unit>());
            }
            else if (selectionInfo.collider.transform.parent.GetComponent<Building>())
            {
                SelectBuilding(selectionInfo.collider.transform.parent.GetComponent<Building>());
            }
            else if (selectionInfo.collider.transform.parent.GetComponent<Mine>())
            {
                SelectMine(selectionInfo.collider.transform.parent.GetComponent<Mine>());
            }
        }
    }

    public void SelectBuilding(Building building)
    {
        selectedBuilding = null;
        selectedUnit = null;
        selectedMine = null;
        selectedBuilding = building;
        RpcSelectBuilding(building.GetComponent<NetworkIdentity>());
    }

    public void SelectUnit(Unit unit)
    {
        selectedBuilding = null;
        selectedUnit = null;
        selectedMine = null;
        selectedUnit = unit;
        RpcSelectUnit(unit.GetComponent<NetworkIdentity>());
    }

    public void SelectMine(Mine mine)
    {
        selectedBuilding = null;
        selectedUnit = null;
        selectedMine = null;
        selectedMine = mine;
        RpcSelectMine(mine.GetComponent<NetworkIdentity>());
    }

    public void Unselect()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.Unselect();
            selectedBuilding = null;
        }
        if (selectedUnit != null)
        {
            selectedUnit.Unselect();
            selectedUnit = null;
        }
        if (selectedMine != null)
        {
            selectedMine.Unselect();
            selectedMine = null;
        }
        ActionButtons.Instance.HideAllButtons();
    }

    [ClientRpc]
    void RpcSelectUnit(NetworkIdentity networkIdentity)
    {
        Unit unitToSelect = networkIdentity.GetComponent<Unit>();
        if (MultiplayerController.Instance.localPlayer.selectController == this)
        {
            Unselect();
            selectedUnit = unitToSelect;
            unitToSelect.Select();
        }
    }

    [ClientRpc]
    void RpcSelectBuilding(NetworkIdentity networkIdentity)
    {
        Building buildingToSelect = networkIdentity.GetComponent<Building>();
        if (MultiplayerController.Instance.localPlayer.selectController == this)
        {
            Unselect();
            selectedBuilding = buildingToSelect;
            buildingToSelect.Select();
        }
    }

    [ClientRpc]
    void RpcSelectMine(NetworkIdentity networkIdentity)
    {
        Mine mineToSelect = networkIdentity.GetComponent<Mine>();
        if (MultiplayerController.Instance.localPlayer.selectController == this)
        {
            Unselect();
            selectedMine = mineToSelect;
            mineToSelect.Select();
        }
    }

    public IntVector2 GetGridPositionFromMousePosition()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public Vector2 GetGriddedWorldPositionFromMousePosition()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
