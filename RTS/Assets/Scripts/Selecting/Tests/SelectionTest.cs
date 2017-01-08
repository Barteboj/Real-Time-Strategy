using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionTest : MonoBehaviour
{
    public Image portrait;
    public SelectableObjectTest selectedObject;

    public bool IsSomethingSelected
    {
        get
        {
            return gameObject.activeInHierarchy;
        }
    }

    public void Unselect()
    {
        gameObject.SetActive(false);
        if (selectedObject != null)
        {
            selectedObject.selectionIndicator.SetActive(false);
            selectedObject = null;
        }
    }

    public void Select(SelectableObjectTest objectToSelect)
    {
        portrait.sprite = objectToSelect.portrait;
        selectedObject = objectToSelect;
        selectedObject.selectionIndicator.SetActive(true);
        gameObject.SetActive(true);
    }
}
