using UnityEngine;
using System.Collections;

public class SelectController : MonoBehaviour
{
    private static SelectController instance;

    public Unit selectedUnit;

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
        if (Input.GetMouseButtonUp(1) && selectedUnit != null)
        {
            selectedUnit.RequestGoTo(GetGridPositionFromMousePosition());
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SelectItem();
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
            selectedUnit = hitInfo.collider.transform.parent.GetComponent<Unit>();
            hitInfo.collider.transform.parent.GetComponent<Unit>().Select();
        }
    }

    public IntVector2 GetGridPositionFromMousePosition()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
