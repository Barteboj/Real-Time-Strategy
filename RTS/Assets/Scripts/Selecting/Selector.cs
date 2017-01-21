using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Selector : NetworkBehaviour
{
    [SerializeField]
    private SpriteRenderer selectionHighlight;
    private Vector2 startSelectionPosition;

    private List<Unit> selectedUnits = new List<Unit>();
    public List<Unit> SelectedUnits
    {
        get
        {
            return selectedUnits;
        }
    }
    public Building SelectedBuilding { get; set; }
    public Mine SelectedMine { get; set; }

    private List<Unit> actualTestSelectedUnits = new List<Unit>();
    private Building actualTestSelectedBuilding;
    private Mine actualTestSelectedMine;
    private bool wasShiftPressedOnActualTest = false;

    private PlayerOnline player;
    private bool isSelecting = false;

    private Coroutine turnOffSelectorCoroutine;

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
            if (Input.GetMouseButtonUp(0) && selectionHighlight.enabled && MultiplayerController.Instance.IsGameInitialized)
            {
                wasShiftPressedOnActualTest = Input.GetKey(KeyCode.LeftShift);
                CmdTestSelection(startSelectionPosition, selectionHighlight.transform.localScale, wasShiftPressedOnActualTest);
                selectionHighlight.enabled = false;
            }
            if (CheckIfIsInSelectionArea() && !player.Commander.IsSelectingBuildingPlace && MultiplayerController.Instance.IsGameInitialized)
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
            if (MultiplayerController.Instance.IsGameInitialized)
            {
                RaycastHit2D underMouseCursorInfo = GetWhatIsUnderMouseCursor();
                if (underMouseCursorInfo.collider != null && underMouseCursorInfo.collider.GetComponent<TrainingButton>())
                {
                    TrainingButton buttonUnderMouse = underMouseCursorInfo.collider.GetComponent<TrainingButton>();
                    Unit unitToViewCost = Units.Instance.UnitsList.Find(item => item.UnitType == buttonUnderMouse.UnitType);
                    CostGUI.Instance.ShowCostGUI(unitToViewCost.GoldCost, unitToViewCost.LumberCost, unitToViewCost.FoodCost);
                }
                else if (underMouseCursorInfo.collider != null && underMouseCursorInfo.collider.GetComponent<BuildButton>())
                {
                    BuildButton buttonUnderMouse = underMouseCursorInfo.collider.GetComponent<BuildButton>();
                    Building buildingToViewCost = Buildings.Instance.BuildingsList.Find(item => item.BuildingType == buttonUnderMouse.BuildingType);
                    CostGUI.Instance.ShowCostGUI(buildingToViewCost.GoldCost, buildingToViewCost.LumberCost, 0);
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
            if (actualTestSelectedUnits.Count > 0 && !(wasShiftPressedOnActualTest && selectedUnits.Find(item => item.Owner != player.PlayerType)))
            {
                if (wasShiftPressedOnActualTest && actualTestSelectedUnits.Count == 1 && selectedUnits.Contains(actualTestSelectedUnits[0]) && selectionHighlight.transform.localScale.x < 0.3f && selectionHighlight.transform.localScale.y < 0.3f)
                {
                    Unselect(actualTestSelectedUnits[0]);
                }
                else
                {
                    foreach (Unit actualSelectedUnit in actualTestSelectedUnits)
                    {
                        if (!selectedUnits.Contains(actualSelectedUnit) && actualSelectedUnit.Owner == player.PlayerType)
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
        if (SelectedBuilding != null)
        {
            Unselect(SelectedBuilding);
        }
        if (SelectedMine != null)
        {
            Unselect(SelectedMine);
        }
    }

    public void Select(Unit unitToSelect)
    {
        if (selectedUnits.Count < SelectionInfoKeeper.Instance.Selections.Count)
        {
            selectedUnits.Add(unitToSelect);
            if (MultiplayerController.Instance.LocalPlayer.PlayerType == player.PlayerType)
            {
                ActionButtons.Instance.HideAllButtons();
                Selection selectionToSelect = SelectionInfoKeeper.Instance.Selections.Find(item => !item.IsSomethingSelected);
                selectionToSelect.Select(unitToSelect);
                unitToSelect.SelectionIndicator.SetActive(true);
                if (selectedUnits.Count == 1)
                {
                    SelectionInfoKeeper.Instance.Selections.Find(item => item.IsSomethingSelected).Unselect();
                    SelectionInfoKeeper.Instance.Selections[0].Select(unitToSelect);
                    SelectionInfoKeeper.Instance.UnitName.text = unitToSelect.name;
                    SelectionInfoKeeper.Instance.UnitName.enabled = true;
                    if (MultiplayerController.Instance.LocalPlayer.PlayerType == unitToSelect.Owner)
                    {
                        player.ActionButtonsController.ShowButtons(unitToSelect);
                    }
                }
                else
                {
                    player.ActionButtonsController.HideAllButtons();
                    SelectionInfoKeeper.Instance.UnitName.enabled = false;
                }
                if (MultiplayerController.Instance.LocalPlayer.PlayerType == unitToSelect.Owner)
                {
                    unitToSelect.SelectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
                }
                else
                {
                    unitToSelect.SelectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                }
            }
        }
    }

    public void Select(Building buildingToSelect)
    {
        SelectedBuilding = buildingToSelect;
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == player.PlayerType)
        {
            if (MultiplayerController.Instance.LocalPlayer.PlayerType == buildingToSelect.Owner)
            {
                buildingToSelect.SelectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
            }
            else
            {
                buildingToSelect.SelectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.red;
            }
            buildingToSelect.SelectionIndicator.SetActive(true);
            Selection selectionToSelect = SelectionInfoKeeper.Instance.Selections[0];
            selectionToSelect.Select(buildingToSelect);
            SelectionInfoKeeper.Instance.UnitName.text = buildingToSelect.BuildingName;
            SelectionInfoKeeper.Instance.UnitName.enabled = true;
            if (MultiplayerController.Instance.LocalPlayer.PlayerType == buildingToSelect.Owner)
            {
                if (buildingToSelect.IsBuilded && !buildingToSelect.IsTraining)
                {
                    player.ActionButtonsController.ShowButtons(buildingToSelect);
                }
                if (buildingToSelect.IsTraining)
                {
                    SelectionInfoKeeper.Instance.TrainingUnitGameObject.SetActive(true);
                }
                if (buildingToSelect.ActualBuildTime < buildingToSelect.BuildTime)
                {
                    SelectionInfoKeeper.Instance.ShowBuildCompletitionBar();
                }
            }
        }
    }

    public void Select(Mine mineToSelect)
    {
        SelectedMine = mineToSelect;
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == player.PlayerType)
        {
            mineToSelect.SelectionIndicator.SetActive(true);
            Selection selectionToSelect = SelectionInfoKeeper.Instance.Selections[0];
            selectionToSelect.Select(mineToSelect);
            SelectionInfoKeeper.Instance.UnitName.text = Mine.mineName;
            SelectionInfoKeeper.Instance.UnitName.enabled = true;
            SelectionInfoKeeper.Instance.GoldLeftAmountText.text = mineToSelect.GoldLeft.ToString();
            SelectionInfoKeeper.Instance.GoldLeftInfoGameObject.SetActive(true);
        }
    }

    public void Unselect(Unit unitToUnselect)
    {
        selectedUnits.Remove(unitToUnselect);
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == player.PlayerType)
        {
            unitToUnselect.SelectionIndicator.SetActive(false);
            SelectionInfoKeeper.Instance.Selections.Find(item => item.SelectedUnit == unitToUnselect).Unselect();
            if (selectedUnits.Count == 1)
            {
                Unit unitToChangeSelectionPosition = SelectionInfoKeeper.Instance.Selections.Find(item => item.IsSomethingSelected).SelectedUnit;
                SelectionInfoKeeper.Instance.Selections.Find(item => item.IsSomethingSelected).Unselect();
                SelectionInfoKeeper.Instance.Selections[0].Select(unitToChangeSelectionPosition);
                SelectionInfoKeeper.Instance.UnitName.text = unitToChangeSelectionPosition.UnitName;
                SelectionInfoKeeper.Instance.UnitName.enabled = true;
                if (MultiplayerController.Instance.LocalPlayer.PlayerType == unitToChangeSelectionPosition.Owner)
                {
                    player.ActionButtonsController.ShowButtons(unitToChangeSelectionPosition);
                }
            }
            else
            {
                SelectionInfoKeeper.Instance.UnitName.enabled = false;
                player.ActionButtonsController.HideButtons(unitToUnselect);
            }
        }
    }

    public void Unselect(Building buildingToUnselect)
    {
        SelectedBuilding = null;
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == player.PlayerType)
        {
            buildingToUnselect.SelectionIndicator.SetActive(false);
            SelectionInfoKeeper.Instance.Selections.Find(item => item.SelectedBuilding == buildingToUnselect).Unselect();
            SelectionInfoKeeper.Instance.UnitName.enabled = false;
            player.ActionButtonsController.HideButtons(buildingToUnselect);
            SelectionInfoKeeper.Instance.TrainingUnitGameObject.SetActive(false);
            SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
        }
    }

    public void Unselect(Mine mineToUnselect)
    {
        SelectedMine = null;
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == player.PlayerType)
        {
            mineToUnselect.SelectionIndicator.SetActive(false);
            SelectionInfoKeeper.Instance.Selections.Find(item => item.SelectedMine == mineToUnselect).Unselect();
            SelectionInfoKeeper.Instance.UnitName.enabled = false;
            SelectionInfoKeeper.Instance.GoldLeftInfoGameObject.SetActive(false);
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
