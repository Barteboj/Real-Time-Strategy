using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SelectionInfoKeeper : MonoBehaviour
{
    public Text unitName;
    public Text unitLevelValueText;
    public Image unitPortrait;
    public GameObject viewGameObject;
    public Text actualHealth;
    public Text maxHealth;
    public Button[] buttons;

    public GameObject buildCompletitionBar;
    public Image buildCompletitionBarFill;

    public GameObject trainingUnitGameObject;
    public Image trainingCompletitionBarFill;
    public Image trainedUnitPortrait;

    public Image healthBar;

    public GameObject healthInfoGameObject;
    public GameObject levelInfoGameObject;
    public GameObject goldLeftInfoGameObject;

    public Text goldLeftAmountText;

    public List<Selection> selections;

    private static SelectionInfoKeeper instance;

    public static SelectionInfoKeeper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SelectionInfoKeeper>();
            }
            return instance;
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
        unitLevelValueText.text = unit.level.ToString();
        unitPortrait.sprite = unit.portrait;
        actualHealth.text = unit.actualHealth.ToString();
        maxHealth.text = unit.maxHealth.ToString();
    }

    public void Assign(Building building)
    {
        unitName.text = building.buildingName;
        unitLevelValueText.text = building.level.ToString();
        unitPortrait.sprite = building.portrait;
        actualHealth.text = building.actualHealth.ToString();
        maxHealth.text = building.maxHealth.ToString();
    }

    public void ShowBuildCompletitionBar()
    {
        buildCompletitionBar.SetActive(true);
    }

    public void HideBuildCompletitionBar()
    {
        buildCompletitionBar.SetActive(false);
    }

    public void ShowTrainingInfo()
    {
        trainingUnitGameObject.SetActive(true);
    }

    public void HideTrainingInfo()
    {
        trainingUnitGameObject.SetActive(false);
    }

    public void SetCompletitionBar(float fillAmount)
    {
        buildCompletitionBarFill.fillAmount = fillAmount;
    }

    public void SetHealthBar(float fillAmount)
    {
        healthBar.fillAmount = fillAmount;
    }

    public void SetTrainingBar(float fillAmount)
    {
        trainingCompletitionBarFill.fillAmount = fillAmount;
    }
}
