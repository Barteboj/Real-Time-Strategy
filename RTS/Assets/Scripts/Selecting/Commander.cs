using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Commander : NetworkBehaviour
{
    private bool isSelectingBuildingPlace = false;
    public bool IsSelectingBuildingPlace
    {
        get
        {
            return isSelectingBuildingPlace;
        }
    }
    private Building buildingToBuild;

    private PlayerOnline player;

    private void Awake()
    {
        player = gameObject.GetComponent<PlayerOnline>();
    }

    [Command]
    void CmdRightClickCommand(Vector2 mousePositionInWorld, PlayerType commander)
    {
        IntVector2 mousePositionOnMap = MapGridded.WorldToMapPosition(mousePositionInWorld);
        if (player.Selector.SelectedUnits.Count > 0 && MapGridded.Instance.IsInMap(mousePositionOnMap) && player.Selector.SelectedUnits[0].Owner == commander)
        {
            foreach (Unit selectedUnit in player.Selector.SelectedUnits)
            {
                if (selectedUnit.GetType() == typeof(Worker))
                {
                    if (!((Worker)selectedUnit).IsSelectingPlaceForBuilding)
                    {
                        ((Worker)selectedUnit).CancelBuild();
                        ((Worker)selectedUnit).CancelGatheringGold();
                        ((Worker)selectedUnit).CancelGatheringLumber();
                        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray((Vector3)mousePositionInWorld - Vector3.forward, Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
                        if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Mine>() && ((Worker)selectedUnit).TakenGoldAmount == 0 && ((Worker)selectedUnit).TakenLumberAmount == 0)
                        {
                            ((Worker)selectedUnit).GoForGold(hitInfo.collider.transform.parent.GetComponent<Mine>());
                        }
                        else if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<LumberInGame>() && !hitInfo.collider.transform.parent.GetComponent<LumberInGame>().IsDepleted && ((Worker)selectedUnit).TakenGoldAmount == 0 && ((Worker)selectedUnit).TakenLumberAmount == 0)
                        {
                            ((Worker)selectedUnit).GoForLumber();
                        }
                        else if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Building>() && hitInfo.collider.transform.parent.GetComponent<Building>().BuildingType == BuildingType.Castle && hitInfo.collider.transform.parent.GetComponent<Building>().Owner == selectedUnit.Owner)
                        {
                            if (((Worker)selectedUnit).TakenGoldAmount > 0)
                            {
                                ((Worker)selectedUnit).ReturnWithGold();
                            }
                            else if (((Worker)selectedUnit).TakenLumberAmount > 0)
                            {
                                ((Worker)selectedUnit).ReturnWithLumber();
                            }
                            
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
                    if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Unit>() && hitInfo.collider.transform.parent.GetComponent<Unit>().Owner != selectedUnit.Owner)
                    {
                        ((Warrior)selectedUnit).StartAttack(hitInfo.collider.transform.parent.GetComponent<Unit>());
                    }
                    else if (hitInfo.collider != null && hitInfo.collider.transform.parent.GetComponent<Building>() && hitInfo.collider.transform.parent.GetComponent<Building>().Owner != selectedUnit.Owner)
                    {
                        ((Warrior)selectedUnit).StartAttack(hitInfo.collider.transform.parent.GetComponent<Building>());
                    }
                    else
                    {
                        if (((Warrior)selectedUnit).IsAttacking)
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
        ((Worker)player.Selector.SelectedUnits[0]).GoToBuildPlace(buildingType, buildPlaceInWorldSpace);
    }

    void Update()
    {
        if (!hasAuthority || SceneManager.GetActiveScene().name != "Game" || MapGridded.Instance.MapGrid == null)
        {
            return;
        }
        if (isSelectingBuildingPlace && Selector.CheckIfIsInSelectionArea())
        {
            Vector2 griddedPosition = new Vector2(Selector.GetGridPositionFromMousePosition().X, Selector.GetGridPositionFromMousePosition().Y);
            buildingToBuild.transform.position = Selector.GetGriddedWorldPositionFromMousePosition();
            buildingToBuild.ShowBuildGrid();
            if (Input.GetMouseButtonUp(0))
            {
                if (buildingToBuild.CouldBeBuildInPlace(MapGridded.WorldToMapPosition(buildingToBuild.transform.position), player.Selector.SelectedUnits[0]))
                {
                    isSelectingBuildingPlace = false;
                    buildingToBuild.HideBuildGrid();
                    CmdOrderUnitToBuild(buildingToBuild.BuildingType, buildingToBuild.transform.position);
                    Destroy(buildingToBuild.gameObject);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isSelectingBuildingPlace = false;
                buildingToBuild.HideBuildGrid();
                Destroy(buildingToBuild.gameObject);
                player.ActionButtonsController.ShowButtons(player.Selector.SelectedUnits[0]);
            }
        }
        else
        {
            if (Selector.CheckIfIsInSelectionArea())
            {
                if (Input.GetMouseButtonUp(1) && player.Selector.SelectedUnits.Count > 0 && MapGridded.Instance.IsInMap(Selector.GetGridPositionFromMousePosition()) && player.Selector.SelectedUnits[0].Owner == MultiplayerController.Instance.LocalPlayer.PlayerType)
                {
                    CmdRightClickCommand(Camera.main.ScreenToWorldPoint(Input.mousePosition), MultiplayerController.Instance.LocalPlayer.PlayerType);
                }
            }
        }
    }

    public void PlaceBuilding(BuildingType buildingType)
    {
        Building buildingToBuild = Buildings.Instance.GetBuildingPrefab(buildingType, player.Selector.SelectedUnits[0].Owner).GetComponent<Building>();
        if (buildingToBuild.GoldCost > MultiplayerController.Instance.Players.Find(item => item.PlayerType == player.Selector.SelectedUnits[0].Owner).GoldAmount)
        {
            MessagesController.Instance.RpcShowMessage("Not enough gold", player.Selector.SelectedUnits[0].Owner);
        }
        else
        {
            player.ActionButtonsController.HideButtons(player.Selector.SelectedUnits[0]);
            isSelectingBuildingPlace = true;
            this.buildingToBuild = Instantiate(buildingToBuild);
            this.buildingToBuild.Builder = (Worker)player.Selector.SelectedUnits[0];
        }
    }
}
