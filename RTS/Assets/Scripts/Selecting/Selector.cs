using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Selector : NetworkBehaviour
{
    public SpriteRenderer selectionHighlight;
    public Vector2 startSelectionPosition;

    public List<Unit> selectedUnits = new List<Unit>();
    public Building selectedBuilding;
    public Mine selectedMine;

    public List<Unit> actualTestSelectedUnits = new List<Unit>();
    public Building actualTestSelectedBuilding;
    public Mine actualTestSelectedMine;
    public bool wasShiftPressedOnActualTest = false;

    private PlayerOnline player;
    private bool isSelecting = false;

    Coroutine turnOffSelectorCoroutine;

    private void Awake()
    {
        player = GetComponent<PlayerOnline>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isSelecting)
        {
            if (collision.transform.parent != null)
            {
                Unit selectedUnit = collision.transform.parent.GetComponent<Unit>();
                Building selectedBuilding = collision.transform.parent.GetComponent<Building>();
                Mine selectedMine = collision.transform.parent.GetComponent<Mine>();
                if (selectedUnit != null && !actualTestSelectedUnits.Contains(selectedUnit))
                {
                    actualTestSelectedUnits.Add(selectedUnit);
                }
                else if (selectedBuilding != null && actualTestSelectedBuilding != selectedBuilding)
                {
                    if (actualTestSelectedBuilding == null)
                    {
                        actualTestSelectedBuilding = selectedBuilding;
                    }
                }
                else if (selectedMine != null && actualTestSelectedMine != selectedMine)
                {
                    if (actualTestSelectedMine == null)
                    {
                        actualTestSelectedMine = selectedMine;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetMouseButtonUp(0) && selectionHighlight.enabled && MultiplayerController.Instance.isGameInitialized)
            {
                wasShiftPressedOnActualTest = Input.GetKey(KeyCode.LeftShift);
                CmdTestSelection(startSelectionPosition, selectionHighlight.transform.localScale, wasShiftPressedOnActualTest);
                selectionHighlight.enabled = false;
            }
            if (CheckIfIsInSelectionArea() && !player.commander.isSelectingBuildingPlace && MultiplayerController.Instance.isGameInitialized)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    selectionHighlight.enabled = true;
                    startSelectionPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    selectionHighlight.transform.position = startSelectionPosition;
                }
                if (Input.GetMouseButton(0))
                {
                    selectionHighlight.transform.localScale = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - startSelectionPosition.x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y - startSelectionPosition.y, 1f);
                }
            }
            if (MultiplayerController.Instance.isGameInitialized)
            {
                RaycastHit2D underMouseCursorInfo = GetWhatIsUnderMouseCursor();
                if (underMouseCursorInfo.collider != null && underMouseCursorInfo.collider.GetComponent<TrainingButton>())
                {
                    TrainingButton buttonUnderMouse = underMouseCursorInfo.collider.GetComponent<TrainingButton>();
                    Unit unitToViewCost = Units.Instance.unitsList.Find(item => item.unitType == buttonUnderMouse.unitType);
                    CostGUI.Instance.ShowCostGUI(unitToViewCost.goldCost, unitToViewCost.lumberCost, unitToViewCost.foodCost);
                }
                else if (underMouseCursorInfo.collider != null && underMouseCursorInfo.collider.GetComponent<BuildButton>())
                {
                    BuildButton buttonUnderMouse = underMouseCursorInfo.collider.GetComponent<BuildButton>();
                    Building buildingToViewCost = Buildings.Instance.buildingsList.Find(item => item.buildingType == buttonUnderMouse.buildingType);
                    CostGUI.Instance.ShowCostGUI(buildingToViewCost.goldCost, buildingToViewCost.lumberCost, 0);
                }
                else if (CostGUI.Instance.IsVisible)
                {
                    CostGUI.Instance.HideCostGUI();
                }
            }
        }
    }

    public IEnumerator TurnOffSelector()
    {
        yield return new WaitForSeconds(0.05f);
        NetworkInstanceId[] unitsNetworkIdentities = new NetworkInstanceId[actualTestSelectedUnits.Count];
        for (int i = 0; i < unitsNetworkIdentities.Length; ++i)
        {
            unitsNetworkIdentities[i] = actualTestSelectedUnits[i].GetComponent<NetworkIdentity>().netId;
        }
        RpcReactOnSelections(unitsNetworkIdentities, actualTestSelectedBuilding != null ? actualTestSelectedBuilding.GetComponent<NetworkIdentity>() : null, actualTestSelectedMine != null ? actualTestSelectedMine.GetComponent<NetworkIdentity>() : null, wasShiftPressedOnActualTest, selectionHighlight.transform.localScale);
        isSelecting = false;
        selectionHighlight.transform.position = -Vector3.one;
        selectionHighlight.transform.localScale = Vector3.zero;
    }

    [ClientRpc]
    public void RpcReactOnSelections(NetworkInstanceId[] units, NetworkIdentity building, NetworkIdentity mine, bool wasShiftPressed, Vector3 selectionSize)
    {
        selectionHighlight.transform.localScale = selectionSize;
        wasShiftPressedOnActualTest = wasShiftPressed;
        actualTestSelectedUnits.Clear();
        foreach (NetworkInstanceId unitNetworkIdentity in units)
        {
            actualTestSelectedUnits.Add(ClientScene.FindLocalObject(unitNetworkIdentity).GetComponent<Unit>());
        }
        actualTestSelectedBuilding = null;
        if (building != null)
        {
            actualTestSelectedBuilding = building.GetComponent<Building>();
        }
        actualTestSelectedMine = null;
        if (mine != null)
        {
            actualTestSelectedMine = mine.GetComponent<Mine>();
        }
        ReactOnSelections();
    }

    public void ReactOnSelections()
    {
        if (actualTestSelectedUnits.Count > 0 || actualTestSelectedBuilding != null || actualTestSelectedMine != null)
        {
            if (!wasShiftPressedOnActualTest)
            {
                UnSelectAll();
            }
            if (actualTestSelectedUnits.Count > 0 && !(wasShiftPressedOnActualTest && selectedUnits.Find(item => item.owner != player.playerType)))
            {
                if (wasShiftPressedOnActualTest && actualTestSelectedUnits.Count == 1 && selectedUnits.Contains(actualTestSelectedUnits[0]) && selectionHighlight.transform.localScale.x < 0.3f && selectionHighlight.transform.localScale.y < 0.3f)
                {
                    Unselect(actualTestSelectedUnits[0]);
                }
                else
                {
                    foreach (Unit actualSelectedUnit in actualTestSelectedUnits)
                    {
                        if (!selectedUnits.Contains(actualSelectedUnit) && actualSelectedUnit.owner == player.playerType)
                        {
                            Select(actualSelectedUnit);
                        }
                    }
                    if (selectedUnits.Count == 0 && !wasShiftPressedOnActualTest)
                    {
                        Select(actualTestSelectedUnits[0]);
                    }
                }
            }
            else if (!wasShiftPressedOnActualTest && actualTestSelectedBuilding != null)
            {
                Select(actualTestSelectedBuilding);
            }
            else if (!wasShiftPressedOnActualTest && actualTestSelectedMine != null)
            {
                Select(actualTestSelectedMine);
            }
        }
    }

    [Command]
    public void CmdTestSelection(Vector2 startSelectionPosition, Vector3 localScale, bool wasShiftPressed)
    {
        wasShiftPressedOnActualTest = wasShiftPressed;
        selectionHighlight.transform.position = startSelectionPosition;
        selectionHighlight.transform.localScale = localScale;
        actualTestSelectedBuilding = null;
        actualTestSelectedMine = null;
        actualTestSelectedUnits.Clear();
        if (transform.localScale.x == 0)
        {
            transform.localScale += new Vector3(0.01f, 0, 0);
        }
        if (transform.localScale.y == 0)
        {
            transform.localScale += new Vector3(0, 0.01f, 0);
        }
        isSelecting = true;
        if (turnOffSelectorCoroutine != null)
        {
            StopCoroutine(turnOffSelectorCoroutine);
        }
        turnOffSelectorCoroutine = StartCoroutine(TurnOffSelector());
    }

    public void UnSelectAll()
    {
        while (selectedUnits.Count > 0)
        {
            Unselect(selectedUnits[0]);
        }
        if (selectedBuilding != null)
        {
            Unselect(selectedBuilding);
        }
        if (selectedMine != null)
        {
            Unselect(selectedMine);
        }
    }

    public void Select(Unit unitToSelect)
    {
        if (selectedUnits.Count < SelectionInfoKeeper.Instance.selections.Count)
        {
            selectedUnits.Add(unitToSelect);
            if (MultiplayerController.Instance.localPlayer.playerType == player.playerType)
            {
                ActionButtons.Instance.HideAllButtons();
                Selection selectionToSelect = SelectionInfoKeeper.Instance.selections.Find(item => !item.IsSomethingSelected);
                selectionToSelect.Select(unitToSelect);
                unitToSelect.selectionIndicator.SetActive(true);
                if (selectedUnits.Count == 1)
                {
                    SelectionInfoKeeper.Instance.selections.Find(item => item.IsSomethingSelected).Unselect();
                    SelectionInfoKeeper.Instance.selections[0].Select(unitToSelect);
                    SelectionInfoKeeper.Instance.unitName.text = unitToSelect.name;
                    SelectionInfoKeeper.Instance.unitName.enabled = true;
                    if (MultiplayerController.Instance.localPlayer.playerType == unitToSelect.owner)
                    {
                        player.actionButtonsController.ShowButtons(unitToSelect);
                    }
                }
                else
                {
                    player.actionButtonsController.HideAllButtons();
                    SelectionInfoKeeper.Instance.unitName.enabled = false;
                }
                if (MultiplayerController.Instance.localPlayer.playerType == unitToSelect.owner)
                {
                    unitToSelect.selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
                }
                else
                {
                    unitToSelect.selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                }
            }
        }
    }

    public void Select(Building buildingToSelect)
    {
        selectedBuilding = buildingToSelect;
        if (MultiplayerController.Instance.localPlayer.playerType == player.playerType)
        {
            if (MultiplayerController.Instance.localPlayer.playerType == buildingToSelect.owner)
            {
                buildingToSelect.selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
            }
            else
            {
                buildingToSelect.selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.red;
            }
            buildingToSelect.selectionIndicator.SetActive(true);
            Selection selectionToSelect = SelectionInfoKeeper.Instance.selections[0];
            selectionToSelect.Select(buildingToSelect);
            SelectionInfoKeeper.Instance.unitName.text = buildingToSelect.buildingName;
            SelectionInfoKeeper.Instance.unitName.enabled = true;
            if (MultiplayerController.Instance.localPlayer.playerType == buildingToSelect.owner)
            {
                if (buildingToSelect.isBuilded && !buildingToSelect.isTraining)
                {
                    player.actionButtonsController.ShowButtons(buildingToSelect);
                }
                if (buildingToSelect.isTraining)
                {
                    SelectionInfoKeeper.Instance.trainingUnitGameObject.SetActive(true);
                }
                if (buildingToSelect.actualBuildTime < buildingToSelect.buildTime)
                {
                    SelectionInfoKeeper.Instance.ShowBuildCompletitionBar();
                }
            }
        }
    }

    public void Select(Mine mineToSelect)
    {
        selectedMine = mineToSelect;
        if (MultiplayerController.Instance.localPlayer.playerType == player.playerType)
        {
            mineToSelect.selectionIndicator.SetActive(true);
            Selection selectionToSelect = SelectionInfoKeeper.Instance.selections[0];
            selectionToSelect.Select(mineToSelect);
            SelectionInfoKeeper.Instance.unitName.text = Mine.mineName;
            SelectionInfoKeeper.Instance.unitName.enabled = true;
            SelectionInfoKeeper.Instance.goldLeftAmountText.text = mineToSelect.GoldLeft.ToString();
            SelectionInfoKeeper.Instance.goldLeftInfoGameObject.SetActive(true);
        }
    }

    public void Unselect(Unit unitToUnselect)
    {
        selectedUnits.Remove(unitToUnselect);
        if (MultiplayerController.Instance.localPlayer.playerType == player.playerType)
        {
            unitToUnselect.selectionIndicator.SetActive(false);
            SelectionInfoKeeper.Instance.selections.Find(item => item.selectedUnit == unitToUnselect).Unselect();
            if (selectedUnits.Count == 1)
            {
                Unit unitToChangeSelectionPosition = SelectionInfoKeeper.Instance.selections.Find(item => item.IsSomethingSelected).selectedUnit;
                SelectionInfoKeeper.Instance.selections.Find(item => item.IsSomethingSelected).Unselect();
                SelectionInfoKeeper.Instance.selections[0].Select(unitToChangeSelectionPosition);
                SelectionInfoKeeper.Instance.unitName.text = unitToChangeSelectionPosition.unitName;
                SelectionInfoKeeper.Instance.unitName.enabled = true;
                if (MultiplayerController.Instance.localPlayer.playerType == unitToChangeSelectionPosition.owner)
                {
                    player.actionButtonsController.ShowButtons(unitToChangeSelectionPosition);
                }
            }
            else
            {
                SelectionInfoKeeper.Instance.unitName.enabled = false;
                player.actionButtonsController.HideButtons(unitToUnselect);
            }
        }
    }

    public void Unselect(Building buildingToUnselect)
    {
        selectedBuilding = null;
        if (MultiplayerController.Instance.localPlayer.playerType == player.playerType)
        {
            buildingToUnselect.selectionIndicator.SetActive(false);
            SelectionInfoKeeper.Instance.selections.Find(item => item.selectedBuilding == buildingToUnselect).Unselect();
            SelectionInfoKeeper.Instance.unitName.enabled = false;
            player.actionButtonsController.HideButtons(buildingToUnselect);
            SelectionInfoKeeper.Instance.trainingUnitGameObject.SetActive(false);
            SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
        }
    }

    public void Unselect(Mine mineToUnselect)
    {
        selectedMine = null;
        if (MultiplayerController.Instance.localPlayer.playerType == player.playerType)
        {
            mineToUnselect.selectionIndicator.SetActive(false);
            SelectionInfoKeeper.Instance.selections.Find(item => item.selectedMine == mineToUnselect).Unselect();
            SelectionInfoKeeper.Instance.unitName.enabled = false;
            SelectionInfoKeeper.Instance.goldLeftInfoGameObject.SetActive(false);
        }
    }

    public static bool CheckIfIsInSelectionArea()
    {
        return Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Selection Area")).collider != null;
    }

    public static RaycastHit2D GetWhatIsUnderMouseCursor()
    {
        return Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
    }

    public static IntVector2 GetGridPositionFromMousePosition()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public static Vector2 GetGriddedWorldPositionFromMousePosition()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
