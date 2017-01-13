using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SelectionInfoKeeper : MonoBehaviour
{
    public Text unitName;

    public GameObject buildCompletitionBar;
    public Image buildCompletitionBarFill;

    public GameObject trainingUnitGameObject;
    public Image trainingCompletitionBarFill;
    public Image trainedUnitPortrait;

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

    public void SetTrainingBar(float fillAmount)
    {
        trainingCompletitionBarFill.fillAmount = fillAmount;
    }
}
