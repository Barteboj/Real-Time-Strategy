using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectControllerTest : MonoBehaviour
{
    public SpriteRenderer selectionHighlight;
    public Vector2 startSelectionPosition;
    public BoxCollider2D selectionCollider;
    public SelectGUIControllerTest selectGuiController;

    public bool isSelectionTested = false;
    public bool isFirstSelection = true;

    public List<SelectableObjectTest> selectedObjects = new List<SelectableObjectTest>();

    private void Awake()
    {
        selectGuiController = FindObjectOfType<SelectGUIControllerTest>();
    }

    public bool IsObjectAlreadySelected(SelectableObjectTest objectToCheck)
    {
        return selectGuiController.selections.Find(item => item.selectedObject == objectToCheck) != null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SelectableObjectTest objectToSelect = collision.GetComponent<SelectableObjectTest>();
        if (objectToSelect != null)
        {
            if (isFirstSelection)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    UnSelectAll();
                }
                isFirstSelection = false;
            }
            if (IsObjectAlreadySelected(objectToSelect) && Input.GetKey(KeyCode.LeftShift) && transform.localScale.x < 0.1f && transform.localScale.y < 0.1f)
            {
                Unselect(objectToSelect);
            }
            else if (!IsObjectAlreadySelected(objectToSelect))
            {
                Select(objectToSelect);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionHighlight.enabled = true;
            startSelectionPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectionHighlight.transform.position = startSelectionPosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            TestSelection();
            selectionHighlight.enabled = false;
        }
        if (Input.GetMouseButton(0))
        {
            selectionHighlight.transform.localScale = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - startSelectionPosition.x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y - startSelectionPosition.y, 1f);
        }
    }

    public IEnumerator TurnOffSelector()
    {
        yield return new WaitForFixedUpdate();
        selectionCollider.enabled = false;
    }

    public void TestSelection()
    {
        if (transform.localScale.x == 0)
        {
            transform.localScale += new Vector3(0.01f, 0, 0);
        }
        if (transform.localScale.y == 0)
        {
            transform.localScale += new Vector3(0, 0.01f, 0);
        }
        isFirstSelection = true;
        selectionCollider.enabled = true;
        StartCoroutine(TurnOffSelector());
    }

    public void UnSelectAll()
    {
        while(selectedObjects.Count > 0)
        {
            Unselect(selectedObjects[0]);
        }
    }

    public void Select(SelectableObjectTest objectToSelect)
    {
        selectedObjects.Add(objectToSelect);
        SelectionTest selectionToSelect = selectGuiController.selections.Find(item => !item.IsSomethingSelected);
        if (selectionToSelect != null)
        {
            selectionToSelect.Select(objectToSelect);
        }
        if (selectedObjects.Count == 1)
        {
            selectGuiController.selections.Find(item => item.IsSomethingSelected).Unselect();
            selectGuiController.selections[0].Select(objectToSelect);
            selectGuiController.nameText.text = objectToSelect.name;
            selectGuiController.nameText.enabled = true;
        }
        else
        {
            selectGuiController.nameText.enabled = false;
        }
    }

    public void Unselect(SelectableObjectTest objectToUnselect)
    {
        selectedObjects.Remove(objectToUnselect);
        selectGuiController.selections.Find(item => item.selectedObject == objectToUnselect).Unselect();
        if (selectedObjects.Count == 1)
        {
            SelectableObjectTest objectToSelect = selectGuiController.selections.Find(item => item.IsSomethingSelected).selectedObject;
            selectGuiController.selections.Find(item => item.IsSomethingSelected).Unselect();
            selectGuiController.selections[0].Select(objectToSelect);
            selectGuiController.nameText.text = selectedObjects[0].name;
            selectGuiController.nameText.enabled = true;
        }
        else
        {
            selectGuiController.nameText.enabled = false;
        }
    }
}
