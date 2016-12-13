using UnityEngine;
using System.Collections;

public class SelectController : MonoBehaviour
{
    private static SelectController instance;

    public Unit selectedUnit;
    public Building selectedBuilding;
    public Mine selectedMine;

    public static SelectController Instance
    {
        get
        {
            if (instance == null)
            {
                if (FindObjectOfType<SelectController>())
                {
                    instance = FindObjectOfType<SelectController>();
                    return instance;
                }
                else
                {
                    Debug.LogError("SelectController instance not added to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instance of SelectController destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1) && selectedUnit != null && MapGridded.Instance.IsInMap(GetGridPositionFromMousePosition()))
        {
            if (selectedUnit.GetType() == typeof(Worker))
            {
                if (!((Worker)selectedUnit).isSelectingPlaceForBuilding)
                {
                    ((Worker)selectedUnit).CancelBuild();
                    ((Worker)selectedUnit).CancelGatheringGold();
                    RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
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
                        selectedUnit.RequestGoTo(GetGridPositionFromMousePosition());
                    }
                }
                else
                {
                    ((Worker)selectedUnit).CancelBuild();
                }
            }
            else if (selectedUnit.GetType() == typeof(Warrior))
            {
                RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
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
                    selectedUnit.RequestGoTo(GetGridPositionFromMousePosition());
                }
            }
            else
            {
                selectedUnit.RequestGoTo(GetGridPositionFromMousePosition());
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedUnit != null && selectedUnit.GetType() == typeof(Worker))
            {
                if (!((Worker)selectedUnit).isSelectingPlaceForBuilding)
                {
                    SelectItem();
                }
            }
            else
            {
                SelectItem();
            }
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

    public RaycastHit2D GetWhatIsUnderMouseCursor()
    {
        return Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
    }

    public void SelectItem()
    {
        RaycastHit2D selectionInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
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
        Unselect();
        selectedBuilding = building;
        building.Select();
    }

    public void SelectUnit(Unit unit)
    {
        Unselect();
        selectedUnit = unit;
        unit.Select();
    }

    public void SelectMine(Mine mine)
    {
        Unselect();
        selectedMine = mine;
        mine.Select();
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
