using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionButtons : MonoBehaviour
{
    private static ActionButtons instance;

    public static ActionButtons Instance
    {
        get
        {
            if (instance == null)
            {
                ActionButtons searchedActionButtons;
                if (searchedActionButtons = FindObjectOfType<ActionButtons>())
                {
                    instance = searchedActionButtons;
                    return instance;
                }
                else
                {
                    Debug.LogError("No instance of ActionButtons and it is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    [SerializeField]
    public List<ActionButton> buttons;
    private List<ActionButton> Buttons
    {
        get
        {
            return buttons;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instance of ActionButtons destroying excessive one");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void HideAllButtons()
    {
        foreach (ActionButton button in buttons)
        {
            button.Hide();
        }
    }
}
