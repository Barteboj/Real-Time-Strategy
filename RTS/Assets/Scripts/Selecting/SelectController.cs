using UnityEngine;
using System.Collections;

public class SelectController : MonoBehaviour
{
    private static SelectController instance;

    public Unit selectedUnit;
    public Building selectedBuilding;

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
                    if (hitInfo.collider != null)
                    {
                        if (hitInfo.collider.transform.parent.GetComponent<Mine>())
                        {
                            ((Worker)selectedUnit).GoForGold(hitInfo.collider.transform.parent.GetComponent<Mine>());
                        }
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
    }

    public void buildWithSelectedUnit()
    {

    }

    public void SelectItem()
    {
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Select"));
        if (hitInfo.collider != null)
        {
            if (hitInfo.collider.transform.parent.GetComponent<Unit>())
            {
                if (selectedUnit != null)
                {
                    selectedUnit.Unselect();
                }
                selectedUnit = hitInfo.collider.transform.parent.GetComponent<Unit>();
                if (selectedBuilding != null)
                {
                    selectedBuilding.Unselect();
                    selectedBuilding = null;
                }
                hitInfo.collider.transform.parent.GetComponent<Unit>().Select();
            }
            else
            {
                if (selectedBuilding != null)
                {
                    selectedBuilding.Unselect();
                }
                selectedBuilding = hitInfo.collider.transform.parent.GetComponent<Building>();
                if (selectedUnit != null)
                {
                    selectedUnit.Unselect();
                    selectedUnit = null;
                }
                selectedBuilding.Select();
            }
        }
    }

    public void SelectBuilding(Building building)
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.Unselect();
        }
        if (selectedUnit != null)
        {
            selectedUnit.Unselect();
            selectedUnit = null;
        }
        selectedBuilding = building;
        building.Select();
    }

    public void Unselect()
    {
        selectedUnit = null;
        selectedBuilding = null;
        SelectionInfoKeeper.Instance.Hide();
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
