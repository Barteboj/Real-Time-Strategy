using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Commander : NetworkBehaviour
{
    public bool isSelectingBuildingPlace = false;
    public Building buildingToBuild;

    private PlayerOnline player;

    private void Awake()
    {
        player = gameObject.GetComponent<PlayerOnline>();
    }

    [Command]
    void CmdRightClickCommand(Vector2 mousePositionInWorld, PlayerType commander)
    {
        IntVector2 mousePositionOnMap = MapGridded.WorldToMapPosition(mousePositionInWorld);
        if (player.selector.selectedUnits.Count > 0 && MapGridded.Instance.IsInMap(mousePositionOnMap) && player.selector.selectedUnits[0].owner == commander)
        {
            foreach (Unit selectedUnit in player.selector.selectedUnits)
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
                            ((Worker)selectedUnit).GoForLumber();
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
                    if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Unit>() && hitInfo.collider.transform.parent.GetComponent<Unit>().owner != selectedUnit.owner)
                    {
                        ((Warrior)selectedUnit).StartAttack(hitInfo.collider.transform.parent.GetComponent<Unit>());
                    }
                    else if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Building>() && hitInfo.collider.transform.parent.GetComponent<Building>().owner != selectedUnit.owner)
                    {
                        ((Warrior)selectedUnit).StartAttack(hitInfo.collider.transform.parent.GetComponent<Building>());
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
    }

    [Command]
    void CmdOrderUnitToBuild(BuildingType buildingType, Vector2 buildPlaceInWorldSpace)
    {
        ((Worker)player.selector.selectedUnits[0]).GoToBuildPlace(buildingType, buildPlaceInWorldSpace);
    }

    void Update()
    {
        if (!hasAuthority || SceneManager.GetActiveScene().name != "Game" || MapGridded.Instance.mapGrid == null)
        {
            return;
        }
        if (isSelectingBuildingPlace && Selector.CheckIfIsInSelectionArea())
        {
            Vector2 griddedPosition = new Vector2(Selector.GetGridPositionFromMousePosition().x, Selector.GetGridPositionFromMousePosition().y);
            buildingToBuild.transform.position = Selector.GetGriddedWorldPositionFromMousePosition();
            buildingToBuild.ShowBuildGrid();
            if (Input.GetMouseButtonUp(0))
            {
                if (buildingToBuild.CouldBeBuildInPlace(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), player.selector.selectedUnits[0]))
                {
                    isSelectingBuildingPlace = false;
                    buildingToBuild.HideBuildGrid();
                    CmdOrderUnitToBuild(buildingToBuild.buildingType, buildingToBuild.transform.position);
                    Destroy(buildingToBuild.gameObject);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isSelectingBuildingPlace = false;
                buildingToBuild.HideBuildGrid();
                Destroy(buildingToBuild.gameObject);
                player.actionButtonsController.ShowButtons(player.selector.selectedUnits[0]);
            }
        }
        else
        {
            if (Selector.CheckIfIsInSelectionArea())
            {
                if (Input.GetMouseButtonUp(1) && player.selector.selectedUnits.Count > 0 && MapGridded.Instance.IsInMap(Selector.GetGridPositionFromMousePosition()) && player.selector.selectedUnits[0].owner == MultiplayerController.Instance.localPlayer.playerType)
                {
                    CmdRightClickCommand(Camera.main.ScreenToWorldPoint(Input.mousePosition), MultiplayerController.Instance.localPlayer.playerType);
                }
            }
        }
    }

    public void PlaceBuilding(BuildingType buildingType)
    {
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, player.selector.selectedUnits[0].owner).GetComponent<Building>();
        if (buildingToBuild.goldCost > MultiplayerController.Instance.players.Find(item => item.playerType == player.selector.selectedUnits[0].owner).goldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", player.selector.selectedUnits[0].owner);
        }
        else
        {
            player.actionButtonsController.HideButtons(player.selector.selectedUnits[0]);
            isSelectingBuildingPlace = true;
            this.buildingToBuild = Instantiate(buildingToBuild);
            this.buildingToBuild.builder = (Worker)player.selector.selectedUnits[0];
        }
    }
}
