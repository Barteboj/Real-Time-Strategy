using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectionInfoKeeper : MonoBehaviour
{
    public Text unitName;
    public Text unitLevel;
    public Image unitPortrait;
    public GameObject viewGameObject;
    public Text actualHealth;
    public Text maxHealth;
    public Button[] buttons;

    private static SelectionInfoKeeper instance;

    public static SelectionInfoKeeper Instance
    {
        get
        {
            if (instance == null)
            {
                if (FindObjectOfType<SelectionInfoKeeper>())
                {
                    instance = FindObjectOfType<SelectionInfoKeeper>();
                    return instance;
                }
                else
                {
                    Debug.LogError("SelectionInfoKeeper instance not added to scene and is tried to be obtained");
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
            Debug.LogError("More than one instance of SelectionInfoKeeper destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void Show()
    {
        viewGameObject.SetActive(true);
    }

    public void Hide()
    {
        viewGameObject.SetActive(false);
    }

    public void Assign(Unit unit)
    {
        unitName.text = unit.unitName;
        unitLevel.text = unit.level.ToString();
        unitPortrait.sprite = unit.portrait;
        actualHealth.text = unit.actualHealth.ToString();
        maxHealth.text = unit.maxHealth.ToString();
        //((Image)buttons[0].targetGraphic).sprite = unit.actionButton.ButtonImage;
        //buttons[0].onClick.RemoveAllListeners();
        //buttons[0].onClick.AddListener(unit.actionButton)
    }
}
